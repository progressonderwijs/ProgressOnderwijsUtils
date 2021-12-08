namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class ProjectingEqualityComparerTest
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    sealed record Example(string A, int B, DateTime? C, int[]? D);

    readonly ProjectingEqualityComparer<Example> EmptyComparer = new();
    readonly Example A1 = new("A", 1, null, null);
    readonly Example A0 = new("A", 0, null, null);
    readonly Example a1 = new("a", 1, null, null);
    readonly Example b0 = new("b", 0, null, null);
    readonly Example A1_date = new("A", 1, new(2021, 2, 3), null);
    readonly Example A1_date_time = new("A", 1, new DateTime(2021, 2, 3).AddHours(9.5), null);
    readonly Example A0_date = new("A", 0, new(2021, 2, 3), null);
    readonly Example a1_date = new("a", 1, new(2021, 2, 3), null);
    readonly Example b0_date = new("b", 0, new(2021, 2, 3), null);
    readonly Example A1_arr123 = new("A", 1, null, new[] { 1, 2, 3, });
    readonly Example A1_arr123_other = new("A", 1, null, new[] { 1, 2, 3, });
    readonly Example A1_arr456 = new("A", 1, null, new[] { 4, 5, 6, });

    [Fact]
    public void Precondition_ExamplesAreUnique()
        => PAssert.That(() => !new[] { A1, A0, a1, b0, }.ContainsDuplicates(), "precondition");

    [Fact]
    public void EmptyCrashes()
        => Assert.ThrowsAny<Exception>(() => new ProjectingEqualityComparer<Example>().Finish());

    [Fact]
    public void SingleStringComparison()
    {
        var eq = EmptyComparer.AddKeyColumn(o => o.A).Finish();
        PAssert.That(() => eq.Equals(A1, A0));
        PAssert.That(() => !eq.Equals(A1, a1));
    }

    [Fact]
    public void SingleStringComparisonWithCaseInsensitivity()
    {
        var eq = EmptyComparer.AddKeyColumn(o => o.A, StringComparer.OrdinalIgnoreCase).Finish();
        PAssert.That(() => eq.Equals(A1, A0));
        PAssert.That(() => eq.Equals(A1, a1));
        PAssert.That(() => !eq.Equals(A0, b0));
    }

    [Fact]
    public void SingleStringComparisonWithCaseInsensitivity_WithNullableDate()
    {
        var eq = EmptyComparer.AddKeyColumn(o => o.A, StringComparer.OrdinalIgnoreCase).AddKeyColumn(o => o.C).Finish();
        PAssert.That(() => eq.Equals(A1, A0));
        PAssert.That(() => eq.Equals(A1, a1));
        PAssert.That(() => !eq.Equals(A0, b0));

        PAssert.That(() => eq.Equals(A1_date, A0_date));
        PAssert.That(() => eq.Equals(A1_date, a1_date));
        PAssert.That(() => !eq.Equals(A0_date, b0_date));

        PAssert.That(() => !eq.Equals(A1_date, A1));
        PAssert.That(() => !eq.Equals(A1_date, A1_date_time));
        PAssert.That(() => !eq.Equals(A0_date, A0));
        PAssert.That(() => !eq.Equals(a1_date, a1));
        PAssert.That(() => !eq.Equals(b0_date, b0));
    }

    [Fact]
    public void ByArrayReferenceComparison()
    {
        var eq = EmptyComparer.AddKeyColumn_ViaObjectEquals(o => o.D).Finish();
        PAssert.That(() => eq.Equals(A1, A0));
        PAssert.That(() => eq.Equals(A1, a1));
        PAssert.That(() => !eq.Equals(A1, A1_arr123));
        PAssert.That(() => !eq.Equals(A1_arr123, A1_arr123_other));
    }

    [Fact]
    public void ByArrayContentComparison()
    {
        var eq = EmptyComparer.AddKeyColumn(o => o.D, SequenceEqualityComparer<int>.Default).Finish();
        PAssert.That(() => eq.Equals(A1, A0));
        PAssert.That(() => eq.Equals(A1, a1));
        PAssert.That(() => !eq.Equals(A1, A1_arr123));
        PAssert.That(() => !eq.Equals(A1_arr456, A1_arr123));
        PAssert.That(() => eq.Equals(A1_arr123, A1_arr123_other));
    }
}
