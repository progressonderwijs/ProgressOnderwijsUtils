using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    sealed class QueryTableValuedParameterComponent<TIn, TOut> : IQueryParameter, ISqlComponent
        where TOut : IMetaObject
    {
        static readonly string columnListClause =
            MetaObject.GetMetaProperties<TOut>()
                .Select(mp => "TVP." + mp.Name)
                .JoinStrings(", ");

        const string subselect_part1 = "(select ";
        const string subselect_part3 = " from ";
        const string subselect_part5 = " TVP)";
        readonly string DbTypeName;
        readonly IEnumerable<TIn> values;
        readonly Func<IEnumerable<TIn>, TOut[]> projection;
        int cachedProjectedLength;

        [NotNull]
        public object EquatableValue => Tuple.Create(values, DbTypeName);

        internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<TIn> values, Func<IEnumerable<TIn>, TOut[]> projection)
        {
            this.values = values;
            this.projection = projection;
            DbTypeName = dbTypeName;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part1);
            ParameterizedSqlFactory.AppendSql(ref factory, columnListClause);
            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part3);
            ParameterizedSqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(this));
            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part5);

            if (cachedProjectedLength > 0) {
                //Insert length category token in TVP sql output, so that the query
                //optimizer uses differing query plans for arrays.  In effect, every
                //factor of 8 a new query plan is used.
                ParameterizedSqlFactory.AppendSql(ref factory, querySizeToken[LengthToCategory(cachedProjectedLength)]);
            }
        }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            var objs = projection(values);
            paramArgs.Value = new MetaObjectDataReader<TOut>(objs);
            paramArgs.TypeName = DbTypeName;
            cachedProjectedLength = objs.Length;
        }

        static int LengthToCategory(int length) => Utils.LogBase2RoundedDown((uint)length) / 3;
        static readonly int maxCategory = LengthToCategory(int.MaxValue);
        static readonly string[] querySizeToken = Enumerable.Range(0, maxCategory + 1).Select(n => $"/*{n}*/").ToArray();
    }
}
