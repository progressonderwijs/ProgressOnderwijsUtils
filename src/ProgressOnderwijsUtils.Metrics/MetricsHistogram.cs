using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// A bucket of a histogram: where it starts, and its name in Dutch and in English. The lower bound is inclusive. A
/// bucket ends where the next one starts; the last one is open-ended.
/// </summary>
public readonly record struct MetricsBucket(double LowerBoundSeconds, string NL, string EN);

public sealed class MetricsHistogram
{
    public static MetricsHistogram Log2Buckets()
        => new(
            [
                new(0.0, "[0 - 4) ms", "[0 - 4) ms"),
                new(0.004, "[4 - 8) ms", "[4 - 8) ms"),
                new(0.008, "[8 - 16) ms", "[8 - 16) ms"),
                new(0.016, "[16 - 32) ms", "[16 - 32) ms"),
                new(0.032, "[32 - 64) ms", "[32 - 64) ms"),
                new(0.064, "[64 - 128) ms", "[64 - 128) ms"),
                new(0.128, "[128 - 256) ms", "[128 - 256) ms"),
                new(0.256, "[256 - 512) ms", "[256 - 512) ms"),
                new(0.512, "[512 - 1.024) ms", "[512 - 1,024) ms"),
                new(1.024, "[1,024 - 2,048) s", "[1.024 - 2.048) s"),
                new(2.048, "[2,048 - 4,096) s", "[2.048 - 4.096) s"),
                new(4.096, "[4,096 - 8,192) s", "[4.096 - 8.192) s"),
                new(8.192, "[8,192 - 16,384) s", "[8.192 - 16.384) s"),
                new(16.384, ">= 16,384 s", ">= 16.384 s"),
            ]
        );

    public static MetricsHistogram ExponentialLatencyBuckets()
        => new(
            [
                new(0.0, "[0 - 1) ms", "[0 - 1) ms"),
                new(0.001, "[1 - 10) ms", "[1 - 10) ms"),
                new(0.01, "[10 - 50) ms", "[10 - 50) ms"),
                new(0.05, "[50 - 250) ms", "[50 - 250) ms"),
                new(0.25, "[250 - 1.000) ms", "[250 - 1,000) ms"),
                new(1.0, "[1 - 5) s", "[1 - 5) s"),
                new(5.0, "[5 - 30) s", "[5 - 30) s"),
                new(30.0, ">= 30 s", ">= 30 s"),
            ]
        );

    /// <summary>
    /// Buckets that do not start at zero or do not ascend are refused on creation: the bucket index would no longer
    /// match the bounds, and that only surfaces as skewed data once it is stored.
    /// </summary>
    public MetricsHistogram(ImmutableArray<MetricsBucket> buckets)
    {
        if (buckets.IsDefaultOrEmpty || buckets[0].LowerBoundSeconds != 0.0) {
            var lowerBounds = buckets.IsDefault ? "" : buckets.Select(bucket => bucket.LowerBoundSeconds.ToString(CultureInfo.InvariantCulture)).JoinStrings(", ");
            throw new ArgumentException($"Expected at least one bucket, the first one starting at 0, but got the lower bounds [{lowerBounds}].", nameof(buckets));
        }

        for (var i = 1; i < buckets.Length; i++) {
            if (!(buckets[i].LowerBoundSeconds > buckets[i - 1].LowerBoundSeconds)) {
                throw new ArgumentException(
                    $"Expected strictly ascending lower bounds, but bucket {i} ({buckets[i].LowerBoundSeconds}) does not start after bucket {i - 1} ({buckets[i - 1].LowerBoundSeconds}).",
                    nameof(buckets)
                );
            }
        }

        Buckets = buckets;
    }

    /// <summary>
    /// Starts off with the bucket at 0s.
    /// Does not follow the OpenTelemetry convention where they let 0 out making it minus infinite.
    /// </summary>
    public ImmutableArray<MetricsBucket> Buckets { get; }

    public int BucketCount
        => Buckets.Length;

    /// <summary>
    /// The exclusive upper bound of a bucket, which is where the next bucket starts; null for the last bucket, which
    /// is open-ended.
    /// </summary>
    public double? UpperBoundSeconds(int bucketIndex)
        => bucketIndex == BucketCount - 1 ? null : Buckets[bucketIndex + 1].LowerBoundSeconds;

