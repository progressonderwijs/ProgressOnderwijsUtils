using System;
using JetBrains.Annotations;

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

        [NotNull]
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
        static SortDirection FlipDirection(SortDirection dir)
            => dir == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;

        [Pure]
        public bool Equals(ColumnSort other)
            =>
                string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) &&
                    SortDirection == other.SortDirection;

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
}
