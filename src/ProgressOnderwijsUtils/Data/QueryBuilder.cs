using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils.Data;
using ProgressOnderwijsUtils.Filters;

namespace ProgressOnderwijsUtils
{
	public abstract class QueryBuilder : IEquatable<QueryBuilder>
	{
		QueryBuilder() { } // only inner classes may inherit
		protected virtual QueryBuilder PrefixOrNull { get { return null; } }
		protected virtual QueryBuilder SuffixOrNull { get { return null; } }
		internal virtual IQueryComponent ValueOrNull { get { return null; } }

		sealed class EmptyComponent : QueryBuilder
		{
			EmptyComponent() { }
			public static readonly EmptyComponent Instance = new EmptyComponent();
		}

		sealed class SingleComponent : QueryBuilder
		{
			readonly IQueryComponent value;
			internal override IQueryComponent ValueOrNull { get { return value; } }

			public SingleComponent(IQueryComponent singleNode)
			{
				if (singleNode == null) throw new ArgumentNullException("singleNode");
				value = singleNode;
			}
		}

		sealed class PrefixAndComponent : QueryBuilder
		{
			readonly QueryBuilder precedingComponents;
			readonly IQueryComponent value;

			protected override QueryBuilder PrefixOrNull { get { return precedingComponents; } }
			internal override IQueryComponent ValueOrNull { get { return value; } }

			public PrefixAndComponent(QueryBuilder prefix, IQueryComponent singleComponent)
			{
				if (null == prefix) throw new ArgumentNullException("prefix");
				if (null == singleComponent) throw new ArgumentNullException("singleComponent");
				precedingComponents = prefix.IsEmpty ? null : prefix;
				value = singleComponent;
			}
		}

		sealed class PrefixAndSuffix : QueryBuilder
		{
			readonly QueryBuilder precedingComponents, next;
			protected override QueryBuilder PrefixOrNull { get { return precedingComponents; } }
			protected override QueryBuilder SuffixOrNull { get { return next; } }
			public PrefixAndSuffix(QueryBuilder prefix, QueryBuilder continuation)
			{
				if (null == prefix) throw new ArgumentNullException("prefix");
				if (null == continuation) throw new ArgumentNullException("continuation");
				precedingComponents = prefix;
				next = continuation;
			}
		}

		//INVARIANT:
		// IF next != null THEN precedingComponents !=null; conversely IF precedingComponents == null THEN next == null 
		// !(value != null AND next !=null)

		public static QueryBuilder Empty { get { return EmptyComponent.Instance; } }

		bool IsEmpty { get { return this is EmptyComponent; } }
		bool IsSingleElement { get { return this is SingleComponent; } } //implies ValueOrNull != null


		public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) { return Concat(a, b); }
		public static QueryBuilder operator +(QueryBuilder a, string b) { return Concat(a, QueryComponent.CreateString(b)); }
		public static QueryBuilder operator +(string a, QueryBuilder b) { return Concat(Create(a), b); }
		public static explicit operator QueryBuilder(string a) { return Create(a); }

		static QueryBuilder Concat(QueryBuilder query, IQueryComponent part) { return null == part ? query : new PrefixAndComponent(query, part); }

		static QueryBuilder Concat(QueryBuilder first, QueryBuilder second)
		{
			if (null == first) throw new ArgumentNullException("first");
			else if (null == second) throw new ArgumentNullException("second");
			else if (first.IsEmpty) return second;
			else if (second.IsEmpty) return first;
			else if (second.IsSingleElement) return new PrefixAndComponent(first, second.ValueOrNull);
			else return new PrefixAndSuffix(first, second);
		}

		public static QueryBuilder Param(object o) { return new SingleComponent(QueryComponent.CreateParam(o)); }

