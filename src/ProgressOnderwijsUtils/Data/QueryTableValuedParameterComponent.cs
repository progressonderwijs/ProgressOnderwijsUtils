﻿using System;
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

        const string subselect_part1 = "(select ";
        const string subselect_part3 = " from ";
        const string subselect_part5 = " TVP)";
        readonly string DbTypeName;
        readonly IEnumerable<TIn> values;
        readonly Func<IEnumerable<TIn>, TOut[]> projection;

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
            var paramName = factory.RegisterParameterAndGetName(this);

            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part1);
            ParameterizedSqlFactory.AppendSql(ref factory, columnListClause);
            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part3);
            ParameterizedSqlFactory.AppendSql(ref factory, paramName);
            ParameterizedSqlFactory.AppendSql(ref factory, subselect_part5);
        }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            var objs = projection(values);
            paramArgs.Value = new MetaObjectDataReader<TOut>(objs, CancellationToken.None);
            paramArgs.TypeName = DbTypeName;
        }
    }
}
