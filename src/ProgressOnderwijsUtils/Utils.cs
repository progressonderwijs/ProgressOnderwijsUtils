namespace ProgressOnderwijsUtils;

public static class Utils
{
    /// <summary>
    /// Compares two floating point number for approximate equality (up to a 1 part per 2^13 deviation, i.e. 4 significant digits)
    /// </summary>
    public static bool FuzzyEquals(double x, double y)
    {
        const double relativeEpsilon = 1.0 / (1ul << 13);

        var delta = Math.Abs(x - y);
        var magnitude = Math.Abs(x) + Math.Abs(y);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return x == y || delta / magnitude < relativeEpsilon;
    }

    public static Func<T> Lazy<T>(Func<T> factory)
    {
        //Ideally we'd use Lazy<T>, but that either caches exceptions or fails to lock around initialization. see: https://github.com/dotnet/runtime/issues/27421#issuecomment-424732179
        var value = default(T?);
        var initialized = false;
        var sync = (object)factory;

        return () => LazyInitializer.EnsureInitialized(ref value, ref initialized, ref sync, factory);
    }

    public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<T, IEnumerable<T>> edgeLookup)
        => TransitiveClosure(elems, edgeLookup, EqualityComparer<T>.Default);

    public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<T, IEnumerable<T>> edgeLookup, IEqualityComparer<T> comparer)
        => TransitiveClosure(elems, distinctArray => distinctArray.UnderlyingArrayThatShouldNeverBeMutated().SelectMany(edgeLookup), comparer);

    public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<DistinctArray<T>, IEnumerable<T>> multiEdgeLookup)
        => TransitiveClosure(elems, multiEdgeLookup, EqualityComparer<T>.Default);

    public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<DistinctArray<T>, IEnumerable<T>> multiEdgeLookup, IEqualityComparer<T> comparer)
    {
        var set = elems.ToHashSet(comparer);
        var distinctNewlyReachable = set.ToDistinctArray();
        while (distinctNewlyReachable.Count > 0) {
            distinctNewlyReachable = multiEdgeLookup(distinctNewlyReachable).Where(set.Add).ToArray().ToDistinctArrayFromDistinct_Unchecked();
        }
        return set;
    }

    // ReSharper disable UnusedMember.Global
    // Deze F's zijn voor makkelijke type inference, dus worden misschien niet altijd gebruikt
    // maar wel goed om te houden
    public static Func<TR> F<TR>(Func<TR> v)
        => v;
    //purely for delegate type inference

    public static Func<T, TR> F<T, TR>(Func<T, TR> v)
        => v;
    //purely for delegate type inference

    public static Func<T1, T2, TR> F<T1, T2, TR>(Func<T1, T2, TR> v)
        => v;
    //purely for delegate type inference

    public static Func<T1, T2, T3, TR> F<T1, T2, T3, TR>(Func<T1, T2, T3, TR> v)
        => v;
    //purely for delegate type inference

    public static Expression<Func<TR>> E<TR>(Expression<Func<TR>> v)
        => v;
    //purely for delegate type inference

    public static Expression<Func<T, TR>> E<T, TR>(Expression<Func<T, TR>> v)
        => v;
    //purely for delegate type inference

    public static Expression<Func<T1, T2, TR>> E<T1, T2, TR>(Expression<Func<T1, T2, TR>> v)
        => v;
    //purely for delegate type inference
    // ReSharper restore UnusedMember.Global

    /// <summary>
    /// Geeft het verschil in maanden tussen twee datums
    /// </summary>
    public static int MaandSpan(DateTime d1, DateTime d2)
        => Math.Abs(d1 > d2 ? 12 * (d1.Year - d2.Year) + d1.Month - d2.Month : 12 * (d2.Year - d1.Year) + d2.Month - d1.Month);

    /// <summary>
    /// Executions a computation with reliable cleanup (like try...finally or using(...) {}).
    /// When both computation and cleanup throw exceptions, wraps both exceptions in an AggregateException.
    /// </summary>
    public static T TryWithCleanup<T>(Func<T> computation, Action cleanup)
    {
        var completedOk = false;
        try {
            var retval = computation();
            completedOk = true;
            cleanup();
            return retval;
        } catch (Exception computationEx) when (!completedOk) {
            //Catch(cleanup) is checked with an if and not in the when clause because
            //a function in the when clause causes the exection order to change
            if (Catch(cleanup) is { } cleanupEx) {
                throw new AggregateException("Both the computation and the cleanup code crashed", computationEx, cleanupEx);
            }
            throw;
        }
    }

    /// <summary>
    /// Executions a computation with reliable cleanup (like try...finally or using(...) {}).
    /// When both computation and cleanup throw exceptions, wraps both exceptions in an AggregateException.
    /// </summary>
    public static void TryWithCleanup(Action computation, Action cleanup)
        => TryWithCleanup(computation.ToUnitReturningFunc(), cleanup);

    static Exception? Catch(Action cleanup)
    {
        try {
            cleanup();
        } catch (Exception e) {
            return e;
        }
        return null;
    }

    /// <summary>
    /// Volgordebehoudende transformatie van getal naar string, dus:
    /// 
    /// a kleiner dan b
    ///    is equivalent aan
    /// ToSortableShortString(a) kleiner dan ToShortableShortString(b)
    /// 
    /// Deze eigenschap geldt wanneer je m verifieert in C#, JS, SQL (, etc?)
    /// (want er worden alleen letters in 1 case en cijfers gebruikt)
    /// </summary>
    public static string ToSortableShortString(long value)
    {
        var sb = new StringBuilder();
        sb.AppendSortableShortString(value);
        return sb.ToString();
    }

    public static void AppendSortableShortString(this StringBuilder target, long value)
    {
        //This function is used on a hot-path in Programma and Resultaten export - it needs to be fast.
        if (value < 0) {
            SssNegHelper(target, value, 0);
        } else {
            SssHelper(target, value, 0);
        }
    }

    static void SssHelper(StringBuilder target, long value, int index)
    {
        if (value != 0) {
            var digit = (int)(value % 36);
            SssHelper(target, value / 36, index + 1);
            _ = target.Append(MapToBase36Char(digit));
        } else {
            var encodedLength = index + 13; //-6..6 for int32; but for 64-bit -13..13 so to futureproof this offset by 13
            _ = target.Append(MapToBase36Char(encodedLength));
        }
    }

    static void SssNegHelper(StringBuilder target, long value, int index)
    {
        if (value != 0) {
            var digit = (int)(value % 36); //in range -35..0!!
            SssNegHelper(target, value / 36, index + 1);
            _ = target.Append(MapToBase36Char(35 + digit));
        } else {
            var encodedLength = 13 - index; //-6..6; but for 64-bit -13..13 so to futureproof this offset by 13
            _ = target.Append(MapToBase36Char(encodedLength));
        }
    }

    static char MapToBase36Char(int digit)
        => (char)((digit < 10 ? '0' : 'a' - 10) + digit);

    /// <summary>
    /// This is almost equivalent to num.ToString("f"+precision), but around 10 times faster.
    /// 
    /// Differences:
    ///   - rounding differences may exist for doubles like 1.005 which are not precisely representable.
    ///   - numbers over (2^64 - 2^10)/(2^precision) are slow.
    /// </summary>
    public static string ToFixedPointString(double number, CultureInfo culture, int precision)
    {
        var fI = culture.NumberFormat;
        Span<char> str = stackalloc char[32]; //64-bit:20 digits, leaves 12 for ridiculous separators.

        var mult = 1ul;
        for (var i = 0; i < precision; i++) {
            mult *= 10;
        }
        var rounded = Math.Abs(number) * mult + 0.5;
        if (!(rounded <= ulong.MaxValue - 1024)) {
            if (double.IsNaN(number)) {
                return fI.NaNSymbol;
            }
            if (double.IsInfinity(number)) {
                return number < 0 ? fI.NegativeInfinitySymbol : fI.PositiveInfinitySymbol;
            }
            return number.ToString($"f{precision}", culture);
        }
        var x = (ulong)rounded;

        var isNeg = x > 0 && number < 0;

        var idx = 0;
        if (precision > 0) {
            do {
                var tmp = x;
                x /= 10;
                str[idx++] = (char)('0' + (tmp - x * 10));
            } while (idx < precision);
            str[idx++] = fI.PercentDecimalSeparator[0];
        }
        do {
            var tmp = x;
            x /= 10;
            str[idx++] = (char)('0' + (tmp - x * 10));
        } while (x != 0);
        if (isNeg) {
            str[idx++] = fI.NegativeSign[0];
        }
        for (int i = 0, j = idx - 1; i < j; i++, j--) {
            (str[i], str[j]) = (str[j], str[i]);
        }
        return new(str.Slice(0, idx));
    }

    /// <summary>
    /// returns 0 if input is zero; otherwise returns the only int for which the postcondition holds
    /// Postcondition: (1ul &lt;&lt; result) &lt;= x &lt; (1ul &lt;&lt; result+1)
    /// </summary>
    public static int LogBase2RoundedDown(uint x)
    {
        var res = 0;
        if (x >= 1 << 16) {
            res += 16;
            x >>= 16;
        }
        if (x >= 1 << 8) {
            res += 8;
            x >>= 8;
        }
        if (x >= 1 << 4) {
            res += 4;
            x >>= 4;
        }
        if (x >= 1 << 2) {
            res += 2;
            x >>= 2;
        }
        if (x >= 1 << 1) {
            res += 1;
            //conceptually: x = x >> 1;
        }
        return res;
    }

    /// <summary>
    /// returns 0 if input is zero; otherwise returns the only int for which the postcondition holds
    /// Postcondition: (1ul &lt;&lt; result-1) &lt; x &lt;= (1ul &lt;&lt; result)
    /// </summary>
    public static int LogBase2RoundedUp(uint x)
        => x <= 1 ? 0 : LogBase2RoundedDown(x - 1) + 1;

    public static CancellationToken CreateLinkedTokenWith(this CancellationToken a, CancellationToken b)
        => b == CancellationToken.None
            ? a
            : a == CancellationToken.None
                ? b
                : CancellationTokenSource.CreateLinkedTokenSource(b, a).Token;
}

public sealed class ComparisonComparer<T> : IComparer<T>
{
    readonly Comparison<T> comparer;

    public ComparisonComparer(Comparison<T> comparer)
        => this.comparer = comparer;

    public int Compare(T? x, T? y)
        => comparer(x!, y!);
}

public sealed class EqualsEqualityComparer<T> : IEqualityComparer<T>
{
    readonly Func<T, T, bool> equals;
    readonly Func<T, int>? hashCode;

    public EqualsEqualityComparer(Func<T, T, bool> equals, Func<T, int>? hashCode = null)
    {
        this.equals = equals;
        this.hashCode = hashCode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(T? x, T? y)
        => x == null ? y == null : y != null && equals(x, y);

    public int GetHashCode(T obj)
        => hashCode != null
            ? hashCode(obj)
            : obj != null
                ? obj.GetHashCode()
                : 0;
}
