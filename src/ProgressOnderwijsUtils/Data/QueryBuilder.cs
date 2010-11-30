using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	public class QueryBuilder
	{
		readonly IQueryComponent value;
		readonly QueryBuilder nestedNode;
		readonly QueryBuilder prev;
		QueryBuilder(IQueryComponent then)
		{
			if (then == null) throw new ArgumentNullException("then");
			prev = null;
			value = then;
		}
		QueryBuilder(QueryBuilder prefix, IQueryComponent then)
		{
			if (prefix == null) throw new ArgumentNullException("prefix");
			if (then == null) throw new ArgumentNullException("then");
			prev = prefix.IsEmpty ? null : prefix;
			value = then;
		}
		QueryBuilder(QueryBuilder prefix, QueryBuilder then)
		{
			if (prefix == null) throw new ArgumentNullException("prefix");
			if (then == null) throw new ArgumentNullException("then");
			prev = prefix;
			nestedNode = then;
		}
		QueryBuilder() { }
		public static readonly QueryBuilder Empty = new QueryBuilder();

		public bool IsEmpty { get { return value == null && nestedNode == null; } }
		public bool IsSingleElement { get { return prev == null && value != null; } }


		//INVARIANT:
		// IF nestedNode != null THEN prev !=null; conversely IF prev == null THEN nestedNode == null 
		// !(value != null AND nestedNode !=null)

		public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) { return Concat(a, b); }
		public static QueryBuilder operator +(QueryBuilder a, string b) { return Concat(a, QueryComponent.CreateString(b)); }
		public static QueryBuilder operator +(string a, QueryBuilder b) { return Concat(Create(a), b); }
		public static explicit operator QueryBuilder(string a) { return Create(a); }

		static QueryBuilder Concat(QueryBuilder query, IQueryComponent part)
		{
			return part == null ? query : new QueryBuilder(query, part);
		}

		static QueryBuilder Concat(QueryBuilder first, QueryBuilder second)
		{
			if (first.IsEmpty)
				return second;
			else if (second.IsEmpty)
				return first;
			else if (second.IsSingleElement)
				return new QueryBuilder(first, second.value);
			else return new QueryBuilder(first, second);
		}


		public static QueryBuilder Create(string str) { return new QueryBuilder(QueryComponent.CreateString(str)); }
		public static QueryBuilder Param(object o) { return new QueryBuilder(QueryComponent.CreateParam(o)); }
		public static QueryBuilder Create(string str, params object[] parms) { return Create(Empty, str, parms); }
		public static QueryBuilder Create(QueryBuilder prefix, string str, params object[] parms)
		{
			IQueryComponent[] parValues = parms.Select(QueryComponent.CreateParam).ToArray();

			QueryBuilder query = prefix;

			foreach (Match paramRefMatch in paramsRegex.Matches(str))
			{
				if (paramRefMatch.Groups["paramRef"].Success)
					query = Concat(query, parValues[int.Parse(paramRefMatch.Groups["paramRef"].Value)]);
				else if (paramRefMatch.Groups["queryText"].Success)
					query = Concat(query, QueryComponent.CreateString(paramRefMatch.Groups["queryText"].Value));
				else if (paramRefMatch.Success)
					throw new ProgressNetException("Impossible: regex must have a matching group if matching.");
			}
			return query;
		}

		static readonly Regex paramsRegex = new Regex(@"\{(?<paramRef>\d+)\}|(?<queryText>((?!\{\d+\}).)+)", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		public static QueryBuilder CreateFromFilter(Filter filter, Dictionary<string, string> computedcolumns)
		{
			return "and " + filter.ToSqlString(computedcolumns) + " ";
		}

		public static QueryBuilder CreateFromSortOrder(OrderByColumns mostSignificantColumnFirst)
		{
			if (mostSignificantColumnFirst.Columns.Any())
				return Create("order by " + mostSignificantColumnFirst.Columns.Select(sc => sc.SqlSortString).JoinStrings(", "));
			else
				return Empty;
		}

		public sealed class SerializedQuery : IEquatable<SerializedQuery>
		{
			public readonly string CommandText;
			public readonly SqlParameter[] Params;
			public SerializedQuery(string text, SqlParameter[] parms) { CommandText = text; Params = parms; }

			public bool Equals(SerializedQuery other) { return CommandText == other.CommandText && Params.Select(p => p.Value).SequenceEqual(other.Params.Select(p => p.Value)); }
			public override bool Equals(object obj) { return obj is SerializedQuery && Equals((SerializedQuery)obj); }

			public override int GetHashCode()
			{
				return 11 + CommandText.GetHashCode() - Params.Length +
					Params.Select((p, i) => p.Value.GetHashCode() * (i * 2 + 1)).Aggregate(0, (a, b) => a + b); //don't use Sum because sum does overflow checking.
			}
		}

		public SerializedQuery Serialize()
		{
			QueryParamNumberer qnum = new QueryParamNumberer();
			IQueryComponent[] components = ComponentsInReverseOrder.ToArray();
			Array.Reverse(components);
			var queryString = components.Select(component => component.ToSqlString(qnum)).JoinStrings();
			var sqlParams = qnum.SqlParamters;//any parameter that was used in ToSqlString is in the numberer
			return new SerializedQuery(queryString, sqlParams);
		}

		public string CommandText()
		{
			QueryParamNumberer qnum = new QueryParamNumberer();
			IQueryComponent[] components = ComponentsInReverseOrder.ToArray();
			Array.Reverse(components);
			return components.Select(component => component.ToSqlString(qnum)).JoinStrings();
		}

		public string DebugText()
		{
			IQueryComponent[] components = ComponentsInReverseOrder.ToArray();
			Array.Reverse(components);
			return components.Select(component => component.ToDebugText()).JoinStrings();
		}

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

					if (current.nestedNode != null)
						current = current.nestedNode;
					else
					{
						yield return current.value;
						if (Continuation.Count == 0) yield break;

						current = Continuation.Pop();
					}
				}
			}
		}

		public static bool operator ==(QueryBuilder a, QueryBuilder b)
		{
			return ReferenceEquals(a, b) ||
				!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.ComponentsInReverseOrder.SequenceEqual(b.ComponentsInReverseOrder);
		}

		public static bool operator !=(QueryBuilder a, QueryBuilder b) { return !(a == b); }

		public override bool Equals(object obj) { return obj is QueryBuilder && this == (QueryBuilder)obj; }
		public override int GetHashCode() { return HashCodeHelper.ComputeHash(ComponentsInReverseOrder.ToArray()) + 123; }
	}
}
