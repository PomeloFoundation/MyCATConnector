﻿using System;
using System.Text;

namespace MyCat.Data.Serialization
{
	internal class ColumnDefinitionPayload
	{
		public string Name { get; }
		public CharacterSet CharacterSet { get; }
		public int ColumnLength { get; }
		public ColumnType ColumnType { get; }
		public ColumnFlags ColumnFlags { get; }

		public static ColumnDefinitionPayload Create(PayloadData payload)
		{
			var reader = new ByteArrayReader(payload.ArraySegment);
			var catalog = reader.ReadLengthEncodedByteString();
			var schema = reader.ReadLengthEncodedByteString();
			var table = reader.ReadLengthEncodedByteString();
			var physicalTable = reader.ReadLengthEncodedByteString();
			var name = Encoding.UTF8.GetString(reader.ReadLengthEncodedByteString());
			var physicalName = reader.ReadLengthEncodedByteString();
			reader.ReadByte(0x0C); // length of fixed-length fields, always 0x0C
			var characterSet = (CharacterSet) reader.ReadUInt16();
			var columnLength = (int) reader.ReadUInt32();
			var columnType = (ColumnType) reader.ReadByte();
			var columnFlags = (ColumnFlags) reader.ReadUInt16();
			var decimals = reader.ReadByte(); // 0x00 for integers and static strings, 0x1f for dynamic strings, double, float, 0x00 to 0x51 for decimals
			reader.ReadByte(0);
			if (reader.BytesRemaining > 0)
			{
				int defaultValuesCount = checked((int) reader.ReadLengthEncodedInteger());
				for (int i = 0; i < defaultValuesCount; i++)
					reader.ReadLengthEncodedByteString();
			}

			if (reader.BytesRemaining != 0)
				throw new FormatException("Extra bytes at end of payload.");
			return new ColumnDefinitionPayload(name, characterSet, columnLength, columnType, columnFlags);
		}

		private ColumnDefinitionPayload(string name, CharacterSet characterSet, int columnLength, ColumnType columnType, ColumnFlags columnFlags)
		{
			Name = name;
			CharacterSet = characterSet;
			ColumnLength = columnLength;
			ColumnType = columnType;
			ColumnFlags = columnFlags;
		}
	}
}
