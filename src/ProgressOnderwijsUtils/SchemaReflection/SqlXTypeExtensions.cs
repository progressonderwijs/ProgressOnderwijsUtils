using System;
using System.Linq;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public enum SqlXType
    {
        // ReSharper disable UnusedMember.Global
        Image = 34,
        UniqueIdentifier = 36,
        Date = 40,
        Time = 41,
        DateTime2 = 42,
        DateTimeOffset = 43,
        TinyInt = 48,
        SmallInt = 52,
        Int = 56,
        SmallDateTime = 58,
        DateTime = 61,
        Sql_variant = 98,
        Bit = 104,
        Float = 62,
        Decimal = 106,
        Numeric = 108,
        BigInt = 127,
        VarBinary = 165,
        VarChar = 167,
        Binary = 173,
        Char = 175,
        RowVersion = 189,
        NVarChar = 231,
        NChar = 239,
        Xml = 241,
        // ReSharper restore UnusedMember.Global
    }

    public struct SqlUnderlyingTypeInfo
    {
        public readonly Type ClrType;
        public readonly string SqlTypeName;

        public SqlUnderlyingTypeInfo(SqlXType xType, Type clrType)
        {
            ClrType = clrType;
            SqlTypeName = xType.ToString();
        }
    }

    public static class SqlXTypeExtensions
    {
        static readonly (SqlXType xType, Type clrType)[] typeLookup = {
            (SqlXType.BigInt, typeof(long)),
            (SqlXType.Binary, typeof(byte[])),
            (SqlXType.Bit, typeof(bool)),
            (SqlXType.Char, typeof(string)),
            (SqlXType.Date, typeof(DateTime)),
            (SqlXType.DateTime, typeof(DateTime)),
            (SqlXType.DateTime2, typeof(DateTime)),
            (SqlXType.DateTimeOffset, typeof(DateTimeOffset)),
            (SqlXType.Decimal, typeof(decimal)),
            (SqlXType.Float, typeof(double)),
            (SqlXType.Image, typeof(byte[])),
            (SqlXType.Int, typeof(int)),
            (SqlXType.NChar, typeof(string)),
            (SqlXType.NVarChar, typeof(string)),
            (SqlXType.Numeric, typeof(decimal)),
            (SqlXType.RowVersion, typeof(byte[])),
            (SqlXType.SmallDateTime, typeof(DateTime)),
            (SqlXType.SmallInt, typeof(short)),
            (SqlXType.Sql_variant, typeof(object)),
            (SqlXType.Time, typeof(TimeSpan)),
            (SqlXType.TinyInt, typeof(byte)),
            (SqlXType.UniqueIdentifier, typeof(Guid)),
            (SqlXType.VarBinary, typeof(byte[])),
            (SqlXType.VarChar, typeof(string)),
            (SqlXType.Xml, typeof(string)),
        };

        public static SqlUnderlyingTypeInfo SqlUnderlyingTypeInfo(this SqlXType sqlXType)
            => new SqlUnderlyingTypeInfo(sqlXType, typeLookup.Single(vt => vt.xType == sqlXType).clrType);

        public static SqlXType NetTypeToSqlXType(Type type)
        {
            var underlyingType = type.GetNonNullableUnderlyingType();
            if (underlyingType == typeof(int)) {
                return SqlXType.Int;
            } else if (underlyingType == typeof(DateTime)) {
                return SqlXType.DateTime;
            } else if (underlyingType == typeof(bool)) {
                return SqlXType.Bit;
            } else if (underlyingType == typeof(double)) {
                return SqlXType.Float;
            } else if (underlyingType == typeof(decimal)) {
                return SqlXType.Decimal;
            } else if (underlyingType == typeof(long)) {
                return SqlXType.BigInt;
            } else if (underlyingType == typeof(string)) {
                return SqlXType.NVarChar;
            } else if (underlyingType == typeof(byte[])) {
                return SqlXType.VarBinary;
            } else if (underlyingType == typeof(char)) {
                return SqlXType.NChar;
            } else {
                return typeLookup.Single(tv => tv.clrType == type).xType;
            }
        }
    }
}
