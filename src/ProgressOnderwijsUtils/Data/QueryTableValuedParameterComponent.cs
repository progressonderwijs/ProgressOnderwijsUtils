using System.Linq;
using System.Collections.Generic;
using System;

namespace ProgressOnderwijsUtils
{
    sealed class QueryTableValuedParameterComponent<T> : IQueryParameter, IQueryComponent
        where T : IMetaObject
    {
        static readonly string columnListClause =
            MetaObject.GetMetaProperties<T>()
                .Select(mp => "TVP." + mp.Name)
                .JoinStrings(", ");

        const string subselect_part1 = "(select ";
        const string subselect_part3 = " from ";
        const string subselect_part5 = " TVP)";
        readonly IEnumerable<T> objs;
        readonly string DbTypeName;
        public object EquatableValue => Tuple.Create(objs, DbTypeName);

        internal QueryTableValuedParameterComponent(string dbTypeName, T[] list)
        {
            objs = list;
            DbTypeName = dbTypeName;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            SqlFactory.AppendSql(ref factory, subselect_part1);
            SqlFactory.AppendSql(ref factory, columnListClause);
            SqlFactory.AppendSql(ref factory, subselect_part3);
            SqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(this));
            SqlFactory.AppendSql(ref factory, subselect_part5);
        }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            paramArgs.Value = MetaObject.CreateDataReader(objs);
            paramArgs.TypeName = DbTypeName;
        }
    }
}
