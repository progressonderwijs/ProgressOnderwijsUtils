using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        public object EquatableValue { get; private set; }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            paramArgs.Value = EquatableValue == CurrentTimeToken.Instance ? DateTime.Now : EquatableValue;
        }

        public static void AppendScalarParameter<TCommandFactory>(ref TCommandFactory factory, [CanBeNull] object o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o == null || o == DBNull.Value) {
                ParameterizedSqlFactory.AppendSql(ref factory, "NULL");
            } else {
                var param = new QueryScalarParameterComponent { EquatableValue = o };
                ParameterizedSqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
            }
        }
    }
}
