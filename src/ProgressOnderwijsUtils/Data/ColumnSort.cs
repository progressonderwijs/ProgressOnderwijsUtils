using System;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;

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
        [Test]
        public void CheckEquals()
        {
            Assert.AreNotEqual(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc));
            Assert.AreEqual(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));
            Assert.AreEqual(new ColumnSort("test", SortDirection.Asc), new ColumnSort("Test", SortDirection.Asc));

            Assert.AreNotSame(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Asc));
            Assert.AreEqual(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Asc));

            Assert.AreNotEqual(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc));
            Assert.AreEqual(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc).WithReverseDirection());

            Assert.AreNotEqual(new ColumnSort("abc", SortDirection.Asc), null);

            Assert.AreNotEqual(null, new ColumnSort("abc", SortDirection.Asc));
        }

        [Test]
        public void OperatorsOk()
        {
            Assert.That(new ColumnSort("test", SortDirection.Asc) == new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));
            PAssert.That(() => !(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test")));

            Assert.That(new ColumnSort("test", SortDirection.Asc) == new ColumnSort("Test", SortDirection.Asc));
            PAssert.That(() => !(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("Test", SortDirection.Asc)));
        }

        [Test]
        public void CheckSqlSortString()
        {
            Assert.AreEqual(new ColumnSort("ziggy", SortDirection.Asc).SqlSortString(), "ziggy Asc");
            Assert.AreEqual(new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test").SqlSortString(), "test Asc");
        }

        [Test]
        public void CheckImmutable()
        {
            var col = new ColumnSort("ziggy", SortDirection.Asc);
            var ignore = col.WithReverseDirection().WithDifferentName("test");
            Assert.AreEqual(new ColumnSort("ziggy", SortDirection.Asc), col);
        }

        [Test]
        public void CheckHashcode()
        {
            //hashcodes *may* collide, just these happen not to (and a collision is quite unlikely)

            Assert.AreNotEqual(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("ziggy", SortDirection.Asc).GetHashCode());
            Assert.AreEqual(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test").GetHashCode());
            Assert.AreEqual(new ColumnSort("test", SortDirection.Asc).GetHashCode(), new ColumnSort("Test", SortDirection.Asc).GetHashCode());

            Assert.AreEqual(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Asc).GetHashCode());

            Assert.AreNotEqual(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Desc).GetHashCode());
            Assert.AreEqual(new ColumnSort("abc", SortDirection.Asc).GetHashCode(), new ColumnSort("abc", SortDirection.Desc).WithReverseDirection().GetHashCode());
        }
    }
}
