using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
    sealed class CommandFactory
    {
        CommandFactory() { }

        public static SqlCommand BuildQuery(IEnumerable<IQueryComponent> components, SqlConnection conn, int commandTimeout)
        {
            var query = ProcessQuery(components);

            return CreateCommand(conn, commandTimeout, query.GenerateCommandText(), query.GenerateSqlParameters());
        }

        static CommandFactory ProcessQuery(IEnumerable<IQueryComponent> components)
        {
            var commandFactory = new CommandFactory();
            foreach (var component in components)
                commandFactory.AppendQueryComponent(component);
            return commandFactory;
        }

        public static string BuildQueryText(IEnumerable<IQueryComponent> components)
        {
            var query = ProcessQuery(components);
            return query.GenerateCommandText();
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

        readonly StringBuilder queryText = new StringBuilder();
        readonly List<IQueryParameter> parmetersInOrder = new List<IQueryParameter>();
        readonly Dictionary<IQueryParameter, int> lookup = new Dictionary<IQueryParameter, int>();

        public CommandFactory AppendQueryComponent(IQueryComponent component)
        {
            queryText.Append(component.ToSqlString(this));
            return this;
        }

        static readonly string[] parNames = Enumerable.Range(0, 20).Select(NumToParName).ToArray();
        static string NumToParName(int num)=> "@par" + num.ToStringInvariant();


        public string GetNameForParam(IQueryParameter o)
        {
            int parameterIndex;
            if (!lookup.TryGetValue(o, out parameterIndex)) {
                parmetersInOrder.Add(o);
                parameterIndex = lookup.Count;
                lookup.Add(o, parameterIndex);
            }
            return parameterIndex < parNames.Length ? parNames[parameterIndex] : NumToParName(parameterIndex);

        }

        SqlParameter[] GenerateSqlParameters() => parmetersInOrder.Select(par => par.ToSqlParameter(GetNameForParam(par))).ToArray();
        string GenerateCommandText() => queryText.ToString();
    }
}
