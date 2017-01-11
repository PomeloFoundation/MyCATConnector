using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MyCat.Data.MyCatClient.CommandExecutors;
using MyCat.Data.Protocol.Serialization;

namespace MyCat.Data.MyCatClient
{
	public sealed class MyCatCommand : DbCommand
	{
		public MyCatCommand()
			: this(null, null, null)
		{
		}

		public MyCatCommand(string commandText)
			: this(commandText, null, null)
		{
		}

		public MyCatCommand(MyCatConnection connection, MyCatTransaction transaction)
			: this(null, connection, transaction)
		{
		}

		public MyCatCommand(string commandText, MyCatConnection connection)
			: this(commandText, connection, null)
		{
		}

		public MyCatCommand(string commandText, MyCatConnection connection, MyCatTransaction transaction)
		{
			CommandText = commandText;
			DbConnection = connection;
			DbTransaction = transaction;
			m_parameterCollection = new MyCatParameterCollection();
			CommandType = CommandType.Text;
		}

		public new MyCatParameterCollection Parameters
		{
			get
			{
				VerifyNotDisposed();
				return m_parameterCollection;
			}
		}

		public override void Cancel()
		{
			// documentation says this shouldn't throw (but just fail silently), but for now make it explicit that this doesn't work
			throw new NotSupportedException("Use the Async overloads with a CancellationToken.");
		}

		public override int ExecuteNonQuery() =>
			ExecuteNonQueryAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();

		public override object ExecuteScalar() =>
			ExecuteScalarAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();

		public override void Prepare()
		{
			// NOTE: Prepared statements in MySQL are not currently supported.
			// 1) Only a subset of statements are actually preparable by the server: http://dev.mysql.com/worklog/task/?id=2871
			// 2) Although CLIENT_MULTI_STATEMENTS is supposed to mean that the Server "Can handle multiple statements per COM_QUERY and COM_STMT_PREPARE" (https://dev.mysql.com/doc/internals/en/capability-flags.html#flag-CLIENT_MULTI_STATEMENTS),
			//    this is not actually true because "Prepared statement handles are defined to work only with strings that contain a single statement" (http://dev.mysql.com/doc/refman/5.7/en/c-api-multiple-queries.html).
		}

		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }

		public override CommandType CommandType
		{
			get
			{
				return m_commandType;
			}
			set
			{
				if (value != CommandType.Text && value != CommandType.StoredProcedure)
					throw new ArgumentException("CommandType must be Text or StoredProcedure.", nameof(value));
				if (value == m_commandType)
					return;

				m_commandType = value;
				if (value == CommandType.Text)
					m_commandExecutor = new TextCommandExecutor(this);
				else if (value == CommandType.StoredProcedure)
					m_commandExecutor = new StoredProcedureCommandExecutor(this);
			}
		}

		public override bool DesignTimeVisible { get; set; }

		public override UpdateRowSource UpdatedRowSource
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public long LastInsertedId { get; internal set; }

		protected override DbConnection DbConnection { get; set; }
		protected override DbParameterCollection DbParameterCollection => m_parameterCollection;
		protected override DbTransaction DbTransaction { get; set; }

		protected override DbParameter CreateDbParameter()
		{
			VerifyNotDisposed();
			return new MyCatParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
			ExecuteReaderAsync(behavior, IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();

		public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) =>
			ExecuteNonQueryAsync(Connection.AsyncIOBehavior, cancellationToken);

		internal async Task<int> ExecuteNonQueryAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			return await m_commandExecutor.ExecuteNonQueryAsync(CommandText, m_parameterCollection, ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) =>
			ExecuteScalarAsync(Connection.AsyncIOBehavior, cancellationToken);

		internal async Task<object> ExecuteScalarAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			return await m_commandExecutor.ExecuteScalarAsync(CommandText, m_parameterCollection, ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) =>
			ExecuteReaderAsync(behavior, Connection.AsyncIOBehavior, cancellationToken);

		internal async Task<DbDataReader> ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior,
			CancellationToken cancellationToken)
		{
            // MyCAT: Split statements
            var statements = CommandText.Split(';');
            if (statements.Count() <= 1)
            {
                return await m_commandExecutor.ExecuteReaderAsync(CommandText, m_parameterCollection, behavior, ioBehavior, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                for (var i = 0; i < statements.Count(); i++)
                {
                    var reader =  await m_commandExecutor.ExecuteReaderAsync(statements[i], m_parameterCollection, behavior, ioBehavior, cancellationToken).ConfigureAwait(false);
                    if (i == statements.Count() - 1)
                    {
                        return reader;
                    }
                }
            }
            return null;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					m_parameterCollection = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		internal new MyCatConnection Connection => (MyCatConnection) DbConnection;

		private void VerifyNotDisposed()
		{
			if (m_parameterCollection == null)
				throw new ObjectDisposedException(GetType().Name);
		}

		internal void VerifyValid()
		{
			VerifyNotDisposed();
			if (DbConnection == null)
				throw new InvalidOperationException("Connection property must be non-null.");
			if (DbConnection.State != ConnectionState.Open && DbConnection.State != ConnectionState.Connecting)
				throw new InvalidOperationException("Connection must be Open; current state is {0}".FormatInvariant(DbConnection.State));
			if (DbTransaction != Connection.CurrentTransaction)
				throw new InvalidOperationException("The transaction associated with this command is not the connection's active transaction.");
			if (string.IsNullOrWhiteSpace(CommandText))
				throw new InvalidOperationException("CommandText must be specified");
			if (Connection.HasActiveReader)
				throw new MyCatException("There is already an open DataReader associated with this Connection which must be closed first.");
		}

		internal void ReaderClosed()
		{
			var executor = m_commandExecutor as StoredProcedureCommandExecutor;
			executor?.SetParams();
		}

		MyCatParameterCollection m_parameterCollection;
		CommandType m_commandType;
		ICommandExecutor m_commandExecutor;
	}
}
