using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public class QueryBuilder
	{
		static readonly object[] EmptyArray = new object[] { };
		public QueryBuilder() { }
		public QueryBuilder(string q) { ExtendCommand(q, EmptyArray); }
		public QueryBuilder(string q, params object[] par) { ExtendCommand(q, par); }
		readonly List<ISqlString> queryParts = new List<ISqlString>();
		readonly List<Param> queryParams = new List<Param>();
		public string CommandText
		{
			get
			{
				int i = 0;
				foreach (var parm in queryParams.Where(parm => parm.used)) parm.index = i++;

				return queryParts.Select(st => st.SqlString()).JoinStrings();
			}
		}

		public string UnsafeDebugText
		{
			get
			{
				int i = 0;
				foreach (var parm in queryParams.Where(parm => parm.used)) parm.index = i++;

				return queryParts.Select(st => st.DebugString()).JoinStrings();
			}
		}

		//DataCommand MakeCommand(BusinessConnection conn) { return new DataCommand(conn, this); }
		//internal UpdateableQuery MakeUpdateableQuery(BusinessConnection conn) { return new UpdateableQuery(conn, CommandText, ConstructParams()); }

		public SqlParameter[] ConstructParams() { return queryParams.Select(qp => qp.ToParameter()).ToArray(); }

		static readonly Regex paramsRegex = new Regex(@"\{(?<paramRef>\d+)\}|(?<queryText>((?!\{\d+\}).)+)", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		interface ISqlString { string SqlString(); string DebugString();}
		class Param : ISqlString
		{
			private readonly object paramval;
			public int index = -1;
			public bool used;
			public Param(object o) { paramval = o ?? DBNull.Value; }
			public string SqlString() { if (index < 0) throw new InvalidOperationException("param not indexed!"); return "@par" + index; }
			public SqlParameter ToParameter()
			{
				return new SqlParameter
					{
						IsNullable = paramval == DBNull.Value,
						ParameterName = "@par" + index,
						Value = paramval,
					};
			}

			public string DebugString()
			{
				if (paramval == null || paramval == DBNull.Value)
					return "null";
				else if (paramval is string)
					return "'" + (paramval as string).Replace("'", "''");
				else if (paramval is int || paramval is decimal)
					return paramval.ToString();
				else if (paramval is DateTime)
					return ((DateTime)paramval).ToString("'yyyy-MM-dd HH:mm:ss.fffffff'");
				else
					return SqlString();
			}
		}

		class SqlText : ISqlString
		{
			readonly string text;
			public SqlText(string text) { this.text = text; }
			public string SqlString() { return text; }

			public string DebugString() { return text; }
		}
		/// <summary>
		/// Breidt het huidige command uit met de string q en de parameters par. De meegegeven parameters
		/// worden toegevoegd aan de SqlCommand parameters. De parameters worden doorgenummerd na 
		/// de reeds in het aanwezige command parameters.
		/// </summary>
		public void ExtendCommand(string q, params object[] par)
		{
			Param[] parValues = par.Select(val => new Param(val)).ToArray();

			foreach (Match paramRefMatch in paramsRegex.Matches(q))
			{
				if (paramRefMatch.Groups["paramRef"].Success)
				{
					var thisParam = parValues[int.Parse(paramRefMatch.Groups["paramRef"].Value)];
					thisParam.used = true;
					queryParts.Add(thisParam);
				}
				else if (paramRefMatch.Groups["queryText"].Success)
					queryParts.Add(new SqlText(paramRefMatch.Groups["queryText"].Value));
				else if (paramRefMatch.Success)
					throw new ProgressNetException("Impossible: regex must have a matching group if matching.");
			}
			queryParams.AddRange(parValues.Where(p => p.used));
		}

		public void ExtendCommand(QueryBuilder coreQuery)
		{
			queryParts.AddRange(coreQuery.queryParts);
			queryParams.AddRange(coreQuery.queryParams);
		}

		public void ExtendCommandWithFilter(Filter filter, Dictionary<string, string> computedcolumns)
		{
			List<object> parlist = new List<object>();
			string q = "and " + filter.ToSqlString(parlist) + " ";
			if (computedcolumns != null && computedcolumns.Any())
				q = replacedCols(q, computedcolumns);
			ExtendCommand(q, parlist.ToArray());
		}

		static string replacedCols(string q, Dictionary<string, string> colMap)
		{
			return Regex.Replace(q, "(?<![a-zA-Z0-9_.])(" + colMap.Keys.JoinStrings("|") + ")(?![a-zA-Z0-9_.])", m => colMap[m.Value]);
		}

#if false		
		static string replacedCols(string q, Dictionary<string, string> colMap)
		{
			return colMap.Aggregate(q,
					(current, computedcolumn) => Regex.Replace(current, string.Format(@"(?<pre>[^a-zA-Z0-9_.]){0}(?<post>[^a-zA-Z0-9_.])", computedcolumn.Key),
							match => string.Format("{0}{1}{2}", match.Groups["pre"].Value, computedcolumn.Value, match.Groups["post"].Value)
						));
		}
#endif

		/// <summary>
		/// Voegt sortering clause toe aan het huidige SqlCommand. Deze functie voegt ook
		/// de "order by" toe!. Werkt voorlopig nog even zonder parameters.
		/// </summary>
		public void ExtendCommandWithSortOrder(OrderByColumns mostSignificantColumnFirst)
		{
			if (mostSignificantColumnFirst.Columns.Any())
				ExtendCommand("order by " + mostSignificantColumnFirst.Columns.Select(sc => sc.SqlSortString).JoinStrings(", "));
		}

	}
}
