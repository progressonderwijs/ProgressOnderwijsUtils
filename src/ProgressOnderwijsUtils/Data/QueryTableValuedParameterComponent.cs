using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        readonly string DbTypeName;
        readonly IEnumerable<TIn> values;
        readonly Func<IEnumerable<TIn>, TOut[]> projection;
        int cachedLength = -1;

        [NotNull]
        public object EquatableValue => Tuple.Create(values, DbTypeName);

        internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<TIn> values, Func<IEnumerable<TIn>, TOut[]> projection)
        {
            this.values = values;
            this.projection = projection;
            DbTypeName = dbTypeName;
            if (values is IReadOnlyList<TIn> arrayLike) {
                cachedLength = arrayLike.Count; //if you pass in something with a length, we can reliably determine its size
            }
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            var paramName = factory.RegisterParameterAndGetName(this);
            ParameterizedSqlFactory.AppendSql(ref factory, "(select ");
            if (cachedLength >= 0) {
                var roundedUpToNearestPowerOfFourOrZeroOrMinusOne = cachedLength == 0 ? 0
                    : cachedLength > 1 << 20 ? -1
                        : 1 << (Utils.LogBase2RoundedUp((uint)cachedLength) + 1 >> 1 << 1);
                if (cachedLength > roundedUpToNearestPowerOfFourOrZeroOrMinusOne) {
                    throw new Exception("Internal error: " + cachedLength + " > " + roundedUpToNearestPowerOfFourOrZeroOrMinusOne);
                }
                if (roundedUpToNearestPowerOfFourOrZeroOrMinusOne >= 0) {
                    ParameterizedSqlFactory.AppendSql(ref factory, "top(");
                    ParameterizedSqlFactory.AppendSql(ref factory, roundedUpToNearestPowerOfFourOrZeroOrMinusOne.ToString());
                    ParameterizedSqlFactory.AppendSql(ref factory, ") ");
                }
            }

            ParameterizedSqlFactory.AppendSql(ref factory, columnListClause);
            ParameterizedSqlFactory.AppendSql(ref factory, " from ");
            ParameterizedSqlFactory.AppendSql(ref factory, paramName);
            ParameterizedSqlFactory.AppendSql(ref factory, " TVP)");
        }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            var objs = projection(values);
            cachedLength = objs.Length;
            //if you pass in something without a length, then only the first consumer gets a size;
            paramArgs.Value = new MetaObjectDataReader<TOut>(objs, CancellationToken.None);
            paramArgs.TypeName = DbTypeName;
        }
    }

    sealed class SingletonQueryTableValuedParameterComponent<TOut> : ISqlComponent
        where TOut : IMetaObject
    {
        readonly TOut row;

        internal SingletonQueryTableValuedParameterComponent(TOut row)
        {
            this.row = row;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            bool isFirst = true;
            ParameterizedSqlFactory.AppendSql(ref factory, "(select ");
            foreach (var metaProp in MetaObject.GetMetaProperties<TOut>()) {
                if (metaProp.CanRead) {
                    if (!isFirst) {
                        ParameterizedSqlFactory.AppendSql(ref factory, ", ");
                    } else {
                        isFirst = false;
                    }

                    ParameterizedSqlFactory.AppendSql(ref factory, metaProp.Name);
                    ParameterizedSqlFactory.AppendSql(ref factory, " = ");
                    QueryScalarParameterComponent.AppendScalarParameter(ref factory, metaProp.Getter(row));
                }
            }
            ParameterizedSqlFactory.AppendSql(ref factory, ")");
        }
    }
}
