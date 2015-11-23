using System;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public static class QueryBuilderExtensions
    {
        static readonly QueryBuilder newline = SQL($"\r\n");

        [Pure]
        public static QueryBuilder Append(this QueryBuilder source, QueryBuilder extra)
        {
            return source + newline + extra;
        }

        [Pure]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, QueryBuilder extra)
        {
            return condition ? source.Append(extra) : source;
        }

        [Pure, UsefulToKeep("Library function, other overloads used")]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, Func<QueryBuilder> extra)
        {
            return condition ? source.Append(extra()) : source;
        }
    }
    public static class QueryBuilder0Extensions
    {
        static readonly QueryBuilder0 newline = QueryBuilder0.SQL($"\r\n");

        [Pure]
        public static QueryBuilder0 Append(this QueryBuilder0 source, QueryBuilder0 extra)
        {
            return source + newline + extra;
        }

        [Pure]
        public static QueryBuilder0 AppendIf(this QueryBuilder0 source, bool condition, QueryBuilder0 extra)
        {
            return condition ? source.Append(extra) : source;
        }

        [Pure, UsefulToKeep("Library function, other overloads used")]
        public static QueryBuilder0 AppendIf(this QueryBuilder0 source, bool condition, Func<QueryBuilder0> extra)
        {
            return condition ? source.Append(extra()) : source;
        }
    }

}
