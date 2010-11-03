using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

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
	public struct ColumnSortOrder : IEquatable<ColumnSortOrder>
	{
		readonly static SortColumn[] EmptyOrder = new SortColumn[] { };

		readonly SortColumn[] sortColumns;
		public IEnumerable<SortColumn> SortColumns { get { if (sortColumns != null)foreach (var sc in sortColumns)yield return sc; } }
		IEnumerable<SortColumn> DirectSortColumns { get { return sortColumns ?? EmptyOrder; } }

		public static ColumnSortOrder Empty { get { return default(ColumnSortOrder); } }
		public ColumnSortOrder(IEnumerable<SortColumn> order) { sortColumns = order.ToArray(); }
		public ColumnSortOrder(string column, SortDirection dir) { sortColumns = new[] { new SortColumn(column, dir) }; }

		public SortColumn GetSortColumn(string column) { return DirectSortColumns.FirstOrDefault(sc => sc.Column == column); }
		public SortDirection? GetColumnSortDirection(string column)
		{
			var sc = GetSortColumn(column);
			return sc == null ? default(SortDirection?) : sc.Direction;
		}
		public int? GetColumnSortRank(string col)
		{
			int index = DirectSortColumns.Select(sc => sc.Column).IndexOf(col);
			return index == -1 ? default(int?) : index + 1;
		}

		public int ColumnCount { get { return sortColumns == null ? 0 : sortColumns.Length; } }

		public ColumnSortOrder ToggleSortDirection(string kolomnaam)
		{
			var oldSortCol = GetSortColumn(kolomnaam);
			return oldSortCol != null
				? FirstSortBy(oldSortCol.WithReverseDirection)
				: new ColumnSortOrder(DirectSortColumns.Prepend(new SortColumn(kolomnaam, SortDirection.Desc)));
		}

		static IEnumerable<SortColumn> PrependFiltered(SortColumn head, IEnumerable<SortColumn> tail) { return head.Concat(tail.Where(sc => sc.Column != head.Column)); }

		public ColumnSortOrder FirstSortBy(SortColumn firstby) { return firstby == null ? this : new ColumnSortOrder(PrependFiltered(firstby, DirectSortColumns)); }
		public ColumnSortOrder ThenSortBy(SortColumn thenby) { return thenby == null || DirectSortColumns.Any(sc => sc.Column == thenby.Column) ? this : new ColumnSortOrder(DirectSortColumns.Concat(thenby)); }
		public ColumnSortOrder ThenSortBy(ColumnSortOrder thenby)
		{
			var mySet = new HashSet<string>(DirectSortColumns.Select(sc => sc.Column));
			return new ColumnSortOrder(DirectSortColumns.Concat(thenby.DirectSortColumns.Where(sc => !mySet.Contains(sc.Column))));
		}

		public bool Equals(ColumnSortOrder other) { return DirectSortColumns.SequenceEqual(other.DirectSortColumns); }
		public static bool operator ==(ColumnSortOrder a, ColumnSortOrder b) { return a.Equals(b); }
		public static bool operator !=(ColumnSortOrder a, ColumnSortOrder b) { return !a.Equals(b); }
		public override bool Equals(object obj) { return obj is ColumnSortOrder && Equals((ColumnSortOrder)obj); }
		public override int GetHashCode() { return 12345 + DirectSortColumns.Select((sc, i) => (2 * i + 1) * sc.GetHashCode()).Sum(); }
	}

	[TestFixture]
	public class SortOrderTest
	{


		[Test]
		public void CheckEquals()
		{
			var ziggyA = new SortColumn("ziggy", SortDirection.Asc);
			var ziggyD = new SortColumn("ziggy", SortDirection.Desc);
			var abcA = new SortColumn("abc", SortDirection.Asc);
			var abcD = new SortColumn("abc", SortDirection.Desc);
			var monsterA = new SortColumn("monster", SortDirection.Asc);
			var monsterD = new SortColumn("monster", SortDirection.Desc);
			var acolA = new SortColumn("acol", SortDirection.Asc);
			var acolD = new SortColumn("acol", SortDirection.Desc);

			Assert.That(new ColumnSortOrder(new[] { ziggyA, abcA, acolD }).SortColumns, Is.EquivalentTo(new[] { ziggyA, abcA, acolD }));
		}

	}
}
