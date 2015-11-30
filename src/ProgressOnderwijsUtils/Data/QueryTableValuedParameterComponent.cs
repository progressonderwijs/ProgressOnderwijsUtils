using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using ExpressionToCodeLib;
using System;

namespace ProgressOnderwijsUtils
{
    sealed class QueryTableValuedParameterComponent<T> : IQueryParameter
        where T : IMetaObject
    {
        const string subselect_part1 = "(select ";
        const string subselect_part3 = " from ";
        const string subselect_part5 = " TVP)";

        static readonly string columnListClause =
            MetaObject.GetMetaProperties<T>()
                .Select(mp => "TVP." + mp.Name)
                .JoinStrings(", ");

        static readonly int estimatedLength = subselect_part1.Length + columnListClause.Length + "@par0".Length + subselect_part5.Length;

        readonly IEnumerable<T> objs;
        readonly string DbTypeName;

        internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<T> list)
        {
            objs = list;
            DbTypeName = dbTypeName;
        }

        public object EquatableValue => Tuple.Create(objs, DbTypeName);

        public int EstimateLength() => estimatedLength;

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            var name = factory.RegisterParameterAndGetName(this);

            SqlFactory.AppendSql(ref factory, subselect_part1);
            SqlFactory.AppendSql(ref factory, columnListClause);
            SqlFactory.AppendSql(ref factory, subselect_part3);
            SqlFactory.AppendSql(ref factory, name);
            SqlFactory.AppendSql(ref factory, subselect_part5);
        }

        public SqlParameter ToSqlParameter(string paramName)
        {
            return new SqlParameter {
                IsNullable = false,
                ParameterName = paramName,
                Value = MetaObject.CreateDataReader(objs),
                SqlDbType = SqlDbType.Structured,
                TypeName = DbTypeName,
            };
        }
    }
}
