namespace ProgressOnderwijsUtils.Collections;

public sealed class ProjectingEqualityComparer<T>
{
    public delegate bool EqualsComparer([DisallowNull] T x, [DisallowNull] T y);

    public delegate int HashComputation([DisallowNull] T obj);

    readonly EqualsComparer[] Equality;

    readonly HashComputation[] Hash;
    //obviously, iteratively appending to arrays isn't hyper efficient, but the presumption is that comparers will have "few" parts,
    //and conversely the comparer may get called often - so using a representation that friendly for the evaluator makes sense.

    ProjectingEqualityComparer(EqualsComparer[] equality, HashComputation[] hash)
        => (Equality, Hash) = (equality, hash);

    public ProjectingEqualityComparer() : this(Array.Empty<EqualsComparer>(), Array.Empty<HashComputation>()) { }

    public ProjectingEqualityComparer<T> AddKeyColumn<TPart>(Func<T, TPart> part, IEqualityComparer<TPart> partComparer)
        => AddKey_WithoutColumn(([DisallowNull] a, [DisallowNull] b) => partComparer.Equals(part(a), part(b)), ([DisallowNull] o) => part(o) is { } nonNull ? partComparer.GetHashCode(nonNull) : 0);

    public ProjectingEqualityComparer<T> AddKey_WithoutColumn(EqualsComparer compare, HashComputation hash)
        => new(Equality.Append(compare).ToArray(), Hash.Append(hash).ToArray());

    public ProjectingEqualityComparer<T> AddKeyColumn<TPart>(Func<T, TPart> part)
        where TPart : IEquatable<TPart>
        => AddKey_WithoutColumn(([DisallowNull] a, [DisallowNull] b) => part(a).Equals(part(b)), ([DisallowNull] o) => part(o).GetHashCode());

    public ProjectingEqualityComparer<T> AddKeyColumn_ViaObjectEquals<TPart>(Func<T, TPart> part)
        => AddKey_WithoutColumn(([DisallowNull] a, [DisallowNull] b) => Equals(part(a), part(b)), ([DisallowNull] o) => part(o)?.GetHashCode() ?? 0);

    public ProjectingEqualityComparer<T> AddKeyColumn<TPart>(Func<T, TPart?> part)
        where TPart : struct, IEquatable<TPart>
        => AddKey_WithoutColumn(
            ([DisallowNull] a, [DisallowNull] b) =>
                part(a) is var partAorNull
                && part(b) is var partBorNull
                && partAorNull is { } partA
                && partBorNull is { } partB
                    ? partA.Equals(partB)
                    : partAorNull is null && partBorNull is null,
            ([DisallowNull] o) => part(o) is { } nonNull ? nonNull.GetHashCode() : 37
        );

    public IEqualityComparer<T> Finish()
    {
        if (Equality.None()) {
            throw new InvalidOperationException("Don't generate equality comparers without parts");
        } else if (Equality.Length == 1) {
            return new Equatable(Equality[0], Hash[0]);
        }

        bool CombinedEquality([DisallowNull] T x, [DisallowNull] T y)
        {
            foreach (var part in Equality) {
                if (!part(x, y)) {
                    return false;
                }
            }
            return true;
        }
        int CombinedHash([DisallowNull] T obj)
        {
            var h = new HashCode();
            foreach (var part in Hash) {
                h.Add(part(obj));
            }
            return h.ToHashCode();
        }

        return new Equatable(CombinedEquality, CombinedHash);
    }

    sealed record Equatable(EqualsComparer EqualsComparer, HashComputation Hash) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
            => x is null ? y is null : y is not null && EqualsComparer(x, y);

        public int GetHashCode(T obj)
            => obj is null ? 0 : Hash(obj);
    }
}
