﻿using System.Linq;
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
        readonly T[] objs;
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

            //Insert length category token in TVP sql output, so that the query
            //optimizer uses differing query plans for arrays.  In effect, every
            //factor of 8 a new query plan is used.
            SqlFactory.AppendSql(ref factory, querySizeToken[LengthToCategory(objs.Length)]);
        }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
        {
            paramArgs.Value = MetaObject.CreateDataReader(objs);
            paramArgs.TypeName = DbTypeName;
        }

        static int LengthToCategory(int length) => Utils.LogBase2RoundedDown((uint)length) / 3;
        static readonly int maxCategory = LengthToCategory(int.MaxValue);
        static readonly string[] querySizeToken = Enumerable.Range(0, maxCategory + 1).Select(n => $"/*{n}*/").ToArray();
    }
}
