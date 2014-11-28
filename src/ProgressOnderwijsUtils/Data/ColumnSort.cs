using System;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public struct ColumnSort : IEquatable<ColumnSort>
    {
        readonly string column;
        readonly SortDirection direction;
        public string ColumnName { get { return column; } }
        public SortDirection SortDirection { get { return direction; } }
        public string SqlSortString() { return column + " " + direction; }
        public override string ToString() { return "[" + column + " " + direction + "]"; }

        public ColumnSort(string column, SortDirection direction)
        {
            this.column = column;
            this.direction = direction;
        }

        public ColumnSort WithReverseDirection() { return new ColumnSort(column, FlipDirection(direction)); }
        public ColumnSort WithDifferentName(string newColumn) { return new ColumnSort(newColumn, direction); }
        static SortDirection FlipDirection(SortDirection dir) { return dir == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc; }

        public bool Equals(ColumnSort other)
        {
            return string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) && SortDirection == other.SortDirection;
        }

        public override bool Equals(object obj) { return obj is ColumnSort && Equals((ColumnSort)obj); }
        public override int GetHashCode() { return StringComparer.OrdinalIgnoreCase.GetHashCode(column) + 51 * (int)direction; }
        public static bool operator ==(ColumnSort a, ColumnSort b) { return a.Equals(b); } //ReferenceEquals(a, b) || null != (object)a &&
        public static bool operator !=(ColumnSort a, ColumnSort b) { return !a.Equals(b); } //!ReferenceEquals(a, b) && (null == (object)a || 
    }

    [Continuous]
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
            Assert.IsFalse(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));

            Assert.That(new ColumnSort("test", SortDirection.Asc) == new ColumnSort("Test", SortDirection.Asc));
            Assert.IsFalse(new ColumnSort("test", SortDirection.Asc) != new ColumnSort("Test", SortDirection.Asc));

            Assert.That(new ColumnSort("test", SortDirection.Asc) != null);
            Assert.IsFalse(new ColumnSort("test", SortDirection.Asc) == null);
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
            col.WithReverseDirection().WithDifferentName("test");
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
