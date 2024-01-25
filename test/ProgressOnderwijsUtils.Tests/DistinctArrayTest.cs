namespace ProgressOnderwijsUtils.Tests;

public sealed class DistinctArrayTest
{
    [Fact]
    public void Initialize_with_empty_array_gives_count_0()
    {
        var sut = DistinctArray<int>.Empty;

        PAssert.That(() => sut.Count == 0);
    }

    [Fact]
    public void Created_DistinctArray_from_distinct_is_same()
    {
        var distinct = new[] { 1, 2, };
        var sut = distinct.ToDistinctArrayFromDistinct();

        PAssert.That(() => sut.SetEqual(distinct));
    }

    [Fact]
    public void Created_DistinctArray_from_distinct_with_custom_comparer_is_same()
    {
        var distinct = new[] { 1, 2, };
        var sut = distinct.ToDistinctArrayFromDistinct(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode()));

        PAssert.That(() => sut.SetEqual(distinct));
    }

    [Fact]
    public void Creating_DistinctArray_from_not_distinct_gives_error()
        => Assert.ThrowsAny<ArgumentException>(
            () => {
                var unused = new[] { 1, 1, 2, }.ToDistinctArrayFromDistinct();
            }
        );

    [Fact]
    public void Creating_DistinctArray_from_not_disBtinct_with_custom_comparer_gives_error()
        => Assert.ThrowsAny<ArgumentException>(
            () => {
                var unused = new[] { 1, 1, 2, }.ToDistinctArrayFromDistinct(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode()));
            }
        );

    [Fact]
    public void DefaultIsEmpty()
    {
        var defaultValue = default(DistinctArray<string>);
        PAssert.That(() => defaultValue.None());
        // ReSharper disable NotDisposedResource
        PAssert.That(() => !((IEnumerable)defaultValue).GetEnumerator().MoveNext());
        PAssert.That(() => !((IEnumerable<string>)defaultValue).GetEnumerator().MoveNext());
        PAssert.That(() => !defaultValue.GetEnumerator().MoveNext());
        // ReSharper restore NotDisposedResource
        PAssert.That(() => defaultValue.UnderlyingArrayThatShouldNeverBeMutated().None());
        PAssert.That(() => defaultValue.Count == 0);
    }

    [Fact]
    public void UncheckedDistinctArrayReturnsSameInstance()
    {
        var ints = new[] { 1, 2, };
        var sut = ints.ToDistinctArrayFromDistinct_Unchecked();

        PAssert.That(() => sut.UnderlyingArrayThatShouldNeverBeMutated() == ints);
    }

    [Fact]
    public void Created_DistinctArray_is_distinct()
    {
        var sut = new[] { 1, 1, 2, }.ToDistinctArray();

        PAssert.That(() => sut.Count == 2);
        PAssert.That(() => sut.SetEqual(new[] { 1, 2, }));
    }

    [Fact]
    public void SupportsForeach()
    {
        var sut = new[] { 1, 2, 3, 2, 1, }.ToDistinctArray();
        var sum = 0;
        foreach (var n in sut) {
            sum += n;
        }

        PAssert.That(() => sum == 6);
    }

    [Fact]
    public void EqualityIsByReferenceNotValue()
    {
        // ReSharper disable once EqualExpressionComparison
        PAssert.That(() => Enumerable.Range(1, 4).ToDistinctArray() != Enumerable.Range(1, 4).ToDistinctArray());
        var existingArr = new[] { "a", "b", "c", };
        var existingDistinctArr = existingArr.ToDistinctArrayFromDistinct_Unchecked();
        var aliasedDistinctArr = existingArr.ToDistinctArrayFromDistinct_Unchecked();
        var nonAliasedCopy = existingArr.ToDistinctArray();

        PAssert.That(() => existingDistinctArr == aliasedDistinctArr);
        PAssert.That(() => !(existingDistinctArr != aliasedDistinctArr));
        PAssert.That(() => existingDistinctArr.Equals(aliasedDistinctArr));
        PAssert.That(() => existingDistinctArr.GetHashCode() == aliasedDistinctArr.GetHashCode());

        PAssert.That(() => existingDistinctArr != nonAliasedCopy);
        PAssert.That(() => !(existingDistinctArr == nonAliasedCopy));
        PAssert.That(() => !existingDistinctArr.Equals(nonAliasedCopy));
        PAssert.That(() => existingDistinctArr.GetHashCode() != nonAliasedCopy.GetHashCode());

        var defaultValue = default(DistinctArray<string>);
        var explicitlyEmpty = Array.Empty<string>().ToDistinctArray();
        PAssert.That(() => defaultValue == explicitlyEmpty);
    }

    [Fact]
    public void EmptyCreatedInDifferentWaysAreEqual()
    {
        var defaultValue = default(DistinctArray<string>);
        // ReSharper disable once UseArrayEmptyMethod
#pragma warning disable CA1825 // Avoid zero-length array allocations
        var explicitlyEmpty = new string[0].ToDistinctArray();
#pragma warning restore CA1825 // Avoid zero-length array allocations
        PAssert.That(() => defaultValue == explicitlyEmpty);
        PAssert.That(() => defaultValue.Equals(explicitlyEmpty));
        PAssert.That(() => defaultValue.GetHashCode() == explicitlyEmpty.GetHashCode());
        PAssert.That(() => defaultValue.UnderlyingArrayThatShouldNeverBeMutated() == explicitlyEmpty.UnderlyingArrayThatShouldNeverBeMutated());
    }

    [Fact]
    public void OverloadForSetsWorks()
    {
        var hashSet = new[] { 1, 2, 4, 5, }.ToHashSet();
        var sut = hashSet.ToDistinctArray();

        PAssert.That(() => sut.Count == 4);
        PAssert.That(() => sut.SetEqual(hashSet));
    }

    [Fact]
    public void OverloadForDictionaryKeysWorks()
    {
        var hashSet = new[] { 1, 2, 4, 5, }.ToDictionary(i => i);
        var sut = hashSet.Keys.ToDistinctArray();

        PAssert.That(() => sut.Count == 4);
        PAssert.That(() => sut.SetEqual(new[] { 1, 2, 4, 5, }));
    }

    [Fact]
    public void Created_DistinctArray_with_custom_comparer_is_distinct()
    {
        var sut = new[] { 1, 1, 2, }.ToDistinctArray(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode()));

        PAssert.That(() => sut.Count == 2);
        PAssert.That(() => sut.SetEqual(new[] { 1, 2, }));
    }

    [Fact]
    public void Indexing_all_items_gives_equals_set()
    {
        var sut = new[] { 1, 1, 2, }.ToDistinctArray();

        PAssert.That(() => sut.Select((item, i) => sut[i]).SetEqual(sut));
    }
}
