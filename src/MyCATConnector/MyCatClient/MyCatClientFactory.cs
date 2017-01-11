using System.Data.Common;

namespace MyCat.Data.MyCatClient
{
	public sealed class MyCatClientFactory : DbProviderFactory
	{
		public static readonly MyCatClientFactory Instance = new MyCatClientFactory();

		private MyCatClientFactory()
		{
		}

		public override DbCommand CreateCommand()
			=> new MyCatCommand();

		public override DbConnection CreateConnection()
			=> new MyCatConnection();

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
			=> new MyCatConnectionStringBuilder();

		public override DbParameter CreateParameter()
			=> new MyCatParameter();
	}
}
