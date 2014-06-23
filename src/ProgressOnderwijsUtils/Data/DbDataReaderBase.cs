using System.Data;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	public abstract class DbDataReaderBase : DbDataReader
	{
		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) { throw new NotSupportedException(); }
		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			var str = (string)GetValue(ordinal);
			length = Math.Min(length, str.Length);
			if (buffer != null && buffer.Length >= bufferOffset + length)
			{
				for (int i = (int)dataOffset; i < length; i++)
					buffer[bufferOffset + i] = str[i];
				return Math.Max(0, length - dataOffset);
			}
			else
				throw new NotSupportedException();
		}
		public override DataTable GetSchemaTable() { throw new NotSupportedException(); }

		public override string GetDataTypeName(int ordinal) { return GetFieldType(ordinal).ToString(); }

		bool hasRows, afterFirstRowPeek;
		protected abstract bool ReadImpl();

		public override bool HasRows
		{
			get
			{
				if (!hasRows)
				{
					afterFirstRowPeek = true;
					hasRows = ReadImpl();
				}
				return hasRows;
			}
		}
		public override bool Read()
		{
			if (afterFirstRowPeek)
			{
				afterFirstRowPeek = false;
				return hasRows;
			}
			else
			{
				bool nextRow = ReadImpl();
				if (nextRow) hasRows = true;
				return nextRow;
			}
		}

		protected bool isClosed;
		protected override void Dispose(bool disposing) { base.Dispose(disposing); isClosed = true; }
		public override bool IsClosed { get { return isClosed; } }

		public override int Depth { get { return 0; } }
		public override System.Collections.IEnumerator GetEnumerator() { while (Read())yield return this; }
		public override bool GetBoolean(int ordinal) { return (bool)GetValue(ordinal); }
		public override byte GetByte(int ordinal) { return (byte)GetValue(ordinal); }
		public override char GetChar(int ordinal) { return (char)GetValue(ordinal); }
		public override DateTime GetDateTime(int ordinal) { return (DateTime)GetValue(ordinal); }
		public override decimal GetDecimal(int ordinal) { return (decimal)GetValue(ordinal); }
		public override double GetDouble(int ordinal) { return (double)GetValue(ordinal); }
		public override float GetFloat(int ordinal) { return (float)GetValue(ordinal); }
		public override Guid GetGuid(int ordinal) { return (Guid)GetValue(ordinal); }
		public override short GetInt16(int ordinal) { return (short)GetValue(ordinal); }
		public override int GetInt32(int ordinal) { return (int)GetValue(ordinal); }
		public override long GetInt64(int ordinal) { return (long)GetValue(ordinal); }
		public override string GetString(int ordinal) { return (string)GetValue(ordinal); }
		public Identifier<T> GetIdentifier<T>(int ordinal) where T : Identifier<T>, new()
		{
			var r = (T)Activator.CreateInstance(typeof(T), null);
			var identifier = r as IIdentifier;
			// ReSharper disable PossibleNullReferenceException
			identifier.Value = (int)GetValue(ordinal);
			// ReSharper restore PossibleNullReferenceException}
			return (Identifier<T>)identifier;
		}

		public override int GetValues(object[] values)
		{
			var fieldsTodo = Math.Min(values.Length, FieldCount);
			for (int i = 0; i < fieldsTodo; i++) values[i] = GetValue(i);
			return fieldsTodo;
		}

		public override bool IsDBNull(int ordinal) { return GetValue(ordinal) is DBNull; }
		public override bool NextResult() { return false; }
		public override int RecordsAffected { get { return -1; } }
		public override object this[string name] { get { return GetValue(GetOrdinal(name)); } }
		public override object this[int ordinal] { get { return GetValue(ordinal); } }
	}
}