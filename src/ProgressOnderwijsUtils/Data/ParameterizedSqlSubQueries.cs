using System;
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
            => SubQueryHelper(subQuery, filterClause, sortOrder, null);

        [Pure]
        public static ParameterizedSql CreatePagedSubQuery(
            ParameterizedSql subQuery,
            IEnumerable<ParameterizedSql> projectedColumns,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            int takeNrows)
        {
            projectedColumns = projectedColumns ?? AllColumns;
            if (!projectedColumns.Any()) {
                throw new InvalidOperationException(
                    "Cannot create subquery without any projected columns: at least one column must be projected (are your columns all virtual?)\nQuery:\n"
                        + subQuery.DebugText());
            }

            var takeRowsParam = ParameterizedSql.Param((long)takeNrows);

            return SubQueryHelper(subQuery, filterClause, sortOrder, takeRowsParam);
        }

        [Pure]
        static ParameterizedSql SubQueryHelper(
            ParameterizedSql subquery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            ParameterizedSql? topRowsOrNull)
        {
            var topClause = topRowsOrNull != null ? SQL($"top ({topRowsOrNull}) ") : ParameterizedSql.Empty;
            return
                SQL($"select {topClause} * from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
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

        static readonly ParameterizedSql[] AllColumns = { SQL($"*") };
    }
}