    /// <summary>
    /// The boundaries between the buckets in milliseconds, so without the 0 where the first bucket starts: exactly the
    /// values <see cref="PercentileSeconds"/> can return, and with that the ticks on an axis of percentiles.
    /// </summary>
    public ImmutableArray<double> InnerBoundariesMilliseconds
        => [.. Buckets.Skip(1).Select(bucket => bucket.LowerBoundSeconds * 1000),];

    /// <summary>
    /// The percentile as the upper bound of the bucket that holds the requested rank: "P95 is at most this much".
    /// There is no interpolation within a bucket, because where the measurements lie in it is unknown. The last
    /// bucket is open-ended, which cannot be plotted; for that one its lower bound is returned, just like
    /// Prometheus does. Null when nothing was measured.
    /// </summary>
    /// <param name="counts">The number of measurements per bucket of a single histogram.</param>
    /// <param name="quantile">The percentile as a fraction, greater than 0 and at most 1.</param>
    public double? PercentileSeconds(ReadOnlySpan<long> counts, double quantile)
    {
        EnsureBucketCount(counts.Length, nameof(counts));
        // as the negation of the valid range, so that NaN is refused as well: every comparison with NaN is false
        if (quantile is not (> 0.0 and <= 1.0)) {
            throw new ArgumentOutOfRangeException(nameof(quantile), quantile, "Expected a quantile greater than 0 and at most 1.");
        }

        var total = 0L;
        foreach (var count in counts) {
            total += count;
        }

        if (total == 0) {
            return null;
        }

        var rank = (long)Math.Ceiling(quantile * total);
        var cumulative = 0L;
        for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
            cumulative += counts[bucketIndex];
            if (cumulative >= rank) {
                return UpperBoundSeconds(bucketIndex) ?? Buckets[^1].LowerBoundSeconds;
            }
        }

        throw new UnreachableException($"The cumulative count {cumulative} never reached rank {rank}.");
    }

    public Totals CreateTotals()
        => new(this);

    void EnsureBucketCount(int length, string paramName)
    {
        if (length != BucketCount) {
            throw new ArgumentException($"Expected {BucketCount} bucket counts, but got {length}.", paramName);
        }
    }

    /// <summary>
    /// Summed bucket counts of a histogram, for example over a series of seconds or over several slots. The collector
    /// counts per second in ints; the totals are longs, because enough seconds together no longer fit in an int.
    /// </summary>
    public sealed class Totals
    {
        readonly MetricsHistogram histogram;
        readonly long[] counts;

        internal Totals(MetricsHistogram histogram)
        {
            this.histogram = histogram;
            counts = new long[histogram.BucketCount];
        }

        public long Count
            => counts.Sum();

        public void Add(ReadOnlySpan<int> added)
        {
            histogram.EnsureBucketCount(added.Length, nameof(added));
            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                counts[bucketIndex] += added[bucketIndex];
            }
        }

        /// <summary>
        /// Takes previously added counts off again. When a bucket would drop below zero, something is being
        /// subtracted that was never added; the totals are then left untouched and this throws.
        /// </summary>
        public void Subtract(ReadOnlySpan<int> subtracted)
        {
            histogram.EnsureBucketCount(subtracted.Length, nameof(subtracted));
            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                if (counts[bucketIndex] < subtracted[bucketIndex]) {
                    throw new InvalidOperationException($"Subtracting {subtracted[bucketIndex]} from bucket {bucketIndex} would make its total of {counts[bucketIndex]} negative.");
                }
            }

            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                counts[bucketIndex] -= subtracted[bucketIndex];
            }
        }

        public void Clear()
            => Array.Clear(counts);

        public double? PercentileSeconds(double quantile)
            => histogram.PercentileSeconds(counts, quantile);
    }

    /// <summary>
    /// The bucket index is the number of later buckets whose lower bound the duration reaches. The bounds ascend, so
    /// the first one that is not reached ends the count.
    /// A negative duration or NaN reaches no bound at all and ends up in the first bucket; everything from the lower
    /// bound of the last bucket on, infinity included, in the last.
    /// </summary>
    public int GetBucketIndex(double duration)
    {
        var index = 0;
        foreach (var bucket in Buckets.AsSpan()[1..]) {
            if (!(duration >= bucket.LowerBoundSeconds)) {
                break;
            }
            index++;
        }
        return index;
    }
}
