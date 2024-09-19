namespace ProgressOnderwijsUtils.Tests;

public sealed class EnumerableExtensionsTests
{
    [Fact]
    public void IndexOfCheck()
    {
        // ReSharper disable NotAccessedVariable
        int ignore;
        // ReSharper restore NotAccessedVariable
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(2) == 1);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(4) == -1);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(1) == 0);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(3) == 2);
        PAssert.That(() => new[] { 1, 2, 3, 1, 2, 3, }.IndexOf(3) == 2);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(x => x == 2) == 1);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(x => x % 7 == 0) == -1);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(x => x < 3) == 0);
        PAssert.That(() => new[] { 1, 2, 3, }.IndexOf(x => x % 2 == 1 && x > 1) == 2);
        PAssert.That(() => new[] { 1, 2, 3, 1, 2, 3, }.IndexOf(x => x % 2 == 1 && x > 1) == 2);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _ = Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(2));
        _ = Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(x => x == 2));
        _ = Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void AsReadOnlyTest()
    {
        var nums = Enumerable.Range(1, 5).Reverse().ToArray();
        var copy = nums.ToReadOnly();
        PAssert.That(() => nums.SequenceEqual(copy));
        Array.Sort(nums);
        PAssert.That(() => !nums.SequenceEqual(copy));
        PAssert.That(() => copy.SequenceEqual(Enumerable.Range(1, 5).Reverse()));
    }

    [Fact]
    public void TestIndexOf()
    {
        var lst = new List<string> { "een", "twee", "drie", };
        //int[] ints = { 1, 2, 3, 4, 5 };
        PAssert.That(() => lst.IndexOf("twee") == 1);
        PAssert.That(() => lst.IndexOf("tweeeneenhalf") == -1);
    }

    [Fact]
    public void TestFirstIndexOfDups()
    {
        PAssert.That(() => new[] { 0, 0, 1, 1, 2, 2, }.IndexOf(0) == 0);
        PAssert.That(() => new[] { 0, 0, 1, 1, 2, 2, }.IndexOf(1) == 2);
        PAssert.That(() => new[] { 0, 0, 1, 1, 2, 2, }.IndexOf(2) == 4);
    }

    [Fact]
    public void WhereNotNull_RemovesNullsWithoutCompilerWarningForReferenceTypes()
    {
        var sampleNullableData = new[] { "test", null, "this", }.AsEnumerable();
        var nonNullItems = sampleNullableData.WhereNotNull(); //inferred as IEnumerable<string>
        var lengths = nonNullItems.Select(item => item.Length); //no nullability warning here; no crash here
        PAssert.That(() => lengths.Max() == 4);
    }

    [Fact]
    public void WhereNotNull_RemovesNullsWithoutCompilerWarningForValueTypes()
    {
        var sampleNullableData = new[] { 37, default(int?), 42, }.AsEnumerable();
        var nonNullItems = sampleNullableData.WhereNotNull(); //inferred as IEnumerable<int>
        PAssert.That(() => nonNullItems.SequenceEqual(new[] { 37, 42, }));
    }

    [Fact]
    public void ArrayWhereNotNull_RemovesNullsWithoutCompilerWarningForReferenceTypes()
    {
        var sampleNullableData = new[] { "test", null, "this", };
        var nonNullItems = sampleNullableData.WhereNotNull(); //inferred as IEnumerable<string>
        var lengths = nonNullItems.Select(item => item.Length); //no nullability warning here; no crash here
        PAssert.That(() => lengths.Max() == 4);
    }

    [Fact]
    public void ArrayWhereNotNull_WithoutRefNullsReturnsInput()
    {
        var sampleNullableData = new[] { "test", "this".PretendNullable(), };
        var nonNullItems = sampleNullableData.WhereNotNull(); //inferred as IEnumerable<string>
        PAssert.That(() => sampleNullableData == nonNullItems);
        var sampleNullableDataEnumerable = sampleNullableData.AsEnumerable();
        var nonNullItemsEnumerable = sampleNullableDataEnumerable.WhereNotNull();
        PAssert.That(() => sampleNullableDataEnumerable != nonNullItemsEnumerable);
    }

    [Fact]
    public void ArrayWhereNotNull_RemovesNullsWithoutCompilerWarningForValueTypes()
    {
        var sampleNullableData = new[] { 37, default(int?), 42, };
        var nonNullItems = sampleNullableData.WhereNotNull(); //inferred as IEnumerable<int>
        PAssert.That(() => nonNullItems.SequenceEqual(new[] { 37, 42, }));
    }

    [Fact]
    public void EmptyIfNullOk()
    {
        PAssert.That(() => new[] { 0, 1, 2, }.EmptyIfNull().SequenceEqual(new[] { 0, 1, 2, }));
        PAssert.That(() => default(int[]).EmptyIfNull().SequenceEqual(new int[] { }));
        PAssert.That(() => default(int[]) == null);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        PAssert.That(() => default(int[]) != default(int[]).EmptyIfNull());
        var arr = new[] { 0, 1, 2, };
        PAssert.That(() => arr.EmptyIfNull() == arr);
    }
}
