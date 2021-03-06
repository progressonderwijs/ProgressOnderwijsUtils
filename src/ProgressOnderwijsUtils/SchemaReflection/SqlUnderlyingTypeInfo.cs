﻿namespace ProgressOnderwijsUtils.SchemaReflection
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
        {
            if (XType == SqlXType.Decimal) {
                return "(" + Precision + "," + Scale + ")";
            } else if (XType == SqlXType.NVarChar || XType == SqlXType.NChar) {
                return MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : "(" + MaxLength / 2 + ")";
            } else if (XType == SqlXType.VarChar || XType == SqlXType.Char || XType == SqlXType.VarBinary || XType == SqlXType.Binary) {
                return MaxLength == VARCHARMAX_MAXLENGTH_FOR_SQLSERVER ? "(max)" : "(" + MaxLength + ")";
            } else {
                return "";
            }
        }

        public string ToSqlTypeName()
            => ToSqlTypeNameWithoutNullability() + NullabilityAnnotation();

        public string ToSqlTypeNameWithoutNullability()
            => XType.SqlUnderlyingTypeInfo().SqlTypeName + ColumnPrecisionSpecifier();

        string NullabilityAnnotation()
            => IsNullable ? " null" : " not null";
    }
}
