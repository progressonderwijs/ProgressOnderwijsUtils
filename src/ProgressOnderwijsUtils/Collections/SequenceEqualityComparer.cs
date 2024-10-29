namespace ProgressOnderwijsUtils.Collections;

public sealed record SequenceEqualityComparer<T>(IEqualityComparer<T> UnderlyingElementComparer, bool NullCountsAsEmpty) : IEqualityComparer<T[]?>, IEqualityComparer<IEnumerable<T>?>
{
    public static readonly SequenceEqualityComparer<T> Default = new(EqualityComparer<T>.Default, false);
    const int NullHashCode = 0x1d45_7af3;

    [Pure]
    public bool Equals(T[]? x, T[]? y)
    {
        if (NullCountsAsEmpty) {
            x ??= [];
            y ??= [];
        } else {
            if (x == null) {
                return y == null;
            } else if (y == null) {
                return false;
            }
        }
        if (x.Length != y.Length) {
            return false;
        }

        for (var i = 0; i < x.Length; i++) {
            if (!UnderlyingElementComparer.Equals(x[i], y[i])) {
                return false;
            }
        }
        return true;
    }

    [Pure]
    public int GetHashCode(T[]? arr)
    {
        if (arr == null) {
            if (NullCountsAsEmpty) {
                arr = [];
            } else {
                return NullHashCode;
            }
        }
        var buffer = new HashCode();
        foreach (var obj in arr) {
            buffer.Add(obj, UnderlyingElementComparer);
        }
        return buffer.ToHashCode();
    }

    [Pure]
    public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
    {
        if (NullCountsAsEmpty) {
            x ??= Array.Empty<T>();
            y ??= Array.Empty<T>();
        } else {
            if (x == null) {
                return y == null;
            } else if (y == null) {
                return false;
            }
        }

        using var xs = x.GetEnumerator();
        using var ys = y.GetEnumerator();

        while (true) {
            var hasX = xs.MoveNext();
            var hasY = ys.MoveNext();
            if (hasX && hasY) {
                if (!UnderlyingElementComparer.Equals(xs.Current, ys.Current)) {
                    return false;
                }
            } else {
                return !hasX && !hasY;
            }
        }
    }

    [Pure]
    public int GetHashCode(IEnumerable<T>? seq)
    {
        if (seq == null) {
            if (NullCountsAsEmpty) {
                seq = Array.Empty<T>();
            } else {
                return NullHashCode;
            }
        }
        var buffer = new HashCode();
        foreach (var obj in seq) {
            buffer.Add(obj, UnderlyingElementComparer);
        }
        return buffer.ToHashCode();
    }
}
