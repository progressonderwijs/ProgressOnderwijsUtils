using System;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        public object EquatableValue { get; private set; }

        public void ToSqlParameter(ref SqlParamArgs paramArgs) {
            paramArgs.Value = EquatableValue == Filter.CurrentTimeToken.Instance ? DateTime.Now : ((EquatableValue as ISmartEnum)?.Id ?? EquatableValue);
        }

        public static void AppendScalarParameter<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            var param = new QueryScalarParameterComponent { EquatableValue = o ?? DBNull.Value };
            SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
        }
    }
}
