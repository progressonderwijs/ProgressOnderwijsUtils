using System;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils
{
    public enum SortDirection
    {
        Asc,
        Desc
    }

    [Serializable]
    public struct ColumnSort : IEquatable<ColumnSort>
    {
        readonly string column;
        readonly SortDirection direction;
        public string ColumnName => column;
        public SortDirection SortDirection => direction;
        public string SqlSortString() => column + " " + direction;
        public override string ToString() => "[" + column + " " + direction + "]";

        public ColumnSort(string column, SortDirection direction)
        {
            this.column = column;
            this.direction = direction;
        }

        [Pure]
        public ColumnSort WithReverseDirection() => new ColumnSort(column, FlipDirection(direction));

        [Pure]
        public ColumnSort WithDifferentName(string newColumn) => new ColumnSort(newColumn, direction);

        [Pure]
        static SortDirection FlipDirection(SortDirection dir) => dir == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;

        [Pure]
        public bool Equals(ColumnSort other) => string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) && SortDirection == other.SortDirection;

        [Pure]
        public override bool Equals(object obj) => obj is ColumnSort && Equals((ColumnSort)obj);

        [Pure]
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(column) + 51 * (int)direction;

        [Pure]
        public static bool operator ==(ColumnSort a, ColumnSort b)
        {
            return a.Equals(b);
        } //ReferenceEquals(a, b) || null != (object)a &&

        [Pure]
        public static bool operator !=(ColumnSort a, ColumnSort b)
        {
            return !a.Equals(b);
        } //!ReferenceEquals(a, b) && (null == (object)a || 
    }

    public class SortColumnTest
    {
        [Fact]
        public void CheckEquals()
        {
            Assert.NotEqual(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc));
            Assert.Equal(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));
            Assert.Equal(new ColumnSort("test", SortDirection.Asc), new ColumnSort("Test", SortDirection.Asc));

            Assert.NotSame(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Asc));
            Assert.Equal(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Asc));

            Assert.NotEqual(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc));
            Assert.Equal(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc).WithReverseDirection());

            Assert.NotEqual((object)new ColumnSort("abc", SortDirection.Asc), null);

            Assert.NotEqual(null, (object)new ColumnSort("abc", SortDirection.Asc));
        }

        [Fact]
        public void OperatorsOk()
        {
            PAssert.That(()=>new ColumnSort("test", SortDirection.Asc) == new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));
            PAssert.That(() => !(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test")));

            PAssert.That(() => new ColumnSort("test", SortDirection.Asc) == new ColumnSort("Test", SortDirection.Asc));
            PAssert.That(() => !(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("Test", SortDirection.Asc)));
        }

        [Fact]
        public void CheckSqlSortString()
        {
            Assert.Equal(new ColumnSort("ziggy", SortDirection.Asc).SqlSortString(), "ziggy Asc");
            Assert.Equal(new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test").SqlSortString(), "test Asc");
        }

        [Fact]
        public void CheckImmutable()
        {
            var col = new ColumnSort("ziggy", SortDirection.Asc);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            col.WithReverseDirection().WithDifferentName("test"); //to test whether it's really pure.
            Assert.Equal(new ColumnSort("ziggy", SortDirection.Asc), col);
        }

        [Fact]
        public void CheckHashcode()
        {
            //hashcodes *may* collide, just these happen not to (and a collision is quite unlikely)

            Assert.NotEqual(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("ziggy", SortDirection.Asc).GetHashCode());
            Assert.Equal(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test").GetHashCode());
            Assert.Equal(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("Test", SortDirection.Asc).GetHashCode());

            Assert.Equal(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Asc).GetHashCode());

            Assert.NotEqual(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Desc).GetHashCode());
            Assert.Equal(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Desc).WithReverseDirection().GetHashCode());
        }
    }
}
