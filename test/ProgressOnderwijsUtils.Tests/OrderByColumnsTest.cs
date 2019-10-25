using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class OrderByColumnsTest
    {
        static readonly ColumnSort ziggyA = new ColumnSort("ziggy", SortDirection.Asc);
        static readonly ColumnSort ziggyD = new ColumnSort("ziggy", SortDirection.Desc);
        static readonly ColumnSort abcA = new ColumnSort("abc", SortDirection.Asc);
        static readonly ColumnSort abcD = new ColumnSort("abc", SortDirection.Desc);
        static readonly ColumnSort monsterA = new ColumnSort("monster", SortDirection.Asc);
        static readonly ColumnSort monsterD = new ColumnSort("monster", SortDirection.Desc);
        static readonly ColumnSort acolA = new ColumnSort("acol", SortDirection.Asc);
        static readonly ColumnSort acolD = new ColumnSort("acol", SortDirection.Desc);
        static readonly ColumnSort[] someOrder = new[] { ziggyA, abcA, acolD };
        static readonly OrderByColumns colSort = new OrderByColumns(someOrder);

        [Fact]
        public void BasicOrderingOk()
            //check that order works as exepcted:
            => PAssert.That(() => colSort.Columns.SequenceEqual(someOrder));

        [Fact]
        public void SortDirectionOk()
        {
            PAssert.That(() => colSort.GetColumnSortDirection("abc") == SortDirection.Asc);
            PAssert.That(() => colSort.GetColumnSortDirection("acol") == SortDirection.Desc);
            PAssert.That(() => colSort.GetColumnSortDirection("monster") == null);
        }

        [Fact]
        public void ColumnCountOk()
            => PAssert.That(() => colSort.Columns.Length == 3);

        [Fact]
        public void ToStringOk()
            => PAssert.That(() => colSort.ToString() == "{[ziggy Asc], [abc Asc], [acol Desc]}");

        [Fact]
        public void IsImmutable()
        {
            foreach (var col in colSort.Columns) {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                colSort.ToggleSortDirection(col.ColumnName); //we're testing if this really is pure.
            }

            PAssert.That(() => colSort.Columns.SequenceEqual(new[] { ziggyA, abcA, acolD }));
            PAssert.That(() => Equals(colSort, new OrderByColumns(new[] { ziggyA, abcA, acolD })));
        }

        [Fact]
        public void EqualsFailsOk()
        {
            //check that equality can fail too...
            PAssert.That(() => !Equals(colSort, new OrderByColumns(new[] { ziggyA, abcA, acolA })));
            PAssert.That(() => !Equals(colSort, new OrderByColumns(new[] { ziggyA, monsterA, acolD })));
        }

        [Fact]
        public void DefaultIsEmpty()
            //check that default order is the empty order:
            => PAssert.That(() => new OrderByColumns(new ColumnSort[] { }) == default(OrderByColumns));

        [Fact]
        public void ToggleOk()
        {
            //verify that toggling adds if not present:
            PAssert.That(() => colSort.ToggleSortDirection("monster").Columns.SequenceEqual(new[] { monsterD, ziggyA, abcA, acolD }));
            //verify that toggling moves to front and flips direction if present:
            PAssert.That(() => colSort.ToggleSortDirection("acol").Columns.SequenceEqual(new[] { acolA, ziggyA, abcA, }));
        }

        [Fact]
        public void DuplicatesIgnored()
            //verify that duplicate columns are ignored:
            => PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD, ziggyD, }) == colSort);

        [Fact]
        public void DoubleToggleNoOp()
            //verify that toggling all columns twice in reverse order is a no-op:
            => PAssert.That(
                () =>
                    colSort.Columns.Concat(colSort.Columns).Reverse()
                        .Aggregate(colSort, (sortorder, col) => sortorder.ToggleSortDirection(col.ColumnName))
                    == colSort);

        [Fact]
        public void OperatorsOk()
        {
            PAssert.That(() => colSort != new OrderByColumns(new[] { ziggyA, abcA, acolA }));
            PAssert.That(() => colSort != new OrderByColumns(new[] { ziggyA, monsterA, acolD }));
            PAssert.That(() => colSort == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
            PAssert.That(() => !(colSort == new OrderByColumns(new[] { ziggyA, abcA, acolA })));
            PAssert.That(() => !(colSort == new OrderByColumns(new[] { ziggyA, monsterA, acolD })));
            PAssert.That(() => !(colSort != new OrderByColumns(new[] { ziggyA, abcA, acolD })));
        }

        [Fact]
        public void ThenByOk()
        {
            //check ThenBy for new column
            PAssert.That(() => colSort == new OrderByColumns(new[] { ziggyA, abcA }).ThenSortBy(acolD));
            //check ThenBy for existing column
            PAssert.That(() => colSort == colSort.ThenSortBy(acolA));
            PAssert.That(() => colSort == colSort.ThenSortBy(ziggyD));
            //check ThenBy for null
        }

        [Fact]
        public void ThenByColumnsOk()
            => PAssert.That(
                () => new OrderByColumns(new[] { ziggyD, abcD }).ThenSortBy(colSort.FirstSortBy(monsterA)) == new OrderByColumns(new[] { ziggyD, abcD, monsterA, acolD }));

        [Fact]
        public void FirstByOk()
            //check firstby with toggle
            => PAssert.That(() => colSort.ToggleSortDirection("ziggy") == colSort.FirstSortBy(ziggyD));

        [Fact]
        public void ComplexFromScratch()
            //check complex construction from scratch
            => PAssert.That(() => colSort == default(OrderByColumns).ToggleSortDirection("acol").FirstSortBy(abcA).ToggleSortDirection("ziggy").ToggleSortDirection("ziggy"));

        [Fact]
        public void GetHashcodeOk()
        {
            PAssert.That(() => colSort.GetHashCode() != new OrderByColumns(new[] { ziggyA, abcA, acolA }).GetHashCode());
            PAssert.That(() => colSort.GetHashCode() != new OrderByColumns(new[] { ziggyA, monsterA, acolD }).GetHashCode());
            PAssert.That(() => colSort.GetHashCode() == new OrderByColumns(new[] { ziggyA, abcA, acolD }).GetHashCode());
            PAssert.That(() => colSort.GetHashCode() == colSort.FirstSortBy(ziggyD).ToggleSortDirection("ziggy").GetHashCode());

            PAssert.That(() => new OrderByColumns(new ColumnSort[] { }).GetHashCode() == default(OrderByColumns).GetHashCode());
        }

        [Fact]
        public void AssumeThenByOk()
        {
            PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(OrderByColumns.Empty) == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
            PAssert.That(
                () => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { acolD })) == new OrderByColumns(new[] { ziggyA, abcA, }));
            PAssert.That(
                () => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { abcA, acolD })) == new OrderByColumns(new[] { ziggyA, }));
            PAssert.That(() => new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { ziggyA, abcA, acolD })) == OrderByColumns.Empty);
            PAssert.That(
                () =>
                    new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { acolD, ziggyA, abcA, }))
                    == new OrderByColumns(new[] { ziggyA, abcA, }));
            PAssert.That(
                () =>
                    new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { abcA, ziggyA, acolD, }))
                    == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
            PAssert.That(
                () =>
                    new OrderByColumns(new[] { ziggyA, abcA, acolD }).AssumeThenBy(new OrderByColumns(new[] { ziggyA, abcA, acolA }))
                    == new OrderByColumns(new[] { ziggyA, abcA, acolD }));
        }
    }
}
