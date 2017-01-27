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
        {
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns ?? AllColumns);
            return
                SQL($"select {projectedColumnsClause} from (\r\n{subQuery}\r\n) as _g1 where {filterClause}\r\n{CreateOrderByClause(sortOrder)}");
        }

        [Pure]
        public static ParameterizedSql CreatePagedSubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            int takeNrows)
        {
            return
                SQL($"select top ({(long)takeNrows}) * from (\r\n{subQuery}\r\n) as _g1 where {filterClause}\r\n{CreateOrderByClause(sortOrder)}");
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
        public static ParameterizedSql CreateOrderByClause(OrderByColumns sortOrder)
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
