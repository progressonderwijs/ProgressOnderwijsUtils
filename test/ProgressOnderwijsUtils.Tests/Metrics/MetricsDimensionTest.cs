using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class MetricsDimensionTest
{
    /// <summary>
    /// A dimension with tracked endpoints of its own choosing, to test the layout of the slots.
    /// </summary>
    sealed class TrackedEndpoints(IEnumerable<TestEndpoint> tracked) : MetricsDimension<TestEndpoint>(MetricsHistogram.Log2Buckets(), tracked)
    {
        protected override (TestEndpoint? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            => throw new NotSupportedException();
    }

    enum ByteBased : byte
    {
        One = 1,
    }

    sealed class ByteBasedDimension() : MetricsDimension<ByteBased>(MetricsHistogram.Log2Buckets(), [ByteBased.One,])
    {
        protected override (ByteBased? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            => throw new NotSupportedException();
    }

    [Fact]
    public void Every_tracked_value_finds_its_own_slot()
    {
        var dimension = new TestRequestDuration();

        PAssert.That(() => dimension.Values[MetricsDimension<TestEndpoint>.OtherSlot] == null);
        PAssert.That(() => dimension.Values.Length == TestRequestDuration.TrackedEndpoints.Length + 1);
        for (var slot = 1; slot < dimension.Values.Length; slot++) {
            var value = dimension.Values[slot];
            var found = dimension.SlotOf(value);
            var expected = slot;
            PAssert.That(() => found == expected);
        }
    }

    /// <summary>
    /// Everything without a slot of its own shares the other slot; a measurement may never fall outside the array.
    /// </summary>
    [Fact]
    public void Untracked_undefined_and_missing_values_share_the_other_slot()
    {
        var dimension = new TestRequestDuration();

        PAssert.That(() => dimension.SlotOf(null) == MetricsDimension<TestEndpoint>.OtherSlot);
        PAssert.That(() => dimension.SlotOf(TestEndpoint.Status) == MetricsDimension<TestEndpoint>.OtherSlot);
        PAssert.That(() => dimension.SlotOf((TestEndpoint)(-2)) == MetricsDimension<TestEndpoint>.OtherSlot);
        PAssert.That(() => dimension.SlotOf((TestEndpoint)int.MaxValue) == MetricsDimension<TestEndpoint>.OtherSlot);
    }

    [Fact]
    public void The_cells_of_a_slot_are_laid_out_per_bucket_and_outcome()
    {
        var dimension = new TestRequestDuration();
        var bucketCount = dimension.Histogram.BucketCount;

        var cells = (
            from slot in Enumerable.Range(0, dimension.Values.Length)
            from bucketIndex in Enumerable.Range(0, bucketCount)
            from failed in new[] { false, true, }
            select dimension.CellIndex(slot, failed, bucketIndex)
        ).ToArray();

        PAssert.That(() => cells.SequenceEqual(Enumerable.Range(0, dimension.Size)));
        PAssert.That(() => dimension.IsFailed(dimension.CellIndex(1, true, 0)) && !dimension.IsFailed(dimension.CellIndex(1, false, 0)));
    }

    [Fact]
    public void A_duration_ends_up_in_the_cell_of_its_value_outcome_and_bucket()
    {
        var dimension = new TestRequestDuration();
        var slot = dimension.SlotOf(TestEndpoint.Login);

        PAssert.That(() => dimension.Index(TestEndpoint.Login, true, 0.005) == dimension.CellIndex(slot, true, dimension.Histogram.GetBucketIndex(0.005)));
    }

    [Fact]
    public void A_value_tracked_twice_is_refused()
        => _ = Assert.Throws<ArgumentException>(() => new TrackedEndpoints([TestEndpoint.Home, TestEndpoint.Home,]));

    /// <summary>
    /// A negative value would never be measured as a tracked value, so that has to stand out at startup already.
    /// </summary>
    [Fact]
    public void A_negative_value_cannot_be_tracked()
        => _ = Assert.Throws<ArgumentException>(() => new TrackedEndpoints([(TestEndpoint)(-2),]));

    [Fact]
    public void An_enum_that_is_not_int_based_is_refused()
        => _ = Assert.Throws<ArgumentException>(() => new ByteBasedDimension());

    [Fact]
    public void A_dimension_without_tracked_values_puts_everything_in_the_other_slot()
    {
        var dimension = new TrackedEndpoints([]);

        PAssert.That(() => dimension.Values.Length == 1);
        PAssert.That(() => dimension.SlotOf(TestEndpoint.Home) == 0);
    }
}
