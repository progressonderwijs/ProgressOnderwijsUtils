using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
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
}
