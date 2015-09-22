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
            return
                components
                    .Aggregate(new CommandFactory(), (factory, component) => factory.AppendQueryComponent(component));
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

        public int GetNumberForParam(IQueryParameter o)
        {
            int retval;
            if (!lookup.TryGetValue(o, out retval)) {
                parmetersInOrder.Add(o);
                lookup.Add(o, retval = lookup.Count);
            }
            return retval;
        }

        SqlParameter[] GenerateSqlParameters() => parmetersInOrder.Select(par => par.ToSqlParameter(GetNumberForParam(par))).ToArray();
        string GenerateCommandText() => queryText.ToString();
    }
}
