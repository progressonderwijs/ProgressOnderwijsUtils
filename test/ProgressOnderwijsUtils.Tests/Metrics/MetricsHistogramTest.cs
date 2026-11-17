using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class MetricsHistogramTest
{
    public static TheoryData<string> Histograms()
        => [nameof(MetricsHistogram.Log2Buckets), nameof(MetricsHistogram.ExponentialLatencyBuckets),];

    static MetricsHistogram Histogram(string name)
        => name switch {
            nameof(MetricsHistogram.Log2Buckets) => MetricsHistogram.Log2Buckets(),
            nameof(MetricsHistogram.ExponentialLatencyBuckets) => MetricsHistogram.ExponentialLatencyBuckets(),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };

    /// <summary>
    /// A boundary is the inclusive lower bound of its bucket: exactly on the boundary a duration already
    /// belongs in the bucket above, and a fraction below it still in the bucket before.
    /// </summary>
    [Theory]
    [MemberData(nameof(Histograms))]
    public void Every_boundary_starts_its_own_bucket(string name)
    {
        var histogram = Histogram(name);

        for (var bucket = 1; bucket < histogram.BucketCount; bucket++) {
            var boundary = histogram.Boundaries[bucket];
            var atBoundary = histogram.GetBucketIndex(boundary);
            var justBelow = histogram.GetBucketIndex(Math.BitDecrement(boundary));
            var expected = bucket;
            PAssert.That(() => atBoundary == expected);
            PAssert.That(() => justBelow == expected - 1);
        }
    }

    /// <summary>
    /// A measurement must never fall outside the histogram, not even with a value that an instrument
    /// should not really produce.
    /// </summary>
    [Theory]
    [MemberData(nameof(Histograms))]
    public void Odd_durations_land_in_the_outer_buckets(string name)
    {
        var histogram = Histogram(name);
        var last = histogram.BucketCount - 1;

        PAssert.That(() => histogram.GetBucketIndex(0.0) == 0);
        PAssert.That(() => histogram.GetBucketIndex(-1.0) == 0);
        PAssert.That(() => histogram.GetBucketIndex(double.NegativeInfinity) == 0);
        PAssert.That(() => histogram.GetBucketIndex(double.NaN) == 0);
        PAssert.That(() => histogram.GetBucketIndex(double.MaxValue) == last);
        PAssert.That(() => histogram.GetBucketIndex(double.PositiveInfinity) == last);
    }

    static long[] Counts(MetricsHistogram histogram, params (int BucketIndex, long Count)[] filled)
    {
        var counts = new long[histogram.BucketCount];
        foreach (var (bucketIndex, count) in filled) {
            counts[bucketIndex] = count;
        }

        return counts;
    }

    /// <summary>
    /// The counts of a single second, as the collector delivers them.
    /// </summary>
    static int[] SecondCounts(MetricsHistogram histogram, params (int BucketIndex, int Count)[] filled)
    {
        var counts = new int[histogram.BucketCount];
        foreach (var (bucketIndex, count) in filled) {
            counts[bucketIndex] = count;
        }

        return counts;
    }

    /// <summary>
    /// A percentile is always exactly one of the boundaries, so we check which boundary it is instead of
    /// comparing floating-point numbers with each other. Null if it is not a boundary.
    /// </summary>
    static int? BoundaryIndexOf(MetricsHistogram histogram, double? seconds)
        => seconds is { } value && histogram.Boundaries.IndexOf(value) is var index and >= 0 ? index : null;

    [Fact]
    public void An_empty_histogram_has_no_percentile()
    {
        var histogram = MetricsHistogram.Log2Buckets();

        var percentile = histogram.PercentileSeconds(Counts(histogram), 0.5);

        PAssert.That(() => percentile == null);
    }

    [Fact]
    public void Counts_that_do_not_match_the_buckets_are_rejected()
    {
        var histogram = MetricsHistogram.Log2Buckets();

        _ = Assert.Throws<ArgumentException>(() => histogram.PercentileSeconds(new long[histogram.BucketCount - 1], 0.5));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    public void A_quantile_outside_zero_to_one_is_rejected(double quantile)
    {
        var histogram = MetricsHistogram.Log2Buckets();

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => histogram.PercentileSeconds(Counts(histogram, (3, 1)), quantile));
    }

    [Fact]
    public void All_measurements_in_one_bucket_give_its_upper_bound()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var counts = Counts(histogram, (3, 10));

        var p50 = histogram.PercentileSeconds(counts, 0.5);
        var p95 = histogram.PercentileSeconds(counts, 0.95);

        PAssert.That(() => BoundaryIndexOf(histogram, p50) == 4);
        PAssert.That(() => BoundaryIndexOf(histogram, p95) == 4);
    }

    [Fact]
    public void A_rank_on_the_edge_of_a_bucket_stays_in_the_lower_bucket()
    {
        // ten measurements, five per bucket: the median is the fifth measurement and that is still in the first bucket
        var histogram = MetricsHistogram.Log2Buckets();
        var counts = Counts(histogram, (2, 5), (5, 5));

        var p50 = histogram.PercentileSeconds(counts, 0.5);

        PAssert.That(() => BoundaryIndexOf(histogram, p50) == 3);
    }

    [Fact]
    public void P50_and_p95_can_fall_in_different_buckets()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var counts = Counts(histogram, (2, 90), (7, 10));

        var p50 = histogram.PercentileSeconds(counts, 0.5);
        var p95 = histogram.PercentileSeconds(counts, 0.95);

        PAssert.That(() => BoundaryIndexOf(histogram, p50) == 3);
        PAssert.That(() => BoundaryIndexOf(histogram, p95) == 8);
    }

    [Theory]
    [MemberData(nameof(Histograms))]
    public void A_rank_in_the_open_last_bucket_gives_the_highest_finite_boundary(string name)
    {
        var histogram = Histogram(name);
        var counts = Counts(histogram, (histogram.BucketCount - 1, 3));

        var p95 = histogram.PercentileSeconds(counts, 0.95);

        var highestFiniteBoundaryIndex = histogram.Boundaries.Length - 2;
        PAssert.That(() => BoundaryIndexOf(histogram, p95) == highestFiniteBoundaryIndex);
    }

    [Theory]
    [MemberData(nameof(Histograms))]
    public void Every_possible_percentile_is_one_of_the_inner_boundaries(string name)
    {
        var histogram = Histogram(name);

        var percentilesInMilliseconds = Enumerable.Range(0, histogram.BucketCount)
            .Select(bucketIndex => histogram.PercentileSeconds(Counts(histogram, (bucketIndex, 1)), 0.5).AssertNotNull() * 1000)
            .Distinct()
            .ToArray();

        PAssert.That(() => percentilesInMilliseconds.SequenceEqual(histogram.InnerBoundariesMilliseconds));
    }

    [Fact]
    public void Totals_add_and_subtract_the_counts_of_seconds()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var totals = histogram.CreateTotals();

        totals.Add(SecondCounts(histogram, (2, 3)));
        totals.Add(SecondCounts(histogram, (2, 1), (5, 4)));
        totals.Subtract(SecondCounts(histogram, (5, 4)));

        PAssert.That(() => totals.Count == 4);
        PAssert.That(() => BoundaryIndexOf(histogram, totals.PercentileSeconds(0.95)) == 3);
    }

    [Fact]
    public void Totals_give_the_same_percentile_as_the_summed_counts()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var first = SecondCounts(histogram, (1, 5), (4, 2));
        var second = SecondCounts(histogram, (4, 3), (9, 1));
        var summed = first.Zip(second, (a, b) => (long)a + b).ToArray();
        var totals = histogram.CreateTotals();

        totals.Add(first);
        totals.Add(second);

        var fromTotals = totals.PercentileSeconds(0.95);
        var fromSum = histogram.PercentileSeconds(summed, 0.95);
        PAssert.That(() => BoundaryIndexOf(histogram, fromTotals) == BoundaryIndexOf(histogram, fromSum));
    }

    [Fact]
    public void Totals_reject_counts_that_do_not_match_the_buckets()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var totals = histogram.CreateTotals();

        _ = Assert.Throws<ArgumentException>(() => totals.Add(new int[histogram.BucketCount + 1]));
        _ = Assert.Throws<ArgumentException>(() => totals.Subtract(new int[histogram.BucketCount + 1]));
    }

    [Fact]
    public void Subtracting_more_than_was_added_is_rejected_and_leaves_the_totals_untouched()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var totals = histogram.CreateTotals();
        totals.Add(SecondCounts(histogram, (1, 2), (2, 1)));

        _ = Assert.Throws<InvalidOperationException>(() => totals.Subtract(SecondCounts(histogram, (1, 1), (2, 2))));

        PAssert.That(() => totals.Count == 3);
    }

    [Fact]
    public void Cleared_totals_have_no_percentile()
    {
        var histogram = MetricsHistogram.Log2Buckets();
        var totals = histogram.CreateTotals();
        totals.Add(SecondCounts(histogram, (1, 2)));

        totals.Clear();

        PAssert.That(() => totals.Count == 0);
        PAssert.That(() => totals.PercentileSeconds(0.5) == null);
    }
}
