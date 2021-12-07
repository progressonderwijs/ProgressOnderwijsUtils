namespace ProgressOnderwijsUtils;

public enum SortDirection { Asc, Desc, }

[Serializable]
public readonly struct ColumnSort : IEquatable<ColumnSort>
{
    public string? ColumnName { get; } //intrinsically nullable due to binary serialization
    public SortDirection SortDirection { get; }

    public string SqlSortString()
        => ColumnName + " " + SortDirection;

    public override string ToString()
        => "[" + ColumnName + " " + SortDirection + "]";

    public ColumnSort(string column, SortDirection direction)
    {
        ColumnName = column;
        SortDirection = direction;
    }

    [Pure]
    public ColumnSort WithReverseDirection()
        => new(ColumnName.AssertNotNull(), SortDirection == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc);

    [Pure]
    public ColumnSort WithDifferentName(string newColumn)
        => new(newColumn, SortDirection);

    [Pure]
    public bool Equals(ColumnSort other)
        => string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) &&
            SortDirection == other.SortDirection;

    [Pure]
    public override bool Equals(object? obj)
        => obj is ColumnSort columnSort && Equals(columnSort);

    [Pure]
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(ColumnName ?? "") + 51 * (int)SortDirection;
}
