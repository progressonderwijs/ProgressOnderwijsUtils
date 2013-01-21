using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Data
{
	public sealed class QueryTracer
	{
		#region Mutable Members
		public int QueryActiveNesting { get { return QueryCount - QueriesCompleted; } }
		public int QueryCount { get { return queryCount; } }
		public int QueriesCompleted { get { return queriesCompleted; } }
		int queryCount;
		int queriesCompleted;
		public readonly bool IncludeSensitiveInfo;
		public QueryTracer(bool inlcudeSensiveInfo) { IncludeSensitiveInfo = inlcudeSensiveInfo; }

		public readonly object Sync = new object();
		readonly List<Tuple<TimeSpan, Func<string>>> allqueries = new List<Tuple<TimeSpan, Func<string>>>();
		Tuple<TimeSpan, Func<string>> slowest = Tuple.Create(default(TimeSpan), (Func<string>)(() => "(none)"));

		public Func<string> SlowestQuery { get { return slowest.Item2; } }
		public TimeSpan SlowestQueryDuration { get { return slowest.Item1; } }
		public IEnumerable<Tuple<string, TimeSpan>> AllQueries { get { return allqueries.Select(tup => Tuple.Create(tup.Item2(), tup.Item1)); } }

		public TimeSpan AllQueryDurations { get; private set; }
		#endregion

		public IDisposable StartQueryTimer(Func<string> commandText)
		{
			if (commandText == null) throw new ArgumentNullException("commandText");

			return new QueryTimer(this, commandText);
		}

		public IDisposable StartQueryTimer(string commandText)
		{
			if (commandText == null) throw new ArgumentNullException("commandText");

			return new QueryTimer(this, () => commandText);
		}

		public IDisposable StartQueryTimer(SqlCommand sqlCommand)
		{
			string prefix = !IncludeSensitiveInfo ? "" :
	sqlCommand.Parameters.Cast<SqlParameter>().Select(par => "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par) + ";\nSET " + par.ParameterName + " = " + SqlValueString(par.Value) + ";\n").JoinStrings();
			//when machine is in LAN, we're not running on the production server: assume it's OK to include potentially confidential info like passwords in debug output.
			return StartQueryTimer(prefix + sqlCommand.CommandText);
		}
		static string SqlParamTypeString(SqlParameter par) { return par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : ""); }

		public static string SqlValueString(object p) // Not Secure, just a debug tool!
		{
			if (p is DBNull || p == null)
				return "NULL";
			else if (p is DateTime)
				return "'" + ((DateTime)p).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture) + "'";
			else if (p is string)
				return "'" + ((string)p).Replace("'", "''") + "'";
			else if (p is long)
				return ((long)p).ToStringInvariant();
			else if (p is int)
				return ((int)p).ToStringInvariant();
			else if (p is bool)
				return (bool)p ? "1" : "0";
			else
				return p.ToString();
		}



		void FinishDisposableTimer(Func<string> commandText, TimeSpan duration)
		{
			lock (Sync)
			{
				var entry = Tuple.Create(duration, commandText);

				if (SlowestQueryDuration < duration)
					slowest = Tuple.Create(duration, commandText);
				allqueries.Add(entry);
				AllQueryDurations += duration;
			}
		}

		sealed class QueryTimer : IDisposable
		{
			readonly QueryTracer tracer;
			readonly Func<string> query;
			readonly Stopwatch queryTimer;

			internal QueryTimer(QueryTracer tracer, Func<string> query)
			{
				this.tracer = tracer;
				this.query = query;
				Interlocked.Increment(ref tracer.queryCount);
				queryTimer = Stopwatch.StartNew();
			}
			public void Dispose()
			{
				queryTimer.Stop();
				Interlocked.Increment(ref tracer.queriesCompleted);
				tracer.FinishDisposableTimer(query, queryTimer.Elapsed);
			}
		}
	}
}
