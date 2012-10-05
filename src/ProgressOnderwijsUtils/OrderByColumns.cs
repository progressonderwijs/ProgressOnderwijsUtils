using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Representeerd de volgorde van sorteren op kolommen.  
	/// 
	/// Deze datastructuur is READONLY!  Als je functies als "Append" "ToggleSortDirection" uitvoerd wordt 
	/// een NIEUWE ColumnSortOrder gereturned. 
	/// 
	/// ColumnSortOrder is een struct; zijn default waarde representeerd "geen sorteering".
	/// </summary>
	[Serializable]
	public struct OrderByColumns : IEquatable<OrderByColumns>
	{
		readonly static ColumnSort[] EmptyOrder = new ColumnSort[] { };
		static bool streq(string a, string b) { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
		readonly ColumnSort[] sortColumns;
		public IEnumerable<ColumnSort> Columns { get { if (sortColumns != null)foreach (var sc in sortColumns) yield return sc; } }
		IEnumerable<ColumnSort> DirectAcessColumns { get { return sortColumns ?? EmptyOrder; } }

		public static OrderByColumns Empty { get { return default(OrderByColumns); } }
		public OrderByColumns(IEnumerable<ColumnSort> order)
		{
			var usedCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			sortColumns = order.Where(col => usedCols.Add(col.ColumnName)).ToArray();
		}

		OrderByColumns(ColumnSort[] order) { sortColumns = order; }

		public ColumnSort GetSortColumn(string column) { return DirectAcessColumns.FirstOrDefault(sc => streq(sc.ColumnName, column)); }
		public SortDirection? GetColumnSortDirection(string column)
		{
			var sc = GetSortColumn(column);
			return sc == null ? default(SortDirection?) : sc.SortDirection;
		}
		public int? GetColumnSortRank(string col)
		{
			int index = DirectAcessColumns.IndexOf(sc => sc.ColumnName.Equals(col, StringComparison.OrdinalIgnoreCase));
			return index == -1 ? default(int?) : index + 1;
		}

		public int ColumnCount { get { return sortColumns == null ? 0 : sortColumns.Length; } }

		public OrderByColumns ToggleSortDirection(string kolomnaam)
		{
			var oldSortCol = GetSortColumn(kolomnaam);
			return oldSortCol != null
				? FirstSortBy(oldSortCol.WithReverseDirection())
				: new OrderByColumns(DirectAcessColumns.Prepend(new ColumnSort(kolomnaam, SortDirection.Desc)).ToArray());
		}

		static IEnumerable<ColumnSort> PrependFiltered(ColumnSort head, IEnumerable<ColumnSort> tail) { return new[] { head }.Concat(tail.Where(sc => sc.ColumnName != head.ColumnName)); }

		public OrderByColumns FirstSortBy(ColumnSort firstby) { return firstby == null ? this : new OrderByColumns(PrependFiltered(firstby, DirectAcessColumns).ToArray()); }
		public OrderByColumns ThenSortBy(ColumnSort thenby)
		{
			return thenby == null || DirectAcessColumns.Any(sc => streq(sc.ColumnName, thenby.ColumnName)) ? this
				: new OrderByColumns(DirectAcessColumns.Concat(thenby).ToArray());
		}
		public OrderByColumns ThenAsc(string column) { return ThenSortBy(new ColumnSort(column, SortDirection.Asc)); }
		public OrderByColumns ThenDesc(string column) { return ThenSortBy(new ColumnSort(column, SortDirection.Desc)); }
		public static OrderByColumns Asc(string column) { return new OrderByColumns(new[] { new ColumnSort(column, SortDirection.Asc) }); }
		public static OrderByColumns Desc(string column) { return new OrderByColumns(new[] { new ColumnSort(column, SortDirection.Desc) }); }

		public OrderByColumns ThenSortBy(OrderByColumns thenby)
		{
			var mySet = new HashSet<string>(DirectAcessColumns.Select(sc => sc.ColumnName), StringComparer.OrdinalIgnoreCase);
			return new OrderByColumns(DirectAcessColumns.Concat(thenby.DirectAcessColumns.Where(sc => !mySet.Contains(sc.ColumnName))).ToArray());
		}

		public bool Equals(OrderByColumns other) { return DirectAcessColumns.SequenceEqual(other.DirectAcessColumns); }
		public static bool operator ==(OrderByColumns a, OrderByColumns b) { return a.Equals(b); }
		public static bool operator !=(OrderByColumns a, OrderByColumns b) { return !a.Equals(b); }
		public override bool Equals(object obj) { return obj is OrderByColumns && Equals((OrderByColumns)obj); }
		public override int GetHashCode()
		{
			return (int)(DirectAcessColumns.Select((sc, i) => (2 * i + 1) * (long)sc.GetHashCode()).Aggregate(12345L, (a, b) => a + b));
		}

		public override string ToString() { return "{" + DirectAcessColumns.Select(col => col.ToString()).JoinStrings(", ") + "}"; }

		public OrderByColumns AssumeThenBy(OrderByColumns BaseSortOrder)
		{
			if (!BaseSortOrder.DirectAcessColumns.Any()) return this;
			var possibleMatchingTail = DirectAcessColumns.SkipWhile(colsort => colsort != BaseSortOrder.DirectAcessColumns.First());
			var baseTailOfSameLength = BaseSortOrder.DirectAcessColumns.Take(possibleMatchingTail.Count());
			if (possibleMatchingTail.SequenceEqual(baseTailOfSameLength)) //equal!
				return new OrderByColumns(DirectAcessColumns.TakeWhile(colsort => colsort != BaseSortOrder.DirectAcessColumns.First()));
			else
				return this;
		}
	}
}
