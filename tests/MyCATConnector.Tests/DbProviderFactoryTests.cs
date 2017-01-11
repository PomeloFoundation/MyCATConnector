using MyCat.Data.MyCatClient;
using Xunit;

namespace MyCat.Data.Tests
{
	public class DbProviderFactoryTests
	{
		[Fact]
		public void CreatesExpectedTypes()
		{
			Assert.IsType<MyCatConnection>(MyCatClientFactory.Instance.CreateConnection());
			Assert.IsType<MyCatConnectionStringBuilder>(MyCatClientFactory.Instance.CreateConnectionStringBuilder());
			Assert.IsType<MyCatCommand>(MyCatClientFactory.Instance.CreateCommand());
			Assert.IsType<MyCatParameter>(MyCatClientFactory.Instance.CreateParameter());
		}

		[Fact]
		public void Singleton()
		{
			var factory1 = MyCatClientFactory.Instance;
			var factory2 = MyCatClientFactory.Instance;
			Assert.True(object.ReferenceEquals(factory1, factory2));
		}
	}
}
