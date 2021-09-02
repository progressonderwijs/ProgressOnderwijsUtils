using System;
using System.Collections.Concurrent;
using System.Linq;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public enum SqlSystemTypeId
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
        SysName = 256,
        // ReSharper restore UnusedMember.Global
    }

    public readonly struct SqlUnderlyingTypeInfo
    {
        public readonly Type ClrType;
        public readonly string SqlTypeName;

        public SqlUnderlyingTypeInfo(SqlSystemTypeId systemTypeId, Type clrType)
        {
            ClrType = clrType;
            SqlTypeName = systemTypeId.ToString();
        }
    }

    public static class SqlSystemTypeIdExtensions
    {
        static readonly (Type clrType, SqlSystemTypeId typeId)[] typeLookup = {
            //this list is ordered: earlier rows are better matches, and picked first.
            (typeof(bool), SqlSystemTypeId.Bit),
            (typeof(byte), SqlSystemTypeId.TinyInt),
            (typeof(byte[]), SqlSystemTypeId.VarBinary),
            (typeof(byte[]), SqlSystemTypeId.Binary),
            (typeof(byte[]), SqlSystemTypeId.Image),
            (typeof(byte[]), SqlSystemTypeId.RowVersion),
            (typeof(ulong), SqlSystemTypeId.RowVersion),
            (typeof(ulong), SqlSystemTypeId.Binary),
            (typeof(uint), SqlSystemTypeId.Binary),
            (typeof(DateTime), SqlSystemTypeId.DateTime2),
            (typeof(DateTime), SqlSystemTypeId.DateTime),
            (typeof(DateTime), SqlSystemTypeId.Date),
            (typeof(DateTime), SqlSystemTypeId.SmallDateTime),
            (typeof(DateTimeOffset), SqlSystemTypeId.DateTimeOffset),
            (typeof(decimal), SqlSystemTypeId.Decimal),
            (typeof(decimal), SqlSystemTypeId.Numeric),
            (typeof(double), SqlSystemTypeId.Float),
            (typeof(Guid), SqlSystemTypeId.UniqueIdentifier),
            (typeof(short), SqlSystemTypeId.SmallInt),
            (typeof(int), SqlSystemTypeId.Int),
            (typeof(long), SqlSystemTypeId.BigInt),
            (typeof(object), SqlSystemTypeId.Sql_variant),
            (typeof(string), SqlSystemTypeId.NVarChar),
            (typeof(string), SqlSystemTypeId.Char),
            (typeof(string), SqlSystemTypeId.NChar),
            (typeof(string), SqlSystemTypeId.VarChar),
            (typeof(string), SqlSystemTypeId.Xml),
            (typeof(string), SqlSystemTypeId.SysName),
            (typeof(char), SqlSystemTypeId.NChar),
            (typeof(TimeSpan), SqlSystemTypeId.Time),
        };

        /// <summary>
        /// Finds the best mapping of this sql type id to a clr-type.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When no mapping could be found.</exception>
        public static SqlUnderlyingTypeInfo SqlUnderlyingTypeInfo(this SqlSystemTypeId sqlSystemTypeId)
        {
            foreach (var o in typeLookup) {
                if (o.typeId == sqlSystemTypeId) {
                    return new(sqlSystemTypeId, o.clrType);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(sqlSystemTypeId), "Could not find a clr-type for the type id " + sqlSystemTypeId);
        }

        static readonly ConcurrentDictionary<Type, SqlSystemTypeId?> convertedTypesCache = new();

        /// <summary>
        /// Finds the best mapping of this clr-type to an sql type id.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When no mapping could be found.</exception>
        public static SqlSystemTypeId DotnetTypeToSqlType(Type type)
        {
            var underlyingType = type.GetNonNullableUnderlyingType();
            var convertedType = convertedTypesCache.GetOrAdd(underlyingType, GetConvertedSqlTypeOrNull);
            if (convertedType != null) {
                return convertedType.Value;
            }
            foreach (var o in typeLookup) {
                if (o.clrType == underlyingType) {
                    return o.typeId;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(type), "Could not find an sql type id for the clr-type " + underlyingType.ToCSharpFriendlyTypeName() + (type == underlyingType ? "" : ", which is the underlying type of " + type.ToCSharpFriendlyTypeName()));
        }

        static SqlSystemTypeId? GetConvertedSqlTypeOrNull(Type underlyingType)
        {
            var converterType = underlyingType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasValueConverter<,,>))
                .Select(i => i.GetGenericArguments()[2])
                .SingleOrNull();
            if (converterType == null) {
                return default;
            }
            var conversionProviderType = converterType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueConverterSource<,>));
            var coversionReturnType = conversionProviderType.GetGenericArguments()[1];
            return DotnetTypeToSqlType(coversionReturnType);
        }
    }
}
