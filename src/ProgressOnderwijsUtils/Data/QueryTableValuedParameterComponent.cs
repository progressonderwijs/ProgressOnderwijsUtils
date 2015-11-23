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
        readonly IEnumerable<T> objs;
        readonly string DbTypeName;

        internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<T> list)
        {
            objs = list;
            DbTypeName = dbTypeName;
        }

        public object EquatableValue => objs;

        public string ToSqlString(ref CommandFactory qnum)
        {
            var name = qnum.GetNameForParam(this);
            var alias = name.Substring(1);

            // select par0.querytablevalue from @par0 par0, par0 is alias for @par0
            return $"(select {alias}.querytablevalue from {name} {alias})";
        }

        public int EstimateLength()
        {
            return "(select par0.querytablevalue from @par0 par0)".Length;
        }

        public void AppendTo(ref CommandFactory factory)
        {
            var sqlString = ToSqlString(ref factory);
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

        public string ToDebugText(Taal? taalOrNull) => "(" + ObjectToCode.ComplexObjectToPseudoCode(objs) + ")";
        public bool Equals(IQueryComponent other) => Equals((object)other);

        public override bool Equals(object other)
        {
            return ReferenceEquals(this, other)
                || (other is QueryTableValuedParameterComponent<T>) && Equals(DbTypeName, ((QueryTableValuedParameterComponent<T>)other).DbTypeName)
                    && (ReferenceEquals(objs, ((QueryTableValuedParameterComponent<T>)other).objs) || objs.SequenceEqual(((QueryTableValuedParameterComponent<T>)other).objs));
        }

        public override int GetHashCode()
        {
            return objs.GetHashCode() + 37 * DbTypeName.GetHashCode() + 200;
        } //paramval never null!
    }
}
