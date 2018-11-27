using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;

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
        static readonly (Type clrType, SqlXType xType)[] typeLookup = {
            (typeof(bool), SqlXType.Bit),
            (typeof(byte), SqlXType.TinyInt),
            (typeof(byte[]), SqlXType.VarBinary),
            (typeof(byte[]), SqlXType.Binary),
            (typeof(byte[]), SqlXType.Image),
            (typeof(byte[]), SqlXType.RowVersion),
            (typeof(DateTime), SqlXType.DateTime),
            (typeof(DateTime), SqlXType.Date),
            (typeof(DateTime), SqlXType.DateTime2),
            (typeof(DateTime), SqlXType.SmallDateTime),
            (typeof(DateTimeOffset), SqlXType.DateTimeOffset),
            (typeof(decimal), SqlXType.Decimal),
            (typeof(decimal), SqlXType.Numeric),
            (typeof(double), SqlXType.Float),
            (typeof(Guid), SqlXType.UniqueIdentifier),
            (typeof(short), SqlXType.SmallInt),
            (typeof(int), SqlXType.Int),
            (typeof(long), SqlXType.BigInt),
            (typeof(object), SqlXType.Sql_variant),
            (typeof(string), SqlXType.NVarChar),
            (typeof(string), SqlXType.Char),
            (typeof(string), SqlXType.NChar),
            (typeof(string), SqlXType.VarChar),
            (typeof(string), SqlXType.Xml),
            (typeof(TimeSpan), SqlXType.Time),
        };

        /// <summary>
        /// Finds the best mapping of this xType to a clr-type.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When no mapping could be found.</exception>
        public static SqlUnderlyingTypeInfo SqlUnderlyingTypeInfo(this SqlXType sqlXType)
        {
            foreach (var o in typeLookup) {
                if (o.xType == sqlXType) {
                    return new SqlUnderlyingTypeInfo(sqlXType, o.clrType);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(sqlXType), "Could not find a clr-type for the XType " + sqlXType);
        }

        /// <summary>
        /// Finds the best mapping of this clr-type to an sql XType.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When no mapping could be found.</exception>
        public static SqlXType NetTypeToSqlXType([NotNull] Type type)
        {
            var underlyingType = type.GetNonNullableUnderlyingType();
            foreach (var o in typeLookup) {
                if (o.clrType == underlyingType) {
                    return o.xType;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(type), "Could not find an sql XType for the clr-type " + underlyingType.ToCSharpFriendlyTypeName() + (type == underlyingType ? "" : ", which is the underlying type of " + type.ToCSharpFriendlyTypeName()));
        }
    }
}
