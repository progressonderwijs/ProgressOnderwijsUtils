using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class QueryBuilderTools
    {
        [Pure]
        public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<QueryBuilder> projectedColumns, QueryBuilder filterClause, OrderByColumns sortOrder)
            => SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, null);

        [Pure]
        public static QueryBuilder CreatePagedSubQuery(
            QueryBuilder subQuery,
            IEnumerable<QueryBuilder> projectedColumns,
            QueryBuilder filterClause,
            OrderByColumns sortOrder,
            int skipNrows,
            int takeNrows,
            bool sqlOptimizeForUnknown)
        {
            projectedColumns = projectedColumns ?? AllColumns;
            if (!projectedColumns.Any()) {
                throw new InvalidOperationException(
                    "Cannot create subquery without any projected columns: at least one column must be projected (are your columns all virtual?)\nQuery:\n"
                        + subQuery.DebugText());
            }

            var takeRowsParam = QueryBuilder.Param((long)takeNrows);
            var skipNrowsParam = QueryBuilder.Param((long)skipNrows);

            var sortorder = sortOrder;
            var orderClause = sortorder == OrderByColumns.Empty ? SafeSql.SQL($"order by (select 1)") : CreateFromSortOrder(sortorder);
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);

            var topNSubQuery = SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, takeRowsParam + SafeSql.SQL($"+") + skipNrowsParam);
            return SafeSql.SQL($@"
select top ({takeRowsParam}) {projectedColumnsClause}
from (select _row=row_number() over ({orderClause}),
      _g2.*
from (

{topNSubQuery}

) as _g2) t
where _row > {skipNrowsParam}
order by _row
").AppendIf(sqlOptimizeForUnknown, SafeSql.SQL($"option (optimize for unknown)"));
        }

        [Pure]
        static QueryBuilder SubQueryHelper(
            QueryBuilder subquery,
            IEnumerable<QueryBuilder> projectedColumns,
            QueryBuilder filterClause,
            OrderByColumns sortOrder,
            QueryBuilder? topRowsOrNull)
        {
            projectedColumns = projectedColumns ?? AllColumns;

            var topClause = topRowsOrNull != null ? SafeSql.SQL($"top ({topRowsOrNull}) ") : QueryBuilder.Empty;
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            return
                SafeSql.SQL($"select {topClause}{projectedColumnsClause} from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
                    + CreateFromSortOrder(sortOrder);
        }

        //TODO: dit aanzetten voor datasource tests
        // ReSharper disable once UnusedMember.Global
        public static void AssertNoVariableColumns(QueryBuilder queryBuilder)
        {
            var commandText = queryBuilder.CommandText();
            var commandTextWithoutComments = Regex.Replace(
                commandText,
                @"/\*.*?\*/|--.*?$",
                "",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
            if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*")) {
                throw new InvalidOperationException(queryBuilder.GetType().FullName + ": Query may not use * as that might cause runtime exceptions in productie when DB changes:\n" + commandText);
            }
        }

        [Pure]
        static QueryBuilder CreateFromSortOrder(OrderByColumns sortOrder)
        {
            return !sortOrder.Columns.Any()
                ? QueryBuilder.Empty
                : QueryBuilder.CreateDynamic("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
        }

        [Pure]
        static QueryBuilder CreateProjectedColumnsClause(IEnumerable<QueryBuilder> projectedColumns)
            => projectedColumns.Aggregate((a, b) => a.Append(Comma_ColumnSeperator).Append(b));

        static readonly QueryBuilder[] AllColumns = { SafeSql.SQL($"*") };
        static readonly QueryBuilder Comma_ColumnSeperator = SafeSql.SQL($", ");
    }
}
