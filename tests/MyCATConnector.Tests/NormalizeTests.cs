using MyCat.Data.MyCatClient;
using MyCat.Data.MyCatClient.Caches;
using Xunit;

namespace MyCat.Data.Tests
{
	public class NormalizeTests
	{
		[Theory]
		[InlineData("`mysql`.`data`", "mysql", "data")]
		[InlineData(" `mysql` . `data` ", "mysql", "data")]
		[InlineData("MyCat.Data", "mysql", "data")]
		[InlineData(" mysql . data ", "mysql", "data")]
		[InlineData("`mysql`.data", "mysql", "data")]
		[InlineData("mysql.`data`", "mysql", "data")]
		[InlineData("`my``sql`.`da``ta`", "my``sql", "da``ta")]
		[InlineData("`MyCat.Data`", null, "MyCat.Data")]
		[InlineData("mysqldata", null, "mysqldata")]
		[InlineData("my`sql.data", null, null)]
		[InlineData("mysql.da`ta", null, null)]
		public void NormalizeSchema(string input, string expectedSchema, string expectedComponent)
		{
			var normalized = new NormalizedSchema(input);
			Assert.Equal(expectedSchema, normalized.Schema);
			Assert.Equal(expectedComponent, normalized.Component);
		}

		[Theory]
		[InlineData("param1", "param1")]
		[InlineData("@param1", "param1")]
		[InlineData("?param1", "param1")]
		[InlineData("@`param1`", "param1")]
		[InlineData("?`param1`", "param1")]
		[InlineData("@`param``1`", "param`1")]
		[InlineData("@'param1'", "param1")]
		[InlineData("?'param1'", "param1")]
		[InlineData("@'param''1'", "param'1")]
		[InlineData("@\"param1\"", "param1")]
		[InlineData("?\"param1\"", "param1")]
		[InlineData("@\"param\"\"1\"", "param\"1")]
		[InlineData(" @PaRaM1 ", "PaRaM1")]
		public void NormalizeParameterName(string input, string expectedName)
		{
			var normalized = MyCatParameter.NormalizeParameterName(input);
			Assert.Equal(expectedName, normalized);
		}
	}
}