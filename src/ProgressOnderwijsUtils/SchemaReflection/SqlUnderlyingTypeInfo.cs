namespace ProgressOnderwijsUtils.SchemaReflection
{
    public readonly struct SqlTypeInfo
    {
        public readonly SqlXType UserTypeId;
        public readonly short MaxLength;
        public readonly byte Precision;
        public readonly byte Scale;
        public readonly bool IsNullable;
        public const short VARCHARMAX_MAXLENGTH_FOR_SQLSERVER = -1;

        public SqlTypeInfo(SqlXType userTypeId, short maxLength, byte precision, byte scale, bool isNullable)
        {
            UserTypeId = userTypeId;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            IsNullable = isNullable;
        }

        string ColumnPrecisionSpecifier()
            => UserTypeId switch {
                SqlXType.Decimal or SqlXType.Numeric => $"({Precision},{Scale})",
                SqlXType.NVarChar or SqlXType.NChar => MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : $"({MaxLength / 2})",
                SqlXType.VarChar or SqlXType.Char or SqlXType.VarBinary or SqlXType.Binary => MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : $"({MaxLength})",
                SqlXType.DateTime2 or SqlXType.DateTimeOffset or SqlXType.Time when Scale != 7 => $"({Scale})",
                _ => ""
            };

        public string ToSqlTypeName()
            => ToSqlTypeNameWithoutNullability() + NullabilityAnnotation();

        public string ToSqlTypeNameWithoutNullability()
            => UserTypeId.SqlUnderlyingTypeInfo().SqlTypeName + ColumnPrecisionSpecifier();

        string NullabilityAnnotation()
            => IsNullable ? " null" : " not null";
    }
}
