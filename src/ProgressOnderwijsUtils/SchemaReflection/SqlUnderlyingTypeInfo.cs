namespace ProgressOnderwijsUtils.SchemaReflection
{
    public struct SqlTypeInfo
    {
        public readonly SqlXType XType;
        public readonly short MaxLength;
        public readonly byte Precision;
        public readonly byte Scale;
        public readonly bool IsNullable;
        public const int VARCHARMAX_MAXLENGTH_FOR_SQLSERVER = -1;

        public SqlTypeInfo(SqlXType xType, short maxLength, byte precision, byte scale, bool isNullable)
        {
            XType = xType;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            IsNullable = isNullable;
        }

        string ColumnPrecisionSpecifier()
            => XType switch {
                SqlXType.Decimal => "(" + Precision + "," + Scale + ")",
                SqlXType.NVarChar or SqlXType.NChar => MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : "(" + MaxLength / 2 + ")",
                SqlXType.VarChar or SqlXType.Char or SqlXType.VarBinary or SqlXType.Binary => MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : "(" + MaxLength + ")",
                SqlXType.DateTime2 when Scale != 7 => "(" + Scale + ")",
                _ => ""
            };

        public string ToSqlTypeName()
            => ToSqlTypeNameWithoutNullability() + NullabilityAnnotation();

        public string ToSqlTypeNameWithoutNullability()
            => XType.SqlUnderlyingTypeInfo().SqlTypeName + ColumnPrecisionSpecifier();

        string NullabilityAnnotation()
            => IsNullable ? " null" : " not null";
    }
}
