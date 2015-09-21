﻿using System.Data;
using System.Data.Common;
using System;

namespace ProgressOnderwijsUtils
{
    public abstract class DbDataReaderBase : DbDataReader
    {
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            var str = (string)GetValue(ordinal);
            length = Math.Min(length, str.Length);
            if (buffer != null && buffer.Length >= bufferOffset + length) {
                for (int i = (int)dataOffset; i < length; i++) {
                    buffer[bufferOffset + i] = str[i];
                }
                return Math.Max(0, length - dataOffset);
            } else {
                throw new NotSupportedException();
            }
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).ToString();
        bool hasRows, afterFirstRowPeek;
        protected abstract bool ReadImpl();

        public override bool HasRows
        {
            get
            {
                if (!hasRows) {
                    afterFirstRowPeek = true;
                    hasRows = ReadImpl();
                }
                return hasRows;
            }
        }

        public override bool Read()
        {
            if (afterFirstRowPeek) {
                afterFirstRowPeek = false;
                return hasRows;
            } else {
                bool nextRow = ReadImpl();
                if (nextRow) {
                    hasRows = true;
                }
                return nextRow;
            }
        }

        protected bool isClosed;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            isClosed = true;
        }

        public override bool IsClosed => isClosed;
        public override int Depth => 0;

        public override System.Collections.IEnumerator GetEnumerator()
        {
            while (Read()) {
                yield return this;
            }
        }

        public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
        public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
        public override char GetChar(int ordinal) => (char)GetValue(ordinal);
        public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
        public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
        public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
        public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
        public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
        public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
        public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
        public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
        public override string GetString(int ordinal) => (string)GetValue(ordinal);

        public override int GetValues(object[] values)
        {
            var fieldsTodo = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < fieldsTodo; i++) {
                values[i] = GetValue(i);
            }
            return fieldsTodo;
        }

        public override bool IsDBNull(int ordinal) => GetValue(ordinal) is DBNull;
        public override bool NextResult() => false;
        public override int RecordsAffected => -1;
        public override object this[string name] => GetValue(GetOrdinal(name));
        public override object this[int ordinal] => GetValue(ordinal);
    }
}
