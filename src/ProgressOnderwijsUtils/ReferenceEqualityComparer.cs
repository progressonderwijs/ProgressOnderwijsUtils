namespace ProgressOnderwijsUtils;

/// <summary>
/// Equality comparer that will compare on reference equality.
/// </summary>
/// <remarks>This might be handy to have collections on reference equality while the elements are value comparable.</remarks>
public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
{
    public bool Equals([AllowNull] T one, [AllowNull] T other)
        => ReferenceEquals(one, other);

    public int GetHashCode([AllowNull] T obj)
        => RuntimeHelpers.GetHashCode(obj! /*not really non-nullable; parameter is incorrectly labelled as not null*/);

    bool IEqualityComparer.Equals(object? x, object? y)
        => ReferenceEquals(x, y);

    int IEqualityComparer.GetHashCode(object obj)
        => RuntimeHelpers.GetHashCode(obj);
}