using System.Data.SqlClient;
using System;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    sealed class QueryScalarParameterComponent : IQueryParameter
    {
        readonly object paramval;
        readonly int hashCode;

        internal QueryScalarParameterComponent(object o)
        {
            paramval = o ?? DBNull.Value;
            hashCode = paramval.GetHashCode() + 37;
        }

        public string ToSqlString(CommandFactory factory) => factory.GetNameForParam(this);

        public SqlParameter ToSqlParameter(string paramName)
        {
            object value;
            if (paramval is Filter.CurrentTimeToken) {
                value = DateTime.Now;
            } else {
                value = paramval;
            }
            return new SqlParameter {
                IsNullable = paramval == DBNull.Value,
                ParameterName = paramName,
                Value = value,
            };
        }

        public string ToDebugText(Taal? taalOrNull)
        {
            if (paramval == null || paramval == DBNull.Value) {
                return "null";
            } else if (paramval is string) {
                return "'" + (paramval as string).Replace("'", "''") + "'";
            } else if (paramval is int) {
                return ((int)paramval).ToStringInvariant();
            } else if (paramval is decimal) {
                return ((decimal)paramval).ToStringInvariant();
            } else if (paramval is DateTime) {
                return ((DateTime)paramval).ToString(@"\'yyyy-MM-dd HH:mm:ss.fffffff\'");
            } else if (paramval is Enum) {
                return ((IConvertible)paramval).ToInt64(null).ToStringInvariant() + "/*" + ObjectToCode.PlainObjectToCode(paramval) + "*/";
            } else {
                return "{!" + (taalOrNull.HasValue ? Converteer.ToString(paramval, taalOrNull.Value) : paramval.ToString()) + "!}";
            }
        }

        public bool Equals(IQueryComponent other) => (other is QueryScalarParameterComponent) && Equals(paramval, ((QueryScalarParameterComponent)other).paramval);
        public override bool Equals(object obj) => (obj is QueryScalarParameterComponent) && Equals((QueryScalarParameterComponent)obj);
        public override int GetHashCode() => hashCode;
    }
}
