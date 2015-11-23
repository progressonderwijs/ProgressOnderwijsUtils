using System.Data.SqlClient;
using System;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        readonly object paramval;
        readonly int hashCode;

        internal QueryScalarParameterComponent(object o)
        {
            paramval = o ?? DBNull.Value;
            hashCode = paramval.GetHashCode() + 37;
        }

        public object EquatableValue => paramval;

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

        public bool Equals(IQueryComponent other) => (other is QueryScalarParameterComponent) && Equals(paramval, ((QueryScalarParameterComponent)other).paramval);
        public override bool Equals(object obj) => (obj is QueryScalarParameterComponent) && Equals((QueryScalarParameterComponent)obj);
        public override int GetHashCode() => hashCode;

        public void AppendTo(ref CommandFactory factory)
        {
            var sqlString = factory.GetNameForParam(this);
            factory.AppendSql(sqlString, 0, sqlString.Length);
        }

        public int EstimateLength()
        {
            return 5; // length of @par0
        }
    }
}
