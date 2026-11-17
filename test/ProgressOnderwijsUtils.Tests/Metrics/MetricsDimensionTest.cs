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

    /// <summary>
    /// The lookup table is as long as the highest tracked value, so a high value would cost a large table for a single
    /// slot.
    /// </summary>
    [Fact]
    public void A_value_above_the_lookup_table_limit_cannot_be_tracked()
    {
        var highest = new TrackedEndpoints([(TestEndpoint)4096,]);

        PAssert.That(() => highest.SlotOf((TestEndpoint)4096) == 1);
        _ = Assert.Throws<ArgumentException>(() => new TrackedEndpoints([(TestEndpoint)4097,]));
    }

    [Fact]
    public void An_enum_that_is_not_int_based_is_refused()
        => _ = Assert.Throws<ArgumentException>(() => new ByteBasedDimension());

    static TestEndpoint[] Endpoints(int count)
        => [.. Enumerable.Range(1, count).Select(value => (TestEndpoint)value),];

    /// <summary>
    /// Collecting sums the cells of a second on the stack, so a dimension that does not fit there must stand out at
    /// startup already.
    /// </summary>
    [Fact]
    public void A_dimension_whose_cells_per_second_do_not_fit_on_the_stack_is_refused()
    {
        var cellsPerSlot = MetricsHistogram.Log2Buckets().BucketCount * 2;
        var mostTracked = MetricsDimension<TestEndpoint>.MaxCellsPerSecond / cellsPerSlot - 1;

        var largest = new TrackedEndpoints(Endpoints(mostTracked));

        PAssert.That(() => largest.Size <= MetricsDimension<TestEndpoint>.MaxCellsPerSecond);
        _ = Assert.Throws<ArgumentException>(() => new TrackedEndpoints(Endpoints(mostTracked + 1)));
    }

    /// <summary>
    /// Sampling and collecting the connection usage keep their slots on the stack, so a layout that does not fit there
    /// must stand out at startup already.
    /// </summary>
    [Fact]
    public void A_connection_pool_layout_that_does_not_fit_on_the_stack_is_refused()
    {
        var poolCount = Enum.GetValues<TestPool>().Length;
        var mostTracked = ConnectionPoolSlots<TestEndpoint, TestPool>.MaxCount / poolCount - 1;

        var largest = new ConnectionPoolSlots<TestEndpoint, TestPool>(new TrackedEndpoints(Endpoints(mostTracked)));

        PAssert.That(() => largest.Count <= ConnectionPoolSlots<TestEndpoint, TestPool>.MaxCount);
        _ = Assert.Throws<ArgumentException>(() => new ConnectionPoolSlots<TestEndpoint, TestPool>(new TrackedEndpoints(Endpoints(mostTracked + 1))));
    }

    [Fact]
    public void A_dimension_without_tracked_values_puts_everything_in_the_other_slot()
    {
        var dimension = new TrackedEndpoints([]);

        PAssert.That(() => dimension.Values.Length == 1);
        PAssert.That(() => dimension.SlotOf(TestEndpoint.Home) == 0);
    }
}
