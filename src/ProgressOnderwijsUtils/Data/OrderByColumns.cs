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

		public override string ToString() { return "{" + DirectAcessColumns.Select(col => col.ToString()).JoinStrings() + "}"; }

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

	[TestFixture]
	public class OrderByColumnsTest
	{
		readonly static ColumnSort ziggyA = new ColumnSort("ziggy", SortDirection.Asc);
		readonly static ColumnSort ziggyD = new ColumnSort("ziggy", SortDirection.Desc);
		readonly static ColumnSort abcA = new ColumnSort("abc", SortDirection.Asc);
		readonly static ColumnSort abcD = new ColumnSort("abc", SortDirection.Desc);
		readonly static ColumnSort monsterA = new ColumnSort("monster", SortDirection.Asc);
		readonly static ColumnSort monsterD = new ColumnSort("monster", SortDirection.Desc);
		readonly static ColumnSort acolA = new ColumnSort("acol", SortDirection.Asc);
		readonly static ColumnSort acolD = new ColumnSort("acol", SortDirection.Desc);
		readonly static ColumnSort[] someOrder = new[] { ziggyA, abcA, acolD };
		readonly static OrderByColumns colSort = new OrderByColumns(someOrder);

		[Test]
		public void BasicOrderingOk()
		{
			//check that order works as exepcted:
			PAssert.That(() => colSort.Columns.SequenceEqual(someOrder));
		}

		[Test]
		public void IsImmutable()
		{
			foreach (var col in colSort.Columns)
				colSort.ToggleSortDirection(col.ColumnName); //doesn't do anything!

			PAssert.That(() => colSort.Columns.SequenceEqual(new[] { ziggyA, abcA, acolD }));
			PAssert.That(() => Equals(colSort, new OrderByColumns((new[] { ziggyA, abcA, acolD }))));
		}

		[Test]
		public void EqualsFailsOk()
		{
			//check that equality can fail too...
			PAssert.That(() => !Equals(colSort, new OrderByColumns(new[] { ziggyA, abcA, acolA })));
			PAssert.That(() => !Equals(colSort, new OrderByColumns(new[] { ziggyA, monsterA, acolD })));
		}

		[Test]
		public void DefaultIsEmpty()
		{
			//check that default order is the empty order:
			PAssert.That(() => new OrderByColumns(new ColumnSort[] { }) == default(OrderByColumns));
		}

		[Test]
		public void ToggleOk()
		{
			//verify that toggling adds if not present:
			PAssert.That(() => colSort.ToggleSortDirection("monster").Columns.SequenceEqual(new[] { monsterD, ziggyA, abcA, acolD }));
			//verify that toggling moves to front and flips direction if present:
			PAssert.That(() => colSort.ToggleSortDirection("acol").Columns.SequenceEqual(new[] { acolA, ziggyA, abcA, }));
		}

		[Test]
		public void DuplicatesIgnored()
		{
			//verify that duplicate columns are ignored:
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD, ziggyD, }) == colSort);
		}

		[Test]
		public void DoubleToggleNoOp()
		{
			//verify that toggling all columns twice in reverse order is a no-op:
			PAssert.That(() =>
				colSort.Columns.Concat(colSort.Columns).Reverse()
				.Aggregate(colSort, (sortorder, col) => sortorder.ToggleSortDirection(col.ColumnName))
				== colSort);
		}

		[Test]
		public void OperatorsOk()
		{
			PAssert.That(() => colSort != new OrderByColumns(new[] { ziggyA, abcA, acolA }));
			PAssert.That(() => colSort != new OrderByColumns(new[] { ziggyA, monsterA, acolD }));
			PAssert.That(() => colSort == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
			PAssert.That(() => !(colSort == new OrderByColumns(new[] { ziggyA, abcA, acolA })));
			PAssert.That(() => !(colSort == new OrderByColumns(new[] { ziggyA, monsterA, acolD })));
			PAssert.That(() => !(colSort != new OrderByColumns(new[] { ziggyA, abcA, acolD })));
		}

		[Test]
		public void ThenByOk()
		{
			//check ThenBy for new column
			PAssert.That(() => colSort == new OrderByColumns(new[] { ziggyA, abcA }).ThenSortBy(acolD));
			//check ThenBy for existing column
			PAssert.That(() => colSort == colSort.ThenSortBy(acolA));
			PAssert.That(() => colSort == colSort.ThenSortBy(ziggyD));
			//check ThenBy for null
			PAssert.That(() => colSort == colSort.ThenSortBy(null));
		}

		[Test]
		public void ThenByColumnsOk()
		{
			PAssert.That(() => new OrderByColumns(new[] { ziggyD, abcD }).ThenSortBy(colSort.FirstSortBy(monsterA)) == new OrderByColumns(new[] { ziggyD, abcD, monsterA, acolD }));
		}

		[Test]
		public void FirstByOk()
		{
			//check firstby with toggle
			PAssert.That(() => colSort.ToggleSortDirection("ziggy") == colSort.FirstSortBy(ziggyD));
		}

		[Test]
		public void ComplexFromScratch()
		{
			//check complex construction from scratch
			PAssert.That(() => colSort == default(OrderByColumns).ToggleSortDirection("acol").FirstSortBy(abcA).ToggleSortDirection("ziggy").ToggleSortDirection("ziggy"));
		}

		[Test]
		public void GetHashcodeOk()
		{
			PAssert.That(() => colSort.GetHashCode() != new OrderByColumns(new[] { ziggyA, abcA, acolA }).GetHashCode());
			PAssert.That(() => colSort.GetHashCode() != new OrderByColumns(new[] { ziggyA, monsterA, acolD }).GetHashCode());
			PAssert.That(() => colSort.GetHashCode() == new OrderByColumns(new[] { ziggyA, abcA, acolD }).GetHashCode());
			PAssert.That(() => colSort.GetHashCode() == colSort.FirstSortBy(ziggyD).ToggleSortDirection("ziggy").GetHashCode());

			PAssert.That(() => new OrderByColumns(new ColumnSort[] { }).GetHashCode() == default(OrderByColumns).GetHashCode());
		}

		[Test]
		public void AssumeThenByOk()
		{
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { acolD })) == new OrderByColumns(new[] { ziggyA, abcA, }));
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { abcA, acolD })) == new OrderByColumns(new[] { ziggyA, }));
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { ziggyA, abcA, acolD })) == OrderByColumns.Empty);
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { acolD, ziggyA, abcA, })) == new OrderByColumns(new[] { ziggyA, abcA, }));
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { abcA, ziggyA, acolD, })) == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
			PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { ziggyA, abcA, acolA })) == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
		}
	}
}
