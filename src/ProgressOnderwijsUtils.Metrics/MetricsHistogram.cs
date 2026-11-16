using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The name of a histogram bucket, in Dutch and in English.
/// </summary>
public readonly record struct MetricsBucketLabel(string NL, string EN);

public sealed class MetricsHistogram
{
    public static MetricsHistogram Log2Buckets()
        => new(
            [0.0, 0.004, 0.008, 0.016, 0.032, 0.064, 0.128, 0.256, 0.512, 1.024, 2.048, 4.096, 8.192, 16.384, double.MaxValue,],
            [
                new("[0 - 4) ms", "[0 - 4) ms"),
                new("[4 - 8) ms", "[4 - 8) ms"),
                new("[8 - 16) ms", "[8 - 16) ms"),
                new("[16 - 32) ms", "[16 - 32) ms"),
                new("[32 - 64) ms", "[32 - 64) ms"),
                new("[64 - 128) ms", "[64 - 128) ms"),
                new("[128 - 256) ms", "[128 - 256) ms"),
                new("[256 - 512) ms", "[256 - 512) ms"),
                new("[512 - 1.024) ms", "[512 - 1,024) ms"),
                new("[1,024 - 2,048) s", "[1.024 - 2.048) s"),
                new("[2,048 - 4,096) s", "[2.048 - 4.096) s"),
                new("[4,096 - 8,192) s", "[4.096 - 8.192) s"),
                new("[8,192 - 16,384) s", "[8.192 - 16.384) s"),
                new(">= 16,384 s", ">= 16.384 s"),
            ]
        );

    public static MetricsHistogram ExponentialLatencyBuckets()
        => new(
            [0.0, 0.001, 0.01, 0.05, 0.25, 1.0, 5.0, 30.0, double.MaxValue,],
            [
                new("[0 - 1) ms", "[0 - 1) ms"),
                new("[1 - 10) ms", "[1 - 10) ms"),
                new("[10 - 50) ms", "[10 - 50) ms"),
                new("[50 - 250) ms", "[50 - 250) ms"),
                new("[250 - 1.000) ms", "[250 - 1,000) ms"),
                new("[1 - 5) s", "[1 - 5) s"),
                new("[5 - 30) s", "[5 - 30) s"),
                new(">= 30 s", ">= 30 s"),
            ]
        );

    readonly Vector256<double>[] thresholds;

    /// <summary>
    /// A label belongs to exactly one bucket. When those get out of step, an index points at the wrong label and that
    /// only surfaces as skewed data once it is stored; that is why such a histogram is refused on creation. The same
    /// goes for boundaries that do not start at zero or do not ascend: the bucket index then no longer matches the
    /// boundaries.
    /// </summary>
    public MetricsHistogram(ImmutableArray<double> boundaries, ImmutableArray<MetricsBucketLabel> labels)
    {
        if (boundaries.Length < 2 || boundaries[0] != 0.0) {
            throw new ArgumentException(
                $"Expected at least two boundaries, starting at 0, but got [{boundaries.Select(b => b.ToString(CultureInfo.InvariantCulture)).JoinStrings(", ")}].",
                nameof(boundaries)
            );
        }

        for (var i = 1; i < boundaries.Length; i++) {
            if (!(boundaries[i] > boundaries[i - 1])) {
                throw new ArgumentException($"Expected strictly ascending boundaries, but boundary {i} ({boundaries[i]}) does not exceed boundary {i - 1} ({boundaries[i - 1]}).", nameof(boundaries));
            }
        }

        if (labels.Length != boundaries.Length - 1) {
            throw new ArgumentException($"Expected {boundaries.Length - 1} histogram labels to match the boundaries, but got {labels.Length}.", nameof(labels));
        }

        if (boundaries[^1] != double.MaxValue && !double.IsPositiveInfinity(boundaries[^1])) {
            throw new ArgumentException("Expected an open-ended final bucket, ending at double.MaxValue or positive infinity.", nameof(boundaries));
        }

        Boundaries = boundaries;
        Labels = labels;
        thresholds = Thresholds(boundaries);
    }

    /// <summary>
    /// In seconds; starts off with 0s; inclusive lower-bound, exclusive upper bound.
    /// Does not follow the OpenTelemetry convention where they let 0 out making it minus infinite.
    /// </summary>
    public ImmutableArray<double> Boundaries { get; }

    /// <summary>
    /// Starts off with the first bucket, thus N array, just like the measurements.
    /// </summary>
    public ImmutableArray<MetricsBucketLabel> Labels { get; }

    /// <summary>
    /// The boundaries are lower bounds plus the closing upper bound, so there is exactly one more of them than there
    /// are buckets.
    /// </summary>
    public int BucketCount
        => Boundaries.Length - 1;

