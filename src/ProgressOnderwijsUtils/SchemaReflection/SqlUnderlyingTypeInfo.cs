namespace ProgressOnderwijsUtils.SchemaReflection
{
    public readonly struct SqlTypeInfo
    {
        public readonly SqlSystemTypeId UserTypeId;
        public readonly short MaxLength;
        public readonly byte Precision;
        public readonly byte Scale;
        public readonly bool IsNullable;

        public SqlTypeInfo(SqlSystemTypeId userTypeId, short maxLength, byte precision, byte scale, bool isNullable)
        {
            UserTypeId = userTypeId;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            IsNullable = isNullable;
        }

        string ColumnPrecisionSpecifier()
            => UserTypeId switch {
                SqlSystemTypeId.Decimal or SqlSystemTypeId.Numeric => $"({Precision},{Scale})",
                SqlSystemTypeId.NVarChar or SqlSystemTypeId.NChar => MaxLength > 0 ? $"({MaxLength / 2})" : "(max)",
                SqlSystemTypeId.VarChar or SqlSystemTypeId.Char or SqlSystemTypeId.VarBinary or SqlSystemTypeId.Binary => MaxLength > 0 ? $"({MaxLength})" : "(max)",
                SqlSystemTypeId.DateTime2 or SqlSystemTypeId.DateTimeOffset or SqlSystemTypeId.Time when Scale != 7 => $"({Scale})",
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
