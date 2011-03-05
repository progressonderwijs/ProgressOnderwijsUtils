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

		readonly ColumnSort[] sortColumns;
		public IEnumerable<ColumnSort> Columns { get { if (sortColumns != null)foreach (var sc in sortColumns)yield return sc; } }
		IEnumerable<ColumnSort> DirectAcessColumns { get { return sortColumns ?? EmptyOrder; } }

		public static OrderByColumns Empty { get { return default(OrderByColumns); } }
		public OrderByColumns(IEnumerable<ColumnSort> order)
		{
			HashSet<string> usedCols = new HashSet<string>();
			sortColumns = order.Where(col => usedCols.Add(col.ColumnName)).ToArray();
		}

		OrderByColumns(ColumnSort[] order) { sortColumns = order; }
		public OrderByColumns(string column, SortDirection dir) { sortColumns = new[] { new ColumnSort(column, dir) }; }

		public ColumnSort GetSortColumn(string column) { return DirectAcessColumns.FirstOrDefault(sc => sc.ColumnName == column); }
		public SortDirection? GetColumnSortDirection(string column)
		{
			var sc = GetSortColumn(column);
			return sc == null ? default(SortDirection?) : sc.SortDirection;
		}
		public int? GetColumnSortRank(string col)
		{
			int index = DirectAcessColumns.Select(sc => sc.ColumnName).IndexOf(col);
			return index == -1 ? default(int?) : index + 1;
		}

		public int ColumnCount { get { return sortColumns == null ? 0 : sortColumns.Length; } }

		public OrderByColumns ToggleSortDirection(string kolomnaam)
		{
			var oldSortCol = GetSortColumn(kolomnaam);
			return oldSortCol != null
				? FirstSortBy(oldSortCol.WithReverseDirection)
				: new OrderByColumns(DirectAcessColumns.Prepend(new ColumnSort(kolomnaam, SortDirection.Desc)).ToArray());
		}

		static IEnumerable<ColumnSort> PrependFiltered(ColumnSort head, IEnumerable<ColumnSort> tail) { return head.Concat(tail.Where(sc => sc.ColumnName != head.ColumnName)); }

		public OrderByColumns FirstSortBy(ColumnSort firstby) { return firstby == null ? this : new OrderByColumns(PrependFiltered(firstby, DirectAcessColumns).ToArray()); }
		public OrderByColumns ThenSortBy(ColumnSort thenby)
		{
			return thenby == null || DirectAcessColumns.Any(sc => sc.ColumnName == thenby.ColumnName) ? this
				: new OrderByColumns(DirectAcessColumns.Concat(thenby).ToArray());
		}
		public OrderByColumns ThenSortBy(OrderByColumns thenby)
		{
			var mySet = new HashSet<string>(DirectAcessColumns.Select(sc => sc.ColumnName));
			return new OrderByColumns(DirectAcessColumns.Concat(thenby.DirectAcessColumns.Where(sc => !mySet.Contains(sc.ColumnName))).ToArray());
		}

		public bool Equals(OrderByColumns other) { return DirectAcessColumns.SequenceEqual(other.DirectAcessColumns); }
		public static bool operator ==(OrderByColumns a, OrderByColumns b) { return a.Equals(b); }
		public static bool operator !=(OrderByColumns a, OrderByColumns b) { return !a.Equals(b); }
		public override bool Equals(object obj) { return obj is OrderByColumns && Equals((OrderByColumns)obj); }
		public override int GetHashCode() { return (int)(DirectAcessColumns.Select((sc, i) => (2 * i + 1) * (long)sc.GetHashCode()).Aggregate(12345L, (a, b) => a + b)); }

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
