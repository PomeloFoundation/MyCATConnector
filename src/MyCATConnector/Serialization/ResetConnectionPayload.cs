using System;

namespace MyCat.Data.Serialization
{
	internal class ResetConnectionPayload
	{
		public static PayloadData Create()
		{
			return new PayloadData(new ArraySegment<byte>(new[] { (byte) CommandKind.ResetConnection }));
		}
	}
}
