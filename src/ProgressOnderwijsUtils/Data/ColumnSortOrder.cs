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
		public ColumnSortOrder(IEnumerable<SortColumn> order)
		{
			HashSet<string> usedCols = new HashSet<string>();
			sortColumns = order.Where(col => usedCols.Add(col.Column)).ToArray();
		}
		ColumnSortOrder(SortColumn[] order) { sortColumns = order; }
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
				: new ColumnSortOrder(DirectSortColumns.Prepend(new SortColumn(kolomnaam, SortDirection.Desc)).ToArray());
		}

		static IEnumerable<SortColumn> PrependFiltered(SortColumn head, IEnumerable<SortColumn> tail) { return head.Concat(tail.Where(sc => sc.Column != head.Column)); }

		public ColumnSortOrder FirstSortBy(SortColumn firstby) { return firstby == null ? this : new ColumnSortOrder(PrependFiltered(firstby, DirectSortColumns).ToArray()); }
		public ColumnSortOrder ThenSortBy(SortColumn thenby)
		{
			return thenby == null || DirectSortColumns.Any(sc => sc.Column == thenby.Column) ? this
				: new ColumnSortOrder(DirectSortColumns.Concat(thenby).ToArray());
		}
		public ColumnSortOrder ThenSortBy(ColumnSortOrder thenby)
		{
			var mySet = new HashSet<string>(DirectSortColumns.Select(sc => sc.Column));
			return new ColumnSortOrder(DirectSortColumns.Concat(thenby.DirectSortColumns.Where(sc => !mySet.Contains(sc.Column))).ToArray());
		}

		public bool Equals(ColumnSortOrder other) { return DirectSortColumns.SequenceEqual(other.DirectSortColumns); }
		public static bool operator ==(ColumnSortOrder a, ColumnSortOrder b) { return a.Equals(b); }
		public static bool operator !=(ColumnSortOrder a, ColumnSortOrder b) { return !a.Equals(b); }
		public override bool Equals(object obj) { return obj is ColumnSortOrder && Equals((ColumnSortOrder)obj); }
		public override int GetHashCode() { return (int)(DirectSortColumns.Select((sc, i) => (2 * i + 1) * (long)sc.GetHashCode()).Aggregate(12345l, (a, b) => a + b)); }

		public override string ToString() { return "{" + DirectSortColumns.Select(col => col.ToString()).JoinStrings() + "}"; }
	}

	[TestFixture]
	public class SortOrderTest
	{
		readonly static SortColumn ziggyA = new SortColumn("ziggy", SortDirection.Asc);
		readonly static SortColumn ziggyD = new SortColumn("ziggy", SortDirection.Desc);
		readonly static SortColumn abcA = new SortColumn("abc", SortDirection.Asc);
		readonly static SortColumn abcD = new SortColumn("abc", SortDirection.Desc);
		readonly static SortColumn monsterA = new SortColumn("monster", SortDirection.Asc);
		readonly static SortColumn monsterD = new SortColumn("monster", SortDirection.Desc);
		readonly static SortColumn acolA = new SortColumn("acol", SortDirection.Asc);
		readonly static SortColumn acolD = new SortColumn("acol", SortDirection.Desc);
		readonly static SortColumn[] someOrder = new[] { ziggyA, abcA, acolD };
		readonly static ColumnSortOrder colSort = new ColumnSortOrder(someOrder);

		[Test]
		public void BasicOrderingOk()
		{
			//check that order works as exepcted:
			Assert.That(colSort.SortColumns, Is.EquivalentTo(someOrder));
		}

		[Test]
		public void IsImmutable()
		{
			foreach (var col in colSort.SortColumns)
				colSort.ToggleSortDirection(col.Column); //doesn't do anything!

			Assert.That(colSort.SortColumns, Is.EquivalentTo(new[] { ziggyA, abcA, acolD }));
		}

		[Test]
		public void NotEqualsOk()
		{
			//check that equality can fail too...
			Assert.That(colSort, Is.Not.EqualTo(new ColumnSortOrder(new[] { ziggyA, abcA, acolA })));
			Assert.That(colSort, Is.Not.EqualTo(new ColumnSortOrder(new[] { ziggyA, monsterA, acolD })));
		}
		[Test]
		public void DefaultIsEmpty()
		{
			//check that default order is the empty order:
			Assert.That(new ColumnSortOrder(new SortColumn[] { }), Is.EqualTo(default(ColumnSortOrder)));
		}

		[Test]
		public void ToggleOk()
		{
			//verify that toggling adds if not present:
			Assert.That(colSort.ToggleSortDirection("monster").SortColumns, Is.EquivalentTo(new[] { monsterD, ziggyA, abcA, acolD }));
			//verify that toggling moves to front and flips direction if present:
			Assert.That(colSort.ToggleSortDirection("acol").SortColumns, Is.EquivalentTo(new[] { acolA, ziggyA, abcA, }));
		}

		[Test]
		public void DuplicatesIgnored()
		{
			//verify that duplicate columns are ignored:
			Assert.That(new ColumnSortOrder(new[] { ziggyA, abcA, acolD, ziggyD, }), Is.EqualTo(colSort));
		}

		[Test]
		public void DoubleToggleNoOp()
		{
			//verify that toggling all columns twice in reverse order is a no-op:
			Assert.That(
				colSort.SortColumns.Concat(colSort.SortColumns).Reverse()
				.Aggregate(colSort, (sortorder, col) => sortorder.ToggleSortDirection(col.Column)),
				Is.EqualTo(colSort));
		}

		[Test]
		public void OperatorsOk()
		{
			Assert.That(colSort != new ColumnSortOrder(new[] { ziggyA, abcA, acolA }));
			Assert.That(colSort != new ColumnSortOrder(new[] { ziggyA, monsterA, acolD }));
			Assert.That(colSort == new ColumnSortOrder(new[] { ziggyA, abcA, acolD }));
			Assert.IsFalse(colSort == new ColumnSortOrder(new[] { ziggyA, abcA, acolA }));
			Assert.IsFalse(colSort == new ColumnSortOrder(new[] { ziggyA, monsterA, acolD }));
			Assert.IsFalse(colSort != new ColumnSortOrder(new[] { ziggyA, abcA, acolD }));
		}

		[Test]
		public void ThenByOk()
		{
			//check ThenBy for new column
			Assert.AreEqual(colSort, new ColumnSortOrder(new[] { ziggyA, abcA }).ThenSortBy(acolD));
			//check ThenBy for existing column
			Assert.AreEqual(colSort, colSort.ThenSortBy(acolA));
			Assert.AreEqual(colSort, colSort.ThenSortBy(ziggyD));
			//check ThenBy for null
			Assert.AreEqual(colSort, colSort.ThenSortBy(null));
		}

		[Test]
		public void ThenByColumnsOk()
		{
			Assert.AreEqual(new ColumnSortOrder(new[] { ziggyD, abcD }).ThenSortBy(colSort.FirstSortBy(monsterA)), new ColumnSortOrder(new[] { ziggyD, abcD, monsterA, acolD }));
		}

		[Test]
		public void FirstByOk()
		{
			//check firstby with toggle
			Assert.AreEqual(colSort.ToggleSortDirection("ziggy"), colSort.FirstSortBy(ziggyD));
		}

		[Test]
		public void ComplexFromScratch()
		{
			//check complex construction from scratch
			Assert.AreEqual(colSort, default(ColumnSortOrder).ToggleSortDirection("acol").FirstSortBy(abcA).ToggleSortDirection("ziggy").ToggleSortDirection("ziggy"));
		}

		[Test]
		public void GetHashcodeOk()
		{
			Assert.AreNotEqual(colSort.GetHashCode(), new ColumnSortOrder(new[] { ziggyA, abcA, acolA }).GetHashCode());
			Assert.AreNotEqual(colSort.GetHashCode(), new ColumnSortOrder(new[] { ziggyA, monsterA, acolD }).GetHashCode());
			Assert.AreEqual(colSort.GetHashCode(), new ColumnSortOrder(new[] { ziggyA, abcA, acolD }).GetHashCode());
			Assert.AreEqual(colSort.GetHashCode(), colSort.FirstSortBy(ziggyD).ToggleSortDirection("ziggy").GetHashCode());

			Assert.That(new ColumnSortOrder(new SortColumn[] { }).GetHashCode(), Is.EqualTo(default(ColumnSortOrder).GetHashCode()));
		}
	}
}
