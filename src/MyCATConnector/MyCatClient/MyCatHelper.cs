using System;

namespace MyCat.Data.MyCatClient
{
	public sealed class MyCatHelper
	{
		[Obsolete("Use MyCatConnection.ClearAllPools or MyCatConnection.ClearAllPoolsAsync")]
		public static void ClearConnectionPools() => MyCatConnection.ClearAllPools();
	}
}