    /// <summary>
    /// The inner boundaries in milliseconds, so without the 0 where the first bucket starts and without the closing
    /// upper bound of the last bucket: exactly the values <see cref="PercentileSeconds{T}"/> can return, and with that
    /// the ticks on an axis of percentiles.
    /// </summary>
    public ImmutableArray<double> InnerBoundariesMilliseconds
        => [.. Boundaries.Skip(1).SkipLast(1).Select(boundary => boundary * 1000),];

    /// <summary>
    /// The percentile as the upper bound of the bucket that holds the requested rank: "P95 is at most this much".
    /// There is no interpolation within a bucket, because where the measurements lie in it is unknown. The last
    /// bucket is open-ended, which cannot be plotted; for that one the highest finite boundary is returned, just like
    /// Prometheus does. Null when nothing was measured.
    /// </summary>
    /// <param name="counts">The number of measurements per bucket of a single histogram.</param>
    /// <param name="quantile">The percentile as a fraction, greater than 0 and at most 1.</param>
    public double? PercentileSeconds<T>(ReadOnlySpan<T> counts, double quantile)
        where T : IBinaryInteger<T>
    {
        EnsureBucketCount(counts.Length, nameof(counts));
        // as the negation of the valid range, so that NaN is refused as well: every comparison with NaN is false
        if (quantile is not (> 0.0 and <= 1.0)) {
            throw new ArgumentOutOfRangeException(nameof(quantile), quantile, "Expected a quantile greater than 0 and at most 1.");
        }

        var total = 0L;
        foreach (var count in counts) {
            total += long.CreateChecked(count);
        }

        if (total == 0) {
            return null;
        }

        var rank = (long)Math.Ceiling(quantile * total);
        var cumulative = 0L;
        for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
            cumulative += long.CreateChecked(counts[bucketIndex]);
            if (cumulative >= rank) {
                return bucketIndex == BucketCount - 1 ? Boundaries[^2] : Boundaries[bucketIndex + 1];
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
    /// Summed bucket counts of a histogram, for example over a series of seconds or over several slots. The totals
    /// are longs: enough seconds of int counts together no longer fit in an int.
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

        public void Add<T>(ReadOnlySpan<T> added)
            where T : IBinaryInteger<T>
        {
            histogram.EnsureBucketCount(added.Length, nameof(added));
            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                counts[bucketIndex] += long.CreateChecked(added[bucketIndex]);
            }
        }

        /// <summary>
        /// Takes previously added counts off again. When a bucket would drop below zero, something is being
        /// subtracted that was never added; the totals are then left untouched and this throws.
        /// </summary>
        public void Subtract<T>(ReadOnlySpan<T> subtracted)
            where T : IBinaryInteger<T>
        {
            histogram.EnsureBucketCount(subtracted.Length, nameof(subtracted));
            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                if (counts[bucketIndex] < long.CreateChecked(subtracted[bucketIndex])) {
                    throw new InvalidOperationException($"Subtracting {subtracted[bucketIndex]} from bucket {bucketIndex} would make its total of {counts[bucketIndex]} negative.");
                }
            }

            for (var bucketIndex = 0; bucketIndex < counts.Length; bucketIndex++) {
                counts[bucketIndex] -= long.CreateChecked(subtracted[bucketIndex]);
            }
        }

        public void Clear()
            => Array.Clear(counts);

        public double? PercentileSeconds(double quantile)
            => histogram.PercentileSeconds(counts, quantile);
    }

    /// <summary>
    /// The bucket index is the number of inner boundaries the duration reaches: the boundaries ascend, so whoever
    /// reaches k of them, reaches exactly the first k. That counting happens four boundaries at a time.
    /// A negative duration or NaN reaches no boundary at all and ends up in the first bucket; everything from the
    /// last inner boundary on, infinity included, in the last.
    /// </summary>
    public int GetBucketIndex(double duration)
    {
        var value = Vector256.Create(duration);
        var index = 0;
        foreach (var chunk in thresholds) {
            index += BitOperations.PopCount(Vector256.GreaterThanOrEqual(value, chunk).ExtractMostSignificantBits());
        }

        return index;
    }

    /// <summary>
    /// The inner boundaries, so without the 0 where the first bucket starts and without the closing upper bound of
    /// the last bucket, in blocks of a vector. The last block is padded with NaN: every comparison with it is false
    /// and so never counts. Infinity would count for an infinite duration, and then place it past the last bucket.
    /// </summary>
    static Vector256<double>[] Thresholds(ImmutableArray<double> boundaries)
    {
        var inner = boundaries.AsSpan()[1..^1];
        var padded = new double[(inner.Length + Vector256<double>.Count - 1) / Vector256<double>.Count * Vector256<double>.Count];
        padded.AsSpan().Fill(double.NaN);
        inner.CopyTo(padded);

        var chunks = new Vector256<double>[padded.Length / Vector256<double>.Count];
        for (var i = 0; i < chunks.Length; i++) {
            chunks[i] = Vector256.Create(padded.AsSpan(i * Vector256<double>.Count, Vector256<double>.Count));
        }

        return chunks;
    }
}
