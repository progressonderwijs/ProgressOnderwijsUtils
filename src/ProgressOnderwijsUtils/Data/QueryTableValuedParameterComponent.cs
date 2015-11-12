using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using ExpressionToCodeLib;

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

        public string ToSqlString(CommandFactory qnum)
        {
            var number = qnum.GetNumberForParam(this);
            // select par0.val from @par0 par0, par0 is alias for @par0
            return $"(select par{number}.val from @par{number} par{number})";
        }

        public SqlParameter ToSqlParameter(int paramnum)
        {
            return new SqlParameter {
                IsNullable = false,
                ParameterName = "@par" + paramnum,
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
