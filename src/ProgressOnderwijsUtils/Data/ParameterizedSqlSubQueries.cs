using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlSubQueries
    {
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
            => !sortOrder.Columns.Any()
                ? ParameterizedSql.Empty
                : ParameterizedSql.CreateDynamic("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
    }
}
