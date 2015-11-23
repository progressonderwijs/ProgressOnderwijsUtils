using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    struct CommandFactory
    {
        readonly StringBuilder queryText;
        readonly SqlCommand command;
        readonly SqlParameterCollection commandParameters;
        readonly Dictionary<object, string> lookup;

        internal CommandFactory(int estimatedLength) {
            queryText = new StringBuilder(estimatedLength);
            command = new SqlCommand();
            commandParameters = command.Parameters;
            lookup = new Dictionary<object, string>();
        } 

        public static SqlCommand BuildQuery(IEnumerable<IQueryComponent> components, SqlConnection conn, int commandTimeout)
        {
            var query = new CommandFactory(32);
            foreach (var component in components) {
                component.AppendTo(ref query);
            }
            return query.CreateCommand(conn, commandTimeout);
        }

        public static string BuildQueryText(IEnumerable<IQueryComponent> components)
        {
            var query = new CommandFactory(32);
            foreach (var component in components) {
                component.AppendTo(ref query);
            }
            return query.queryText.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal SqlCommand CreateCommand(SqlConnection conn, int commandTimeout)
        {
            command.Connection = conn;
#if USE_RAW_TRANSACTIONS
			command.Transaction = conn.SqlTransaction;
#endif
            command.CommandTimeout = commandTimeout; //60 by default
            command.CommandText = queryText.ToString();
            return command;
        }

        const int ParameterNameCacheSize = 20;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize)
                .Select(IndexToParameterName)
                .ToArray();

        static string IndexToParameterName(int parameterIndex) => "@par" + parameterIndex.ToStringInvariant();

        public string GetNameForParam<T>(T o)
            where T: IQueryParameter
        {
            string paramName;
            if (!lookup.TryGetValue(o.EquatableValue, out paramName)) {
                var parameterIndex = lookup.Count;
                paramName = parameterIndex < CachedParameterNames.Length
                    ? CachedParameterNames[parameterIndex]
                    : IndexToParameterName(parameterIndex);
                commandParameters.Add(o.ToSqlParameter(paramName));
                lookup.Add(o.EquatableValue, paramName);
            }
            return paramName;
        }

        internal void AppendSql(string sql, int startIndex, int length)
        {
            queryText.Append(sql, startIndex, length);
        }
    }
}
