using System.Data.SqlClient;
using System;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        readonly object paramval;

        internal QueryScalarParameterComponent(object o)
        {
            paramval = o ?? DBNull.Value;
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

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            var sqlString = factory.RegisterParameterAndGetName(this);
            factory.AppendSql(sqlString);
        }

        public int EstimateLength() => CommandFactory.EstimatedParameterLength;
    }
}
