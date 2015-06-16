using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public static QueryBuilder Append(this QueryBuilder source, string str, params object[] parms)
        {
            return source + QueryBuilder.Create(Environment.NewLine + str + " ", parms);
        }

        [Pure]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, QueryBuilder extra)
        {
            return condition ? source.Append(extra) : source;
        }

        [Pure]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, Func<QueryBuilder> extra)
        {
            return condition ? source.Append(extra()) : source;
        }

        [Pure]
        public static QueryBuilder AppendIf(this QueryBuilder source, bool condition, string str, params object[] parms)
        {
            return condition ? source.Append(str, parms) : source;
        }
    }
}
