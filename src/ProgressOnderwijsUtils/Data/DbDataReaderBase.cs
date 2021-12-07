using System;
using System.Data;
using System.Data.Common;

namespace ProgressOnderwijsUtils;

public abstract class DbDataReaderBase : DbDataReader
{
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        long read = 0;
        var data = (byte[])GetValue(ordinal);
        while (read < length && read + dataOffset < data.Length) {
            if (buffer != null) {
                buffer[bufferOffset + read] = data[dataOffset + read];
            }
            ++read;
        }
        return read;
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        var str = GetString(ordinal);
        length = Math.Min(length, str.Length);
        if (buffer != null && buffer.Length >= bufferOffset + length) {
            for (var i = (int)dataOffset; i < length; i++) {
                buffer[bufferOffset + i] = str[i];
            }
            return Math.Max(0, length - dataOffset);
        } else {
            throw new NotSupportedException();
        }
    }

    public override DataTable GetSchemaTable()
        => throw new NotSupportedException();

    public override string GetDataTypeName(int ordinal)
        => (GetFieldType(ordinal) ?? throw new Exception("column " + ordinal + " untyped")).ToString();

    bool hasRows, afterFirstRowPeek;
    protected abstract bool ReadImpl();

    public override bool HasRows
    {
        get {
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
            var nextRow = ReadImpl();
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

    public override bool IsClosed
        => isClosed;

    public override int Depth
        => 0;

    public override System.Collections.IEnumerator GetEnumerator()
    {
        while (Read()) {
            yield return this;
        }
    }

    public override bool GetBoolean(int ordinal)
        => GetFieldValue<bool>(ordinal);

    public override byte GetByte(int ordinal)
        => GetFieldValue<byte>(ordinal);

    public override char GetChar(int ordinal)
        => GetFieldValue<char>(ordinal);

    public override DateTime GetDateTime(int ordinal)
        => GetFieldValue<DateTime>(ordinal);

    public override decimal GetDecimal(int ordinal)
        => GetFieldValue<decimal>(ordinal);

    public override double GetDouble(int ordinal)
        => GetFieldValue<double>(ordinal);

    public override float GetFloat(int ordinal)
        => GetFieldValue<float>(ordinal);

    public override Guid GetGuid(int ordinal)
        => GetFieldValue<Guid>(ordinal);

    public override short GetInt16(int ordinal)
        => GetFieldValue<short>(ordinal);

    public override int GetInt32(int ordinal)
        => GetFieldValue<int>(ordinal);

    public override long GetInt64(int ordinal)
        => GetFieldValue<long>(ordinal);

    public override string GetString(int ordinal)
        => GetFieldValue<string>(ordinal);

    public override int GetValues(object[] values)
    {
        var fieldsTodo = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < fieldsTodo; i++) {
            values[i] = GetValue(i);
        }
        return fieldsTodo;
    }

    public override bool IsDBNull(int ordinal)
        => GetValue(ordinal) is DBNull;

    public override bool NextResult()
        => false;

    public override int RecordsAffected
        => -1;

    public override object this[string name]
        => GetValue(GetOrdinal(name));

    public override object this[int ordinal]
        => GetValue(ordinal);
}