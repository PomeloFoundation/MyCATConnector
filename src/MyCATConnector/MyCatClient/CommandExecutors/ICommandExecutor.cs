using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MyCat.Data.Protocol.Serialization;

namespace MyCat.Data.MyCatClient.CommandExecutors
{
	internal interface ICommandExecutor
	{
		Task<int> ExecuteNonQueryAsync(string commandText, MyCatParameterCollection parameterCollection, IOBehavior ioBehavior, CancellationToken cancellationToken);

		Task<object> ExecuteScalarAsync(string commandText, MyCatParameterCollection parameterCollection, IOBehavior ioBehavior, CancellationToken cancellationToken);

		Task<DbDataReader> ExecuteReaderAsync(string commandText, MyCatParameterCollection parameterCollection, CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken);
	}
}
