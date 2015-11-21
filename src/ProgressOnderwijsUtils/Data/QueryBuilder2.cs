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
    }
}
