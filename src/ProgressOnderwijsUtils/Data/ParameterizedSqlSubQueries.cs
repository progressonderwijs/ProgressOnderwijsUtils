﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlSubQueries
    {
        [Pure]
        public static ParameterizedSql CreateSubQuery(ParameterizedSql subQuery, IEnumerable<ParameterizedSql> projectedColumns, ParameterizedSql filterClause, OrderByColumns sortOrder)
            => SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, null);

        [Pure]
        public static ParameterizedSql CreatePagedSubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            int takeNrows)
        {
            return SubQueryHelper(subQuery, AllColumns, filterClause, sortOrder, ParameterizedSql.Param((long)takeNrows));
        }

        [Pure]
        public static ParameterizedSql CreateSelectedBySingularKeySubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            ParameterizedSql keyColumn,
            ParameterizedSql selection)
        {
            var joinClause = SQL($"k.querytablevalue = _g2.{keyColumn}");
            return CreateSelectedSubQuery(subQuery, filterClause, sortOrder, joinClause, selection);
        }

        [Pure]
        public static ParameterizedSql CreateSelectedByPluralKeySubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            IEnumerable<ParameterizedSql> keyColumns,
            ParameterizedSql selection)
        {
            var joinClause = keyColumns
                .Select(col => SQL($"k.{col} = _g2.{col}"))
                .Aggregate((a, b) => SQL($"{a} and {b}"));
            return CreateSelectedSubQuery(subQuery, filterClause, sortOrder, joinClause, selection);
        }

        [Pure]
        static ParameterizedSql CreateSelectedSubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            ParameterizedSql selectionClause,
            ParameterizedSql selection)
        {
            var projectedColumns = AllColumns;
            var selectedProjectedColumnsClause = CreateProjectedColumnsClause(projectedColumns.Select(col => SQL($"_g2.{col}")));
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);

            return SQL($@"
                /* multi-selection for current filters */
                select
                    {selectedProjectedColumnsClause}
                from (
                    select 
                        {projectedColumnsClause}
                    from (
                        {subQuery}
                    ) as _g1
                    where {filterClause}
                ) as _g2
                join {selection} k on {selectionClause}
                {CreateFromSortOrder(sortOrder)}
            ");
        }

        [Pure]
        static ParameterizedSql SubQueryHelper(
            ParameterizedSql subquery,
            IEnumerable<ParameterizedSql> projectedColumns,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            ParameterizedSql? topRowsOrNull)
        {
            projectedColumns = projectedColumns ?? AllColumns;
            var topClause = topRowsOrNull != null ? SQL($"top ({topRowsOrNull}) ") : ParameterizedSql.Empty;
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            return
                SQL($"select {topClause}{projectedColumnsClause} from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
                    + CreateFromSortOrder(sortOrder);
        }

        //TODO: dit aanzetten voor datasource tests
        // ReSharper disable once UnusedMember.Global
        public static void AssertNoVariableColumns(ParameterizedSql parameterizedSql)
        {
            var commandText = parameterizedSql.CommandText();
            var commandTextWithoutComments = Regex.Replace(
                commandText,
                @"/\*.*?\*/|--.*?$",
                "",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
            if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*")) {
                throw new InvalidOperationException(parameterizedSql.GetType().FullName + ": Query may not use * as that might cause runtime exceptions in productie when DB changes:\n" + commandText);
            }
        }

        [Pure]
        static ParameterizedSql CreateFromSortOrder(OrderByColumns sortOrder)
        {
            return !sortOrder.Columns.Any()
                ? ParameterizedSql.Empty
                : ParameterizedSql.CreateDynamic("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
        }

        [Pure]
        static ParameterizedSql CreateProjectedColumnsClause(IEnumerable<ParameterizedSql> projectedColumns)
            => projectedColumns.Aggregate((a, b) => SQL($"{a}\n, {b}"));

        static readonly ParameterizedSql[] AllColumns = { SQL($"*") };
    }
}