		/// <summary>
		/// Adds a parameter to the query with a table-value.  Parameters must be an enumerable of meta-object type.
		/// 
		///   You need to define a corresponding type in the database (see QueryComponent.ToTableParameter for details).
		/// </summary>
		/// <param name="typeName">name of the db-type e.g. IntValues</param>
		/// <param name="o">the list of meta-objects with shape corresponding to the DB type</param>
		/// <returns>a composable query-component</returns>
		// ReSharper disable UnusedMember.Global
		public static QueryBuilder TableParam<T>(string typeName, IEnumerable<T> o) where T : IMetaObject, new() { return new SingleComponent(QueryComponent.ToTableParameter(typeName, o)); }
		public static QueryBuilder TableParam(IEnumerable<int> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<string> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<DateTime> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<TimeSpan> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<decimal> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<char> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<bool> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<byte> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<short> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<long> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParam(IEnumerable<double> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		public static QueryBuilder TableParamDynamic(Array o) { return new SingleComponent(QueryComponent.ToTableParameter((dynamic)o)); }
		// ReSharper restore UnusedMember.Global

		public static QueryBuilder Create(string str, params object[] parms)
		{
			IQueryComponent[] parValues = parms.Select(QueryComponent.CreateParam).ToArray();

			QueryBuilder query = Empty;

			foreach (Match paramRefMatch in paramsRegex.Matches(str))
			{
				if (paramRefMatch.Groups["paramRef"].Success)
					query = Concat(query, parValues[Int32.Parse(paramRefMatch.Groups["paramRef"].Value)]);
				else
				{
					Debug.Assert(paramRefMatch.Groups["queryText"].Success);
					query = Concat(query, QueryComponent.CreateString(paramRefMatch.Groups["queryText"].Value));
				}
			}
			return query;
		}

		static readonly Regex paramsRegex = new Regex(@"\{(?<paramRef>\d+)\}|(?<queryText>((?!\{\d+\}).)+)", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		private static readonly string[] AllColumns = new[] { "*" };

		public static QueryBuilder CreateFromFilter(FilterBase filter) { return "and " + filter.ToQueryBuilder() + " "; }

		public static QueryBuilder CreateFromSortOrder(OrderByColumns sortOrder)
		{
			return !sortOrder.Columns.Any() ? Empty :
				Create("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
		}

		public struct ToSqlArgs
		{
			public SqlConnection Connection;
			public QueryTracer Tracer;
			public int CommandTimeout;
		}

		public SqlCommand CreateSqlCommand(ToSqlArgs args)
		{
			var cmd = CommandFactory.BuildQuery(ComponentsInReverseOrder.Reverse(), args.Connection, args.CommandTimeout);
			if (args.Tracer != null)
			{
				try
				{
					var timer = args.Tracer.StartQueryTimer(cmd);
					cmd.Disposed += (s, e) => timer.Dispose();
				}
				catch
				{
					cmd.Dispose();
					throw;
				}
			}
			return cmd;
		}

		public string DebugText(Taal? taalOrNull)
		{
			return ComponentsInReverseOrder.Reverse().Select(component => component.ToDebugText(taalOrNull)).JoinStrings();
		}
		public string CommandText() { return CommandFactory.BuildQueryText(ComponentsInReverseOrder.Reverse()); }

		IEnumerable<IQueryComponent> ComponentsInReverseOrder
		{
			get
			{
				if (IsEmpty) yield break;
				var Continuation = new Stack<QueryBuilder>();
				QueryBuilder current = this;
				while (true)
				{
					if (current.PrefixOrNull != null)
						Continuation.Push(current.PrefixOrNull); //deal with prefix if any later

					if (current.SuffixOrNull != null)
						current = current.SuffixOrNull; //can't have a value, so deal with suffix
					else //no suffix: either empty or with value.
					{
						if (current.ValueOrNull != null)
							yield return current.ValueOrNull;
						if (Continuation.Count == 0) yield break;

						current = Continuation.Pop();
					}
				}
			}
		}

		IEnumerable<IQueryComponent> CanonicalReverseComponents
		{
			get
			{
				var cached = new List<QueryStringComponent>();
				foreach (var comp in ComponentsInReverseOrder)
				{
					if (comp is QueryStringComponent)
						cached.Add((QueryStringComponent)comp);
					else
					{
						if (cached.Count > 0)
						{
							cached.Reverse();
							yield return QueryComponent.CreateString(cached.Select(c => c.val).JoinStrings());
							cached.Clear();
						}
						yield return comp;
					}
				}
				if (cached.Count > 0)
				{
					cached.Reverse();
					yield return QueryComponent.CreateString(cached.Select(c => c.val).JoinStrings());
				}
			}
		}

		public override bool Equals(object obj) { return Equals(obj as QueryBuilder); }

		public static bool operator ==(QueryBuilder a, QueryBuilder b) { return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b); }

		public bool Equals(QueryBuilder other) { return !ReferenceEquals(other, null) && CanonicalReverseComponents.SequenceEqual(other.CanonicalReverseComponents); }

		public static bool operator !=(QueryBuilder a, QueryBuilder b) { return !(a == b); }
		public override int GetHashCode() { return HashCodeHelper.ComputeHash(CanonicalReverseComponents.ToArray()) + 123; }
		public override string ToString() { return DebugText(null); }

		static QueryBuilder SubQueryHelper(QueryBuilder subquery, IEnumerable<string> projectedColumns, IEnumerable<FilterBase> filters, OrderByColumns sortOrder, QueryBuilder topRowsOrNull)
		{
			projectedColumns = projectedColumns ?? AllColumns;
			filters = filters.EmptyIfNull();

			QueryBuilder filterClause = Filter.CreateCombined(BooleanOperator.And, filters).ToQueryBuilder();
			return
				"select" + (topRowsOrNull != null ? " top (" + topRowsOrNull + ")" : Empty) + " " + projectedColumns.JoinStrings(", ") + " from (\n"
					+ subquery + "\n) as _g1 where  " + filterClause + "\n"
					+ CreateFromSortOrder(sortOrder);
		}

		public static QueryBuilder CreatePagedSubQuery(QueryBuilder subQuery, IEnumerable<string> projectedColumns, IEnumerable<FilterBase> filters, OrderByColumns sortOrder, int skipNrows, int takeNrows)
		{
			projectedColumns = projectedColumns ?? AllColumns;
			if (!projectedColumns.Any())
				throw new InvalidOperationException("Cannot create subquery without any projected columns: at least one column must be projected (are your columns all virtual?)\nQuery:\n" + subQuery.DebugText(null));
			filters = filters.EmptyIfNull();

			var takeRowsParam = Param((long)takeNrows);
			var skipNrowsParam = Param((long)skipNrows);

			var sortorder = sortOrder;
			var orderClause = sortorder == OrderByColumns.Empty ? (QueryBuilder)"order by (select 1)" : CreateFromSortOrder(sortorder);

			return "select top (" + takeRowsParam + ") " + projectedColumns.JoinStrings(", ") + "\n"
				+ "from (select _row=row_number() over (" + orderClause + "),\n"
				+ "      _g2.*\n"
				+ "from (\n\n"
				+ SubQueryHelper(subQuery, projectedColumns, filters, sortOrder, takeRowsParam + "+" + skipNrowsParam)
				+ "\n\n) as _g2) t\n"
				+ "where _row > " + skipNrowsParam + " \n"
				+ "order by _row";
		}

		public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<string> projectedColumns, IEnumerable<FilterBase> filterBases, OrderByColumns sortOrder)
		{
			return SubQueryHelper(subQuery, projectedColumns, filterBases, sortOrder, null);
		}

		public void AssertNoVariableColumns()
		{
			var commandText = CommandText();
			var commandTextWithoutComments = Regex.Replace(commandText, @"/\*.*?\*/|--.*?$", "", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
			if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*"))
				throw new InvalidOperationException(GetType().FullName + ": Query may not use * as that might cause runtime exceptions in productie when DB changes:\n" + commandText);
		}
	}
}