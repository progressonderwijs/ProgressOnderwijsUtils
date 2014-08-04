﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

		[Pure]
		public static QueryBuilder Empty { get { return EmptyComponent.Instance; } }

		bool IsEmpty { get { return this is EmptyComponent; } }
		bool IsSingleElement { get { return this is SingleComponent; } } //implies ValueOrNull != null


		[Pure]
		public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) { return Concat(a, b); }
		[Pure]
		public static QueryBuilder operator +(QueryBuilder a, string b) { return Concat(a, QueryComponent.CreateString(b)); }
		[Pure]
		public static QueryBuilder operator +(string a, QueryBuilder b) { return Concat(Create(a), b); }
		[Pure]
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

		[Pure]
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
		[Pure]
		public static QueryBuilder TableParam<T>(string typeName, IEnumerable<T> o) where T : IMetaObject, new() { return new SingleComponent(QueryComponent.ToTableParameter(typeName, o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<int> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<string> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<DateTime> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<TimeSpan> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<decimal> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<char> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<bool> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<byte> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<short> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<long> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParam(IEnumerable<double> o) { return new SingleComponent(QueryComponent.ToTableParameter(o)); }
		[Pure]
		public static QueryBuilder TableParamDynamic(Array o) { return new SingleComponent(QueryComponent.ToTableParameter((dynamic)o)); }
		// ReSharper restore UnusedMember.Global

		[Pure]
		public static QueryBuilder Create(string str, params object[] parms)
		{
			if(str ==null)
				throw new ArgumentNullException("str");

			IQueryComponent[] parValues = parms.Select(QueryComponent.CreateParam).ToArray();
			QueryBuilder query = Empty;

			int pos = 0;
			foreach (var paramRefMatch in ParamRefMatches(str))
			{
				query = Concat(query, QueryComponent.CreateString(str.Substring(pos, paramRefMatch.Index - pos)));
				query = Concat(query, parValues[Int32.Parse(str.Substring(paramRefMatch.Index + 1, paramRefMatch.Length - 2))]);
				pos = paramRefMatch.Index + paramRefMatch.Length;
			}
			query = Concat(query, QueryComponent.CreateString(str.Substring(pos, str.Length - pos)));

			return query;
		}
		public struct SubstringPosition
		{
			public int Index, Length;
		}


		static IEnumerable<SubstringPosition> ParamRefMatches(string query)
		{
			for (int pos = 0; pos < query.Length; pos++)
			{
				char c = query[pos];
				if (c == '{')
				{
					for (int pI = pos + 1; pI < query.Length; pI++)
					{
						if (query[pI] >= '0' && query[pI] <= '9')
							continue;
						else if (query[pI] == '}' && pI >= pos + 2) //{} testen
						{
							yield return new SubstringPosition { Index = pos, Length = pI - pos + 1 };
							pos = pI;
							break;
						}
						else
						{
							break;
						}
					}
				}
			}
		}


		private static readonly string[] AllColumns = new[] { "*" };

		[Pure]
		public static QueryBuilder CreateFromFilter(FilterBase filter) { return "and " + filter.ToQueryBuilder() + " "; }

		[Pure]
		public static QueryBuilder CreateFromSortOrder(OrderByColumns sortOrder)
		{
			return !sortOrder.Columns.Any() ? Empty :
				Create("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
		}

		[Pure]
		public SqlCommand CreateSqlCommand(SqlCommandCreationContext commandCreationContext)
		{
			var cmd = CommandFactory.BuildQuery(ComponentsInReverseOrder.Reverse(), commandCreationContext.Connection, commandCreationContext.CommandTimeout);
			if (commandCreationContext.Tracer != null)
			{
				try
				{
					var timer = commandCreationContext.Tracer.StartQueryTimer(cmd);
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

		[Pure]
		public string DebugText(Taal? taalOrNull)
		{
			return ComponentsInReverseOrder.Reverse().Select(component => component.ToDebugText(taalOrNull)).JoinStrings();
		}
		[Pure]
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

		[Pure]
		public override bool Equals(object obj) { return Equals(obj as QueryBuilder); }

		[Pure]
		public static bool operator ==(QueryBuilder a, QueryBuilder b) { return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b); }

		[Pure]
		public bool Equals(QueryBuilder other) { return !ReferenceEquals(other, null) && CanonicalReverseComponents.SequenceEqual(other.CanonicalReverseComponents); }

		[Pure]
		public static bool operator !=(QueryBuilder a, QueryBuilder b) { return !(a == b); }
		[Pure]
		public override int GetHashCode() { return HashCodeHelper.ComputeHash(CanonicalReverseComponents.ToArray()) + 123; }
		[Pure]
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

		[Pure]
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

		[Pure]
		public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<string> projectedColumns, IEnumerable<FilterBase> filterBases, OrderByColumns sortOrder)
		{
			return SubQueryHelper(subQuery, projectedColumns, filterBases, sortOrder, null);
		}

		[Pure]
		public void AssertNoVariableColumns()
		{
			var commandText = CommandText();
			var commandTextWithoutComments = Regex.Replace(commandText, @"/\*.*?\*/|--.*?$", "", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
			if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*"))
				throw new InvalidOperationException(GetType().FullName + ": Query may not use * as that might cause runtime exceptions in productie when DB changes:\n" + commandText);
		}
	}


	public class SqlCommandCreationContext : IDisposable
	{
		public SqlConnection Connection { get; private set; }
		public QueryTracer Tracer { get; private set; }
		public int CommandTimeout { get; private set; }
		public SqlCommandCreationContext OverrideTimeout(int timeoutSeconds)
		{
			return new SqlCommandCreationContext(Connection, timeoutSeconds, Tracer);
		}

		public SqlCommandCreationContext(SqlConnection conn, int defaultTimeout, QueryTracer tracer)
		{
			Connection = conn;
			CommandTimeout = defaultTimeout;
			Tracer = tracer;
		}

		public void Dispose()
		{
			Connection.Dispose();
		}
	}
}