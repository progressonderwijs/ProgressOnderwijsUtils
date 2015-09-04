using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class QueryBuilderExtensions
    {
        [Pure]
        public static QueryBuilder Append(this QueryBuilder source, QueryBuilder extra)
        {
            return source + Environment.NewLine + extra;
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

        [Pure]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, string str, params object[] parms)
        {
            return condition ? source + Environment.NewLine + QueryBuilder.CreateDynamic(str, parms) + " " : source;
        }
    }
}
