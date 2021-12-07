using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ProgressOnderwijsUtils;

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
    readonly ColumnSort[]? sortColumns;

    [Pure]
    public ColumnSort[] Columns
        => sortColumns.EmptyIfNull();

    public static OrderByColumns Empty
        => new();

    public OrderByColumns(IEnumerable<ColumnSort> order)
        => sortColumns = DeduplicateByName(order);

    static ColumnSort[] DeduplicateByName(IEnumerable<ColumnSort> order)
    {
        var retval = new List<ColumnSort>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var col in order) {
            if (names.Add(col.ColumnName.AssertNotNull())) {
                retval.Add(col);
            }
        }
        return retval.ToArray();
    }

    OrderByColumns(ColumnSort[] order)
        => sortColumns = order;

    [Pure]
    public SortDirection? GetColumnSortDirection(string column)
    {
        foreach (var sc in Columns) {
            if (string.Equals(sc.ColumnName, column, StringComparison.OrdinalIgnoreCase)) {
                return sc.SortDirection;
            }
        }
        return null;
    }

    [Pure]
    public OrderByColumns ToggleSortDirection(string kolomnaam)
        => FirstSortBy(new ColumnSort(kolomnaam, GetColumnSortDirection(kolomnaam) ?? SortDirection.Asc).WithReverseDirection());

    [Pure]
    public OrderByColumns FirstSortBy(ColumnSort firstby)
        => new OrderByColumns(DeduplicateByName(new[] { firstby }.ConcatArray(Columns)));

    [Pure]
    public OrderByColumns ThenSortBy(ColumnSort thenby)
        => new OrderByColumns(Columns.Append(thenby));

    [Pure]
    public OrderByColumns ThenAsc(string column)
        => ThenSortBy(new ColumnSort(column, SortDirection.Asc));

    [Pure]
    public OrderByColumns ThenDesc(string column)
        => ThenSortBy(new ColumnSort(column, SortDirection.Desc));

    [Pure]
    public static OrderByColumns Asc(string column)
        => new OrderByColumns(new[] { new ColumnSort(column, SortDirection.Asc) });

    [Pure]
    public static OrderByColumns Desc(string column)
        => new OrderByColumns(new[] { new ColumnSort(column, SortDirection.Desc) });

    [Pure]
    public OrderByColumns ThenSortBy(OrderByColumns thenby)
        => new OrderByColumns(DeduplicateByName(Columns.ConcatArray(thenby.Columns)));

    [Pure]
    public bool Equals(OrderByColumns other)
        => Columns.SequenceEqual(other.Columns);

    public static bool operator ==(OrderByColumns a, OrderByColumns b)
        => a.Equals(b);

    public static bool operator !=(OrderByColumns a, OrderByColumns b)
        => !a.Equals(b);

    [Pure]
    public override bool Equals(object? obj)
        => obj is OrderByColumns orderByColumns && Equals(orderByColumns);

    [Pure]
    public override int GetHashCode()
        => 12345 + (int)Columns.Select((sc, i) => (2 * i + 1) * (long)sc.GetHashCode()).Sum();

    public override string ToString()
        => "{" + Columns.Select(col => col.ToString()).JoinStrings(", ") + "}";

    [Pure]
    public OrderByColumns AssumeThenBy(OrderByColumns BaseSortOrder)
    {
        var myCols = Columns;
        var assumedCols = BaseSortOrder.Columns;
        for (var matchLen = Math.Min(assumedCols.Length, myCols.Length); 0 < matchLen; matchLen--) {
            if (myCols.AsSpan(myCols.Length - matchLen, matchLen).SequenceEqual(assumedCols.AsSpan(0, matchLen))) {
                return new OrderByColumns(myCols.AsSpan(0, myCols.Length - matchLen).ToArray());
            }
        }
        return this;
    }
}