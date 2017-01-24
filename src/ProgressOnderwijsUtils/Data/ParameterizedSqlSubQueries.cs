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
            // Onderstaande query zou ook met een join op de selectie als TVP uitgedrukt kunnen worden zodat we dezelfde query als voor de plural key kunnen gebruiken.
            // Maar in de praktijk blijkt dat qua performance vies tegen te vallen:
            //
            // Aanmeldingen | page  | all
            // ==============================
            // where clause | 0,03s | 3,1s
            // join clause  | 2,8s  | 3,2s
            //
            // BRON-HO      | page  | all
            // ==============================
            // where clause | 0,02s | Timeout
            // join clause  | 3,5s  | Timeout
            //
            // Vorderingen  | page  | all
            // ==============================
            // where clause | 0,01s | Timeout
            // join clause  | 0,01s | Timeout

            var projectedColumns = AllColumns;
            var selectedProjectedColumnsClause = CreateProjectedColumnsClause(projectedColumns.Select(col => SQL($"_g2.{col}")));
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);

            return SQL($@"
                /* multi-selection (singular key) for current filters */
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
                where _g2.{keyColumn} in {selection}
                {CreateFromSortOrder(sortOrder)}
            ");
        }

        [Pure]
        public static ParameterizedSql CreateSelectedByPluralKeySubQuery(
            ParameterizedSql subQuery,
            ParameterizedSql filterClause,
            OrderByColumns sortOrder,
            IEnumerable<ParameterizedSql> keyColumns,
            ParameterizedSql selection)
        {
            var projectedColumns = AllColumns;
            var selectedProjectedColumnsClause = CreateProjectedColumnsClause(projectedColumns.Select(col => SQL($"_g2.{col}")));
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            var joinClause = keyColumns
                .Select(col => SQL($"k.{col} = _g2.{col}"))
                .Aggregate((a, b) => SQL($"{a} and {b}"));

            return SQL($@"
                /* multi-selection (plural key) for current filters */
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
                join {selection} k on {joinClause}
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
