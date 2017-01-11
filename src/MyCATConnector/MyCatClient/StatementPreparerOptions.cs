using System;

namespace MyCat.Data.MyCatClient
{
	[Flags]
	internal enum StatementPreparerOptions
	{
		None = 0,
		AllowUserVariables = 1,
		OldGuids = 2,
	}
}
