﻿using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MyCat.Data.Protocol.Serialization;
using MyCat.Data.Serialization;

namespace MyCat.Data.MyCatClient.CommandExecutors
{
	internal class TextCommandExecutor : ICommandExecutor
	{
		internal TextCommandExecutor(MyCatCommand command)
		{
			m_command = command;
		}

		public virtual async Task<int> ExecuteNonQueryAsync(string commandText, MyCatParameterCollection parameterCollection,
			IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			using (var reader = (MyCatDataReader) await ExecuteReaderAsync(commandText, parameterCollection, CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(false))
			{
				do
				{
					while (await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(false))
					{
					}
				} while (await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(false));
				return reader.RecordsAffected;
			}
		}

		public virtual async Task<object> ExecuteScalarAsync(string commandText, MyCatParameterCollection parameterCollection,
			IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			object result = null;
			using (var reader = (MyCatDataReader) await ExecuteReaderAsync(commandText, parameterCollection, CommandBehavior.SingleResult | CommandBehavior.SingleRow, ioBehavior, cancellationToken).ConfigureAwait(false))
			{
				do
				{
					if (await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(false))
						result = reader.GetValue(0);
				} while (await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(false));
			}
			return result;
		}

		public virtual async Task<DbDataReader> ExecuteReaderAsync(string commandText, MyCatParameterCollection parameterCollection,
			CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			m_command.VerifyValid();
			var connection = m_command.Connection;
			connection.HasActiveReader = true;

			MyCatDataReader reader = null;
			try
			{
				m_command.LastInsertedId = -1;
				var statementPreparerOptions = StatementPreparerOptions.None;
				if (connection.AllowUserVariables || m_command.CommandType == CommandType.StoredProcedure)
					statementPreparerOptions |= StatementPreparerOptions.AllowUserVariables;
				if (connection.OldGuids)
					statementPreparerOptions |= StatementPreparerOptions.OldGuids;
				var preparer = new MyCatStatementPreparer(commandText, parameterCollection, statementPreparerOptions);
				var payload = new PayloadData(preparer.ParseAndBindParameters());
				await connection.Session.SendAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(false);
				reader = await MyCatDataReader.CreateAsync(m_command, behavior, ioBehavior, cancellationToken).ConfigureAwait(false);
				return reader;
			}
			finally
			{
				if (reader == null)
				{
					// received an error from MySQL and never created an active reader
					connection.HasActiveReader = false;
				}
			}
		}

		readonly MyCatCommand m_command;
	}
}