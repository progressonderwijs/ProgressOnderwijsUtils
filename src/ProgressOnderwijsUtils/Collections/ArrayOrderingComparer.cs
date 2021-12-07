using System.Collections.Generic;

namespace ProgressOnderwijsUtils.Collections;

public sealed class ArrayOrderingComparer<T> : IComparer<T[]?>
{
    public static readonly ArrayOrderingComparer<T> Default = new(Comparer<T>.Default);
    readonly IComparer<T> underlying;

    public ArrayOrderingComparer(IComparer<T> underlying)
        => this.underlying = underlying;

    public int Compare(T[]? x, T[]? y)
    {
        if (x == y) {
            return 0;
        } else if (x == null) {
            return -1; //y nonnull
        } else if (y == null) {
            return 1; //x nonnull
        }
        var i = 0;
        while (i < x.Length && i < y.Length) {
            var cmp = underlying.Compare(x[i], y[i]);
            if (cmp != 0) {
                return cmp;
            }
            i++;
        }
        return x.Length.CompareTo(y.Length);
    }
}