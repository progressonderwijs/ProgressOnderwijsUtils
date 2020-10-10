using Xunit;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class SortColumnTest
    {
        [Fact]
        public void CheckEquals()
        {
            Assert.NotEqual(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc));
            Assert.Equal(new ColumnSort("test", SortDirection.Asc), new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test"));
            Assert.Equal(new ColumnSort("test", SortDirection.Asc), new ColumnSort("Test", SortDirection.Asc));

            Assert.Equal(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Asc));

            Assert.NotEqual(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc));
            Assert.Equal(new ColumnSort("abc", SortDirection.Asc), new ColumnSort("abc", SortDirection.Desc).WithReverseDirection());

            Assert.False(new ColumnSort("abc", SortDirection.Asc).Equals(null));
        }

        [Fact]
        public void CheckSqlSortString()
        {
            Assert.Equal("ziggy Asc", new ColumnSort("ziggy", SortDirection.Asc).SqlSortString());
            Assert.Equal("test Asc", new ColumnSort("ziggy", SortDirection.Asc).WithDifferentName("test").SqlSortString());
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
