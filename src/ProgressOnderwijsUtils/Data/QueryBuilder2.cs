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

        internal void AppendTo(ref CommandFactory factory) => impl.AppendTo(ref factory);

        public SqlCommand CreateSqlCommand(SqlCommandCreationContext conn)
        {
            var factory = new CommandFactory(impl.EstimateLength());
            impl.AppendTo(ref factory);
            return factory.CreateCommand(conn.Connection, conn.CommandTimeoutInS);
        }
    }

    interface IBuildableQuery
    {
        void AppendTo(ref CommandFactory factory);
        int EstimateLength();
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

        public void AppendTo(ref CommandFactory factory)
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
                var argument = interpolatedQuery.GetArgument(paramRefMatch.ReferencedParameterIndex);
                if (argument is QueryBuilder2) {
                    ((QueryBuilder2)argument).AppendTo(ref factory);
                } else {
                    QueryComponent.CreateParam(argument).AppendTo(ref factory);
                }
                pos = paramRefMatch.EndIndex;
            }
            factory.AppendSql(str, pos, str.Length - pos);
        }

        static ParamRefSubString ParamRefNextMatch(string query, int pos)
        {
            while (pos < query.Length) {
                char c = query[pos];
                if (c == '{') {
                    var startPos = pos;
                    int num = 0;
                    for (pos++; pos < query.Length; pos++) {
                        c = query[pos];
                        if (c >= '0' && c <= '9') {
                            num = num * 10 + (c - '0');
                            continue;
                        } else if (c == '}') {
                            return new ParamRefSubString {
                                StartIndex = startPos,
                                EndIndex = pos + 1,
                                ReferencedParameterIndex = num
                            };
                        } else {
                            break;
                        }
                    }
                }
                pos++;
            }
            return ParamRefSubString.NotFound;
        }

        public int EstimateLength()
        {
            return interpolatedQuery.Format.Length + interpolatedQuery.ArgumentCount*2;
            // converting {0} into @par0 adds 2 to length
        }

        struct ParamRefSubString
        {
            public int StartIndex, EndIndex, ReferencedParameterIndex;
            public bool WasNotFound() => ReferencedParameterIndex < 0;
            public static readonly ParamRefSubString NotFound = new ParamRefSubString { ReferencedParameterIndex = -1 };
        }
    }
}
