using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        static readonly ColumnSort[] EmptyOrder = { };

        static bool streq(string a, string b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        readonly ColumnSort[] sortColumns;

        [System.Diagnostics.Contracts.Pure]
        public IEnumerable<ColumnSort> Columns
        {
            get {
                if (sortColumns != null) {
                    foreach (var sc in sortColumns) {
                        yield return sc;
                    }
                }
            }
        }

        ColumnSort[] DirectAcessColumns
            => sortColumns ?? EmptyOrder;

        public static OrderByColumns Empty
            => default(OrderByColumns);

        public OrderByColumns(IEnumerable<ColumnSort> order)
        {
            var columns = new ColumnSort[4];
            var idx = 0;
            var ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            foreach (var columnSort in order) {
                if (!HasDuplicateIn(columnSort, columns, idx, ordinalIgnoreCase)) {
                    if (columns.Length == idx) {
                        Array.Resize(ref columns, idx * 2);
                    }
                    columns[idx++] = columnSort;
                }
            }
            Array.Resize(ref columns, idx);
            sortColumns = columns;
        }

        OrderByColumns(ColumnSort[] order)
        {
            sortColumns = order;
        }

        [Pure]
        public ColumnSort? GetSortColumn(string column)
        {
            foreach (var sc in DirectAcessColumns) {
                if (streq(sc.ColumnName, column)) {
                    return sc;
                }
            }
            return null;
        }

        [Pure]
        public SortDirection? GetColumnSortDirection(string column)
        {
            var sc = GetSortColumn(column);
            return sc == null ? default(SortDirection?) : sc.Value.SortDirection;
        }

        [Pure]
        [CodeThatsOnlyUsedForTests]
        public int? GetColumnSortRank(string col)
        {
            var index = DirectAcessColumns.IndexOf(sc => sc.ColumnName.Equals(col, StringComparison.OrdinalIgnoreCase));
            return index == -1 ? default(int?) : index + 1;
        }

        [System.Diagnostics.Contracts.Pure]
        public int ColumnCount
            => sortColumns == null ? 0 : sortColumns.Length;

        [Pure]
        public OrderByColumns ToggleSortDirection(string kolomnaam)
        {
            var oldSortCol = GetSortColumn(kolomnaam);
            return oldSortCol != null
                ? FirstSortBy(oldSortCol.Value.WithReverseDirection())
                : new OrderByColumns(DirectAcessColumns.Prepend(new ColumnSort(kolomnaam, SortDirection.Desc)).ToArray());
        }

        static IEnumerable<ColumnSort> PrependFiltered(ColumnSort head, IEnumerable<ColumnSort> tail)
            => new[] { head }.Concat(tail.Where(sc => sc.ColumnName != head.ColumnName));

        [Pure]
        public OrderByColumns FirstSortBy(ColumnSort firstby)
            => new OrderByColumns(PrependFiltered(firstby, DirectAcessColumns).ToArray());

        [Pure]
        public OrderByColumns ThenSortBy(ColumnSort thenby)
            => DirectAcessColumns.Any(sc => streq(sc.ColumnName, thenby.ColumnName))
                ? this
                : new OrderByColumns(DirectAcessColumns.Append(thenby).ToArray());

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
        {
            var thenByColumns = thenby.DirectAcessColumns;
            var combinedOrder = DirectAcessColumns;
            var oldLength = combinedOrder.Length;
            Array.Resize(ref combinedOrder, oldLength + thenByColumns.Length);
            var idx = oldLength;
            var comparer = StringComparer.OrdinalIgnoreCase;
            foreach (var columnSort in thenByColumns) {
                if (!HasDuplicateIn(columnSort, combinedOrder, oldLength, comparer)) {
                    combinedOrder[idx++] = columnSort;
                }
            }
            Array.Resize(ref combinedOrder, idx);

            return new OrderByColumns(combinedOrder);
        }

        /// <summary>
        /// returns whether columnSort's name occurs in existing.Take(existingCount).
        /// </summary>
        static bool HasDuplicateIn(ColumnSort columnSort, ColumnSort[] existing, int existingCount, StringComparer comparer)
        {
            var dup = false;
            for (var j = 0; j < existingCount; j++) {
                if (comparer.Equals(existing[j].ColumnName, columnSort.ColumnName)) {
                    dup = true;
                    break;
                }
            }
            return dup;
        }

        [Pure]
        public bool Equals(OrderByColumns other)
            => DirectAcessColumns.SequenceEqual(other.DirectAcessColumns);

        public static bool operator ==(OrderByColumns a, OrderByColumns b)
            => a.Equals(b);

        public static bool operator !=(OrderByColumns a, OrderByColumns b)
            => !a.Equals(b);

        [Pure]
        public override bool Equals(object? obj)
            => obj is OrderByColumns orderByColumns && Equals(orderByColumns);

        [Pure]
        public override int GetHashCode()
            => (int)DirectAcessColumns.Select((sc, i) => (2 * i + 1) * (long)sc.GetHashCode()).Aggregate(12345L, (a, b) => a + b);

        public override string ToString()
            => "{" + DirectAcessColumns.Select(col => col.ToString()).JoinStrings(", ") + "}";

        [Pure]
        public OrderByColumns AssumeThenBy(OrderByColumns BaseSortOrder)
        {
            if (!BaseSortOrder.DirectAcessColumns.Any()) {
                return this;
            }
            var possibleMatchingTail = DirectAcessColumns.SkipWhile(colsort => colsort != BaseSortOrder.DirectAcessColumns.First());
            var baseTailOfSameLength = BaseSortOrder.DirectAcessColumns.Take(possibleMatchingTail.Count());
            if (possibleMatchingTail.SequenceEqual(baseTailOfSameLength)) { //equal!
                return new OrderByColumns(DirectAcessColumns.TakeWhile(colsort => colsort != BaseSortOrder.DirectAcessColumns.First()));
            } else {
                return this;
            }
        }
    }
}
