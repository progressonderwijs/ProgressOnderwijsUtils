using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    sealed class QuerySmartEnumComponent : IQueryParameter
    {
        readonly SmartEnum paramval;

        internal QuerySmartEnumComponent(SmartEnum o)
        {
            paramval = o;
        }

        public string ToSqlString(CommandFactory qnum) => "@par" + qnum.GetNumberForParam(this);

        public SqlParameter ToSqlParameter(int paramNum)
        {
            return new SqlParameter {
                IsNullable = paramval == null,
                ParameterName = "@par" + paramNum,
                Value = (object)paramval?.Id ?? DBNull.Value,
            };
        }

        public string ToDebugText(Taal? taalOrNull)
        {
            if (paramval == null) {
                return "null";
            } else {
                return paramval.Id +  " /*" + (taalOrNull.HasValue ? Converteer.ToString(paramval, taalOrNull.Value) : paramval.ToString()) + "*/";
            }
        }

        public bool Equals(IQueryComponent other) => (other is QuerySmartEnumComponent) && Equals(paramval, ((QuerySmartEnumComponent)other).paramval);
        public override bool Equals(object obj) => (obj is QuerySmartEnumComponent) && Equals((QuerySmartEnumComponent)obj);

        public override int GetHashCode()
        {
            if (paramval == null) {
                return 0;
            }
            return paramval.GetHashCode() + 37;
        }
    }
}