using System.Data.SqlClient;
using System;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        public object EquatableValue { get; }

        internal QueryScalarParameterComponent(object o)
        {
            EquatableValue = o ?? DBNull.Value;
        }

        public SqlParameter ToSqlParameter(string paramName)
            => new SqlParameter {
                IsNullable = EquatableValue == DBNull.Value,
                ParameterName = paramName,
                Value = EquatableValue is Filter.CurrentTimeToken ? DateTime.Now : EquatableValue,
            };

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(this));
    }
}
