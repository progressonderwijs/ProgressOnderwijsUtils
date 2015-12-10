using System.Data.SqlClient;
using System;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        public object EquatableValue { get; private set; }

        public SqlParameter ToSqlParameter(string paramName)
            => new SqlParameter {
                IsNullable = EquatableValue == DBNull.Value,
                ParameterName = paramName,
                Value = EquatableValue is Filter.CurrentTimeToken ? DateTime.Now : EquatableValue,
            };

        public static void AppendScalarParameter<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            var param = new QueryScalarParameterComponent { EquatableValue = o ?? DBNull.Value };
            SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
        }
    }
}
