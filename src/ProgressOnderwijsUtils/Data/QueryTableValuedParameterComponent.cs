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
        static readonly string columnListClause =
            MetaObject.GetMetaProperties<T>()
                .Select(mp => "TVP." + mp.Name)
                .JoinStrings(", ");

        readonly IEnumerable<T> objs;
        readonly string DbTypeName;

        internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<T> list)
        {
            objs = list;
            DbTypeName = dbTypeName;
        }

        public object EquatableValue => Tuple.Create(objs, DbTypeName);

        public int EstimateLength()
        {
            return "(select par0.querytablevalue from @par0 par0)".Length;
        }

        public void AppendTo(ref CommandFactory factory)
        {
            var name = factory.GetNameForParam(this);

            var sqlString = $"(select {columnListClause} from {name} TVP)";

            factory.AppendSql(sqlString, 0, sqlString.Length);
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
