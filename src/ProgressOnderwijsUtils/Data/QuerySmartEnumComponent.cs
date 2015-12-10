using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    struct QuerySmartEnumComponent : IQueryParameter
    {
        public object EquatableValue { get; private set; }

        public SqlParameter ToSqlParameter(string paramName)
            => new SqlParameter {
                IsNullable = EquatableValue == DBNull.Value,
                ParameterName = paramName,
                Value = (object)(EquatableValue as ISmartEnum)?.Id ?? DBNull.Value,
            };

        public static void AppendSmartEnumParameter<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            var param = new QuerySmartEnumComponent { EquatableValue = o ?? DBNull.Value };
            SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
        }
    }
}
