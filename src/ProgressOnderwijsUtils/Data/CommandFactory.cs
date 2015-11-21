using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    sealed class CommandFactory
    {
        readonly StringBuilder queryText = new StringBuilder();
        FastArrayBuilder<SqlParameter> parmetersInOrder = FastArrayBuilder<SqlParameter>.Create();
        readonly Dictionary<IQueryParameter, string> lookup = new Dictionary<IQueryParameter, string>();

        CommandFactory(IEnumerable<IQueryComponent> components)
        {
            foreach (var component in components) {
                queryText.Append(component.ToSqlString(this));
            }
        }
        internal CommandFactory() { }

        public static SqlCommand BuildQuery(IEnumerable<IQueryComponent> components, SqlConnection conn, int commandTimeout)
        {
            var query = new CommandFactory(components);
            return query.CreateCommand(conn, commandTimeout);
        }

        public static string BuildQueryText(IEnumerable<IQueryComponent> components)
        {
            var query = new CommandFactory(components);
            return query.queryText.ToString();
        }

        internal SqlCommand CreateCommand(SqlConnection conn, int commandTimeout)
        {
            var commandText = queryText.ToString();
            var sqlParameters = parmetersInOrder.ToArray();
            return CreateCommand(conn, commandTimeout, commandText, sqlParameters);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        static SqlCommand CreateCommand(SqlConnection sqlconn, int commandTimeout, string commandText, SqlParameter[] parameters)
        {
            bool finishedOk = false;
            var command = new SqlCommand();
            try {
                command.Connection = sqlconn;
#if USE_RAW_TRANSACTIONS
				command.Transaction = conn.SqlTransaction;
#endif
                command.CommandTimeout = commandTimeout; //60 by default
                command.CommandText = commandText;
                command.Parameters.AddRange(parameters);
                finishedOk = true;
                return command;
            } finally {
                if (!finishedOk) {
                    command.Dispose();
                }
            }
        }

        const int ParameterNameCacheSize = 20;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize)
                .Select(IndexToParameterName)
                .ToArray();

        static string IndexToParameterName(int parameterIndex) => "@par" + parameterIndex.ToStringInvariant();

        public string GetNameForParam(IQueryParameter o)
        {
            string paramName;
            if (!lookup.TryGetValue(o, out paramName)) {
                var parameterIndex = lookup.Count;
                paramName = parameterIndex < CachedParameterNames.Length
                    ? CachedParameterNames[parameterIndex]
                    : IndexToParameterName(parameterIndex);
                parmetersInOrder.Add(o.ToSqlParameter(paramName));
                lookup.Add(o, paramName);
            }
            return paramName;
        }

        internal void AppendSql(string sql)
        {
            queryText.Append(sql);
        }
    }
}
