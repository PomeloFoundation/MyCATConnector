using System;
using System.Data.Common;

namespace MyCat.Data.MyCatClient
{
	public sealed class MyCatException : DbException
	{
		public int Number { get; }
		public string SqlState { get; }

		internal MyCatException(string message)
			: this(message, null)
		{
		}

		internal MyCatException(string message, Exception innerException)
			: this(0, null, message, innerException)
		{
		}

		internal MyCatException(int errorNumber, string sqlState, string message)
			: this(errorNumber, sqlState, message, null)
		{
		}

		internal MyCatException(int errorNumber, string sqlState, string message, Exception innerException)
			: base(message, innerException)
		{
			Number = errorNumber;
			SqlState = sqlState;
		}
	}
}
