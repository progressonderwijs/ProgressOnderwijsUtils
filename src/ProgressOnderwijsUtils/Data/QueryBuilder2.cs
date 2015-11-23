using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Data
{
    public struct QueryBuilder2
    {
        readonly IBuildableQuery impl;
        internal QueryBuilder2(IBuildableQuery impl)
        {
            this.impl = impl;
        }

        internal void AppendTo(CommandFactory factory) => impl.AppendTo(factory);

        public SqlCommand CreateSqlCommand(SqlCommandCreationContext conn)
        {
            var factory = new CommandFactory();
            impl.AppendTo(factory);
            return factory.CreateCommand(conn.Connection, conn.CommandTimeoutInS);
        }
    }

    interface IBuildableQuery
    {
        void AppendTo(CommandFactory factory);
    }

    public static class SqlFactory
    {
        internal static QueryBuilder2 ToQuery(this IBuildableQuery q) => new QueryBuilder2(q);
        public static QueryBuilder2 SQL(FormattableString interpolatedQuery) => new InterpolatedSqlFragment(interpolatedQuery).ToQuery();

    }

    class InterpolatedSqlFragment : IBuildableQuery
    {
        readonly FormattableString interpolatedQuery;

        public InterpolatedSqlFragment(FormattableString interpolatedQuery)
        {
            this.interpolatedQuery = interpolatedQuery;
        }

        public void AppendTo(CommandFactory factory)
        {

            if (interpolatedQuery == null) {
                throw new ArgumentNullException(nameof(interpolatedQuery));
            }

            var str = interpolatedQuery.Format;

            var pos = 0;
            while (true) {
                var paramRefMatch = ParamRefNextMatch(str, pos);
                if (paramRefMatch.WasNotFound())
                    break;
                factory.AppendSql(str, pos, paramRefMatch.StartIndex - pos); 
                var argumentIndex = int.Parse(str.Substring(paramRefMatch.StartIndex + 1, paramRefMatch.EndIndex - paramRefMatch.StartIndex - 2), NumberStyles.None, CultureInfo.InvariantCulture);
                var argument = interpolatedQuery.GetArgument(argumentIndex);
                if (argument is QueryBuilder2) {
                    ((QueryBuilder2)argument).AppendTo(factory);
                } else {
                    QueryComponent.CreateParam(argument).AppendTo(factory);
                }
                pos = paramRefMatch.EndIndex;
            }
            factory.AppendSql(str, pos, str.Length - pos);
        }

        static SubstringPosition ParamRefNextMatch(string query, int pos)
        {
            if (pos >= query.Length) {
                return SubstringPosition.NotFound;
            }
            while (query[pos] != '{') {
                pos++;
                if (pos >= query.Length) {
                    return SubstringPosition.NotFound;
                }
            }
            var startPos = pos;
            pos++;
            if (pos >= query.Length) {
                return SubstringPosition.NotFound;
            }
            while (pos < query.Length) {
                if (query[pos] == '}') {
                    return new SubstringPosition { StartIndex = startPos, EndIndex = pos + 1 };
                } else {
                    pos++;
                }
            }
            return SubstringPosition.NotFound;
        }

        struct SubstringPosition
        {
            public int StartIndex, EndIndex;
            public bool WasNotFound() => StartIndex < 0;
            public static readonly SubstringPosition NotFound = new SubstringPosition { StartIndex = -1 };
        }


    }
}
