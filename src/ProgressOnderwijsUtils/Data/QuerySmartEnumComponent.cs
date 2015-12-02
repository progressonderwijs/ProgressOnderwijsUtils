using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    struct QuerySmartEnumComponent : IQueryParameter
    {
        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            paramArgs.TypeName = null;
            paramArgs.Value = (object)(EquatableValue as ISmartEnum)?.Id ?? DBNull.Value;
        }

        public object EquatableValue { get; private set; }

        public static void AppendSmartEnumParameter<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            var param = new QuerySmartEnumComponent { EquatableValue = o ?? DBNull.Value };
            SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
        }
    }
}