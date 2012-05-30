using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	public sealed class QueryBuilder : IEquatable<QueryBuilder>
	{
		readonly QueryBuilder prev;
		readonly IQueryComponent value;
		readonly QueryBuilder next;

		QueryBuilder(IQueryComponent singleNode)
		{
			Debug.Assert(singleNode != null);
			prev = null;
			value = singleNode;
		}

		QueryBuilder(QueryBuilder prefix, IQueryComponent singleComponent)
		{
			if (null == prefix) throw new ArgumentNullException("prefix");
			Debug.Assert(singleComponent != null);
			//if (null == then) throw new ArgumentNullException("then");
			prev = prefix.IsEmpty ? null : prefix;
			value = singleComponent;
		}

		QueryBuilder(QueryBuilder prefix, QueryBuilder continuation)
		{
			if (null == prefix) throw new ArgumentNullException("prefix");
			if (null == continuation) throw new ArgumentNullException("continuation");
			prev = prefix;
			next = continuation;
		}

		QueryBuilder() { }
		public static readonly QueryBuilder Empty = new QueryBuilder();

		public bool IsEmpty { get { return null == value && null == next; } }
		public bool IsSingleElement { get { return null == prev && null != value; } }

		//INVARIANT:
		// IF next != null THEN prev !=null; conversely IF prev == null THEN next == null 
		// !(value != null AND next !=null)

		public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) { return Concat(a, b); }
		public static QueryBuilder operator +(QueryBuilder a, string b) { return Concat(a, QueryComponent.CreateString(b)); }
		public static QueryBuilder operator +(string a, QueryBuilder b) { return Concat(Create(a), b); }
		public static explicit operator QueryBuilder(string a) { return Create(a); }

		static QueryBuilder Concat(QueryBuilder query, IQueryComponent part) { return null == part ? query : new QueryBuilder(query, part); }

		static QueryBuilder Concat(QueryBuilder first, QueryBuilder second)
		{
			if (null == first) throw new ArgumentNullException("first");
			else if (null == second) throw new ArgumentNullException("second");
			else if (first.IsEmpty) return second;
			else if (second.IsEmpty) return first;
			else if (second.IsSingleElement) return new QueryBuilder(first, second.value);
			else return new QueryBuilder(first, second);
		}

		public static QueryBuilder Create(string str) { return new QueryBuilder(QueryComponent.CreateString(str)); }
		public static QueryBuilder Param(object o) { return new QueryBuilder(QueryComponent.CreateParam(o)); }

		/// <summary>
		/// Adds a parameter to the query with a table-value.  Parameters must be an enumerable of meta-object type.
		/// 
		///   You need to define a corresponding type in the database.
		/// e.g:
		/// 
		/// CREATE TYPE [dbo].[IntValues] AS TABLE([val] [int] NOT NULL) would correspond to 
		/// public class MyClass : IMetaObject{ public int val {get;set;} }
		/// 
		/// see MSDN for more details.  For int-valued parameters, a predefined overload is provided.
		/// </summary>
		/// <param name="typeName">name of the db-type e.g. IntValues</param>
		/// <param name="o">the list of meta-objects with shape corresponding to the DB type</param>
		/// <returns>a composable query-component</returns>
		public static QueryBuilder TableParam<T>(string typeName, IEnumerable<T> o) where T : IMetaObject, new() { return new QueryBuilder(QueryComponent.ToTableParameter(typeName, o)); }
		public static QueryBuilder TableParamWithDeducedType(string typeName, IEnumerable<IMetaObject> o) { return new QueryBuilder(QueryComponent.ToTableParameterWithDeducedType(typeName, o)); }
		public static QueryBuilder TableParam(IEnumerable<int> o) { return new QueryBuilder(QueryComponent.ToTableParameter(o)); }

		public static QueryBuilder Create(string str, params object[] parms)
		{
			IQueryComponent[] parValues = parms.Select(QueryComponent.CreateParam).ToArray();

			QueryBuilder query = Empty;

			foreach (Match paramRefMatch in paramsRegex.Matches(str))
			{
				if (paramRefMatch.Groups["paramRef"].Success)
					query = Concat(query, parValues[int.Parse(paramRefMatch.Groups["paramRef"].Value)]);
				else
				{
					Debug.Assert(paramRefMatch.Groups["queryText"].Success);
					query = Concat(query, QueryComponent.CreateString(paramRefMatch.Groups["queryText"].Value));
				}
			}
			return query;
		}

		static readonly Regex paramsRegex = new Regex(@"\{(?<paramRef>\d+)\}|(?<queryText>((?!\{\d+\}).)+)", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		public static QueryBuilder CreateFromFilter(FilterBase filter) { return "and " + filter.ToQueryBuilder() + " "; }

		public static QueryBuilder CreateFromSortOrder(OrderByColumns mostSignificantColumnFirst)
		{
			if (mostSignificantColumnFirst.Columns.Any())
				return Create("order by " + mostSignificantColumnFirst.Columns.Select(sc => sc.SqlSortString).JoinStrings(", "));
			else
				return Empty;
		}

		public SqlCommand Finish(SqlConnection conn, int commandTimeout) { return QueryFactory.BuildQuery(ComponentsInReverseOrder.Reverse(), conn, commandTimeout); }

		public string DebugText()
		{
			return ComponentsInReverseOrder.Reverse().Select(component => component.ToDebugText()).JoinStrings();
		}
		public string CommandText() { return QueryFactory.BuildQueryText(ComponentsInReverseOrder.Reverse()); }

		IEnumerable<IQueryComponent> ComponentsInReverseOrder
		{
			get
			{
				if (IsEmpty) yield break;
				Stack<QueryBuilder> Continuation = new Stack<QueryBuilder>();
				QueryBuilder current = this;
				while (true)
				{
					if (current.prev != null)
						Continuation.Push(current.prev);

					if (current.next != null)
						current = current.next;
					else
					{
						yield return current.value;
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
		public override string ToString() { return DebugText(); }

		static readonly QueryBuilder TrueClause = Create("1=1");
		static readonly QueryBuilder WhereKeyword = Create(" where ");
		static readonly QueryBuilder AndKeyword = Create(" and ");

		public static QueryBuilder CreateWhereClause(IEnumerable<QueryBuilder> predicates) { return WhereKeyword + predicates.DefaultIfEmpty(TrueClause).Select((pred, i) => i == 0 ? pred : AndKeyword + pred).Aggregate(Concat); }
	}
}