using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MyCat.Data.MyCatClient.Types;
using MyCat.Data.Protocol.Serialization;

namespace MyCat.Data.MyCatClient.CommandExecutors
{
	internal class StoredProcedureCommandExecutor : TextCommandExecutor
	{

		internal StoredProcedureCommandExecutor(MyCatCommand command)
			: base(command)
		{
			m_command = command;
		}

		public override async Task<DbDataReader> ExecuteReaderAsync(string commandText, MyCatParameterCollection parameterCollection,
			CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			var cachedProcedure = await m_command.Connection.GetCachedProcedure(ioBehavior, commandText, cancellationToken).ConfigureAwait(false);
			if (cachedProcedure != null)
				parameterCollection = cachedProcedure.AlignParamsWithDb(parameterCollection);

			MyCatParameter returnParam = null;
			m_outParams = new MyCatParameterCollection();
			m_outParamNames = new List<string>();
			var inParams = new MyCatParameterCollection();
			var argParamNames = new List<string>();
			var inOutSetParams = "";
			for (var i = 0; i < parameterCollection.Count; i++)
			{
				var param = parameterCollection[i];
				var inName = "@inParam" + i;
				var outName = "@outParam" + i;
				switch (param.Direction)
				{
					case 0:
					case ParameterDirection.Input:
					case ParameterDirection.InputOutput:
						var inParam = param.WithParameterName(inName);
						inParams.Add(inParam);
						if (param.Direction == ParameterDirection.InputOutput)
						{
							inOutSetParams += $"SET {outName}={inName}; ";
							goto case ParameterDirection.Output;
						}
						argParamNames.Add(inName);
						break;
					case ParameterDirection.Output:
						m_outParams.Add(param);
						m_outParamNames.Add(outName);
						argParamNames.Add(outName);
						break;
					case ParameterDirection.ReturnValue:
						returnParam = param;
						break;
				}
			}

			// if a return param is set, assume it is a funciton.  otherwise, assume stored procedure
			commandText += "(" + string.Join(", ", argParamNames) +")";
			if (returnParam == null)
			{
				commandText = inOutSetParams + "CALL " + commandText;
				if (m_outParams.Count > 0)
				{
					m_setParamsFlags = true;
					m_cancellationToken = cancellationToken;
				}
			}
			else
			{
				commandText = "SELECT " + commandText;
			}

			var reader = (MyCatDataReader) await base.ExecuteReaderAsync(commandText, inParams, behavior, ioBehavior, cancellationToken).ConfigureAwait(false);
			if (returnParam != null && await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(false))
				returnParam.Value = reader.GetValue(0);
			
			return reader;
		}

		internal void SetParams()
		{
			if (!m_setParamsFlags)
				return;
			m_setParamsFlags = false;
			var commandText = "SELECT " + string.Join(", ", m_outParamNames);
			using (var reader = (MyCatDataReader) base.ExecuteReaderAsync(commandText, new MyCatParameterCollection(), CommandBehavior.Default, IOBehavior.Synchronous, m_cancellationToken).GetAwaiter().GetResult())
			{
				reader.Read();
				for (var i = 0; i < m_outParams.Count; i++)
				{
					var param = m_outParams[i];
					if (param.DbType != default(DbType))
					{
						var dbTypeMapping = TypeMapper.Mapper.GetDbTypeMapping(param.DbType);
						if (dbTypeMapping != null)
						{
							param.Value = dbTypeMapping.DoConversion(reader.GetValue(i));
							continue;
						}
					}
					param.Value = reader.GetValue(i);
				}
			}
		}

		readonly MyCatCommand m_command;
		bool m_setParamsFlags;
		MyCatParameterCollection m_outParams;
		List<string> m_outParamNames;
		private CancellationToken m_cancellationToken;
	}
}