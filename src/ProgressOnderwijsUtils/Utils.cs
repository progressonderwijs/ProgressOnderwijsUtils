using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
    public static class ErrorUtils
    {
        public static string TestErrorStackOverflow(int rounds)
        {
            //This is intended for testing error-handling in case of dramatic errors.
            return TestErrorStackOverflow(rounds + 1);
        }

        public static void TestErrorOutOfMemory()
        {
            var memorySlurper = new List<byte[]>();
            for (long i = 0; i < Int64.MaxValue; i++) //no way any machine has near 2^70 bytes of RAM - a zettabyte! no way, ever. ;-)
            {
                memorySlurper.Add(
                    Encoding.UTF8.GetBytes(
                        @"This is a simply string which is repeatedly put in memory to test the Out Of Memory condition.  It's encoded to make sure the program really touches the data and that therefore the OS really needs to allocate the memory, and can't just 'pretend'."));
            }
        }

        public static void TestErrorNormalException()
        {
            throw new ApplicationException("This is a test exception intended to test fault-tolerance.  User's shouldn't see it, of course!");
        }
    }

    public static class DisposableExtensions
    {
        public static T Using<TDisposable, T>(this TDisposable disposable, Func<TDisposable, T> func) where TDisposable : IDisposable
        {
            using (disposable)
                return func(disposable);
        }
    }

    public static class Utils
    {
        /// <summary>
        /// Compares two floating point number for approximate equality (up to a 1 part per 2^32 deviation)
        /// </summary>
        public static bool FuzzyEquals(double x, double y)
        {
            const double relativeEpsilon = 1.0 / (1ul << 32);

            double delta = Math.Abs(x - y);
            double magnitude = Math.Abs(x) + Math.Abs(y);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return x == y || delta / magnitude < relativeEpsilon;
        }

        public static Lazy<T> Lazy<T>(Func<T> factory) { return new Lazy<T>(factory, LazyThreadSafetyMode.ExecutionAndPublication); }

        public static bool ElfProef(int getal)
        {
            int res = 0;
            for (int i = 1; getal != 0; getal /= 10, ++i) {
                res += i * (getal % 10);
            }
            return res != 0 && res % 11 == 0;
        }

        /// <summary>
        /// Uses the sieve of erasthenos
        /// </summary>
        /// <returns>all 64-bit representable primes</returns>
        public static IEnumerable<ulong> Primes()
        {
            var primes = new List<ulong>();
            for (ulong i = 2; i != 0; i++) {
                if (primes.All(p => i % p != 0)) {
                    primes.Add(i);
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Swap two objects.
        /// </summary>
        public static void Swap<T>(ref T one, ref T other)
        {
            T tmp = one;
            one = other;
            other = tmp;
        }

        public static T Retry<T>(Func<T> func, Func<Exception, bool> shouldRetryOnThisFailure)
        {
            const int retryMAX = 5;

            int attempt = 0;
            while (true) {
                try {
                    return func();
                } catch (Exception e) {
                    if (attempt++ >= retryMAX || !shouldRetryOnThisFailure(e)) {
                        throw;
                    }
                }
            }
        }

        public static void Retry(CancellationToken cancel, Action action, Func<Exception, bool> shouldRetryOnThisFailure)
        {
            Retry(
                () => {
                    cancel.ThrowIfCancellationRequested();
                    action();
                    return default(object);
                },
                e => shouldRetryOnThisFailure(e) && !cancel.IsCancellationRequested);
        }

        public static T Retry<T>(CancellationToken cancel, Func<T> func, Func<Exception, bool> shouldRetryOnThisFailure)
        {
            return Retry(
                () => {
                    cancel.ThrowIfCancellationRequested();
                    return func();
                },
                e => shouldRetryOnThisFailure(e) && !cancel.IsCancellationRequested);
        }

        public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<T, IEnumerable<T>> edgeLookup)
        {
            return TransitiveClosure(elems, edgeLookup, EqualityComparer<T>.Default);
        }

        public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<T, IEnumerable<T>> edgeLookup, IEqualityComparer<T> comparer)
        {
            var distinctNewlyReachable = elems.ToArray();
            var set = distinctNewlyReachable.ToSet(comparer);
            while (distinctNewlyReachable.Length > 0) {
                distinctNewlyReachable = distinctNewlyReachable.SelectMany(edgeLookup).Where(set.Add).ToArray();
            }
            return set;
        }

        public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<IEnumerable<T>, IEnumerable<T>> multiEdgeLookup)
        {
            return TransitiveClosure(elems, multiEdgeLookup, EqualityComparer<T>.Default);
        }

        public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<IEnumerable<T>, IEnumerable<T>> multiEdgeLookup, IEqualityComparer<T> comparer)
        {
            var distinctNewlyReachable = elems.ToArray();
            var set = distinctNewlyReachable.ToSet(comparer);
            while (distinctNewlyReachable.Length > 0) {
                distinctNewlyReachable = multiEdgeLookup(distinctNewlyReachable).Where(set.Add).ToArray();
            }
            return set;
        }

        public static bool IsDbConnectionFailure(Exception e)
        {
            if (e == null) {
                return false;
            } else if (e is SqlException) {
                return e.Message.StartsWith("A transport-level error has occurred when receiving results from the server.") ||
                    e.Message.StartsWith("A transport-level error has occurred when sending the request to the server.") ||
                    e.Message.StartsWith("Timeout expired.");
            } else if (e is DBConcurrencyException) {
                return e.Message.StartsWith("Concurrency violation:");
            } else if (e is EntityException) {
                return (e.Message == "The underlying provider failed on Open.");
            } else if (e is AggregateException) {
                return ((AggregateException)e).Flatten().InnerExceptions.DefaultIfEmpty().All(IsDbConnectionFailure);
            } else {
                return IsDbConnectionFailure(e.InnerException);
            }
        }

        // ReSharper disable UnusedMember.Global
        // Deze F's zijn voor makkelijke type inference, dus worden misschien niet altijd gebruikt
        // maar wel goed om te houden
        public static Func<TR> F<TR>(Func<TR> v) { return v; } //purely for delegate type inference
        public static Func<T, TR> F<T, TR>(Func<T, TR> v) { return v; } //purely for delegate type inference
        public static Func<T1, T2, TR> F<T1, T2, TR>(Func<T1, T2, TR> v) { return v; } //purely for delegate type inference
        public static Func<T1, T2, T3, TR> F<T1, T2, T3, TR>(Func<T1, T2, T3, TR> v) { return v; } //purely for delegate type inference
        public static Expression<Func<TR>> E<TR>(Expression<Func<TR>> v) { return v; } //purely for delegate type inference
        public static Expression<Func<T, TR>> E<T, TR>(Expression<Func<T, TR>> v) { return v; } //purely for delegate type inference
        public static Expression<Func<T1, T2, TR>> E<T1, T2, TR>(Expression<Func<T1, T2, TR>> v) { return v; } //purely for delegate type inference
        // ReSharper restore UnusedMember.Global
        public static string GetSqlExceptionDetailsString(Exception exception)
        {
            SqlException sql = exception as SqlException ?? exception.InnerException as SqlException;
            return sql == null ? null : $"[code='{sql.ErrorCode:x}'; number='{sql.Number}'; state='{sql.State}']";
        }

        /// <summary>
        /// Executes a test body, marking a test as inconclusive rather than failed when a timeout occurs.
        /// </summary>
        public static void IgnoreTimeouts(Action test)
        {
            //Ideally, we'd retry the test, however, since lots of test are in a complex inheritance chain
            //we can't really seperate the test body from the side-effects in the setup/teardown.
            //In particuarly, a retry wouldn't run in its own environment.

            try {
                test();
            } catch (Exception e) {
                if (IsTimeoutException(e)) {
                    Assert.Inconclusive("TIMEOUT DETECTED\n\n" + e);
                } else if (e is AggregateException) {
                    // re-order de inner-exceptions zodat we tenminste de eerste non-timeout exceptie te zien krijgen
                    var ae = e as AggregateException;
                    throw new AggregateException(
                        ae.InnerExceptions.OrderBy(
                            x => x,
                            new ComparisonComparer<Exception>(
                                (a, b) => {
                                    if (IsTimeoutException(a) && IsTimeoutException(b)) {
                                        return 0;
                                    } else if (IsTimeoutException(b)) {
                                        return -1;
                                    } else if (IsTimeoutException(a)) {
                                        return 1;
                                    } else {
                                        return 0;
                                    }
                                })));
                } else {
                    throw;
                }
            }
        }

        static bool IsTimeoutException(Exception e)
        {
            if (e is AggregateException) {
                return (e as AggregateException).InnerExceptions.All(IsTimeoutException);
            }

            for (var current = e; current != null; current = current.InnerException) {
                var sqlE = current as SqlException;
                if (sqlE != null && sqlE.Number == -2) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsInUnitTest()
        {
            return RegisterTestingProgressTools.IsInUnitTest;
            //string procname = Process.GetCurrentProcess().ProcessName;
            //return procname.StartsWith("nunit") || procname.StartsWith("pnunit"); //also supports nunit-agent, nunit-console, nunit-x86, etc.
        }

        // vergelijk datums zonder milliseconden.
        public static bool DateTimeWithoutMillisecondsIsEqual(DateTime d1, DateTime d2) => d1.AddMilliseconds(-d1.Millisecond) == d2.AddMilliseconds(-d2.Millisecond);

        /// <summary>
        /// Geeft het verschil in maanden tussen twee datums
        /// </summary>
        public static int MaandSpan(DateTime d1, DateTime d2)
        {
            return Math.Abs(d1 > d2 ? (12 * (d1.Year - d2.Year) + d1.Month) - d2.Month : (12 * (d2.Year - d1.Year) + d2.Month) - d1.Month);
        }

        /// <summary>
        /// converteert incomplete studielinkdatums (bv 'yyyy-00-00' naar complete datum, 
        /// waarbij nulwaarden voor datum of maand worden omgezet naar de waarde 1
        /// Alleen voor KVA4
        /// </summary>
        public static DateTime? SLMaybeIncompleteDateConversion(string incompleteDate)
        {
            if (incompleteDate != null) {
                var incompleteDateFragments = incompleteDate.Split('-');
                string month = incompleteDateFragments[1] == "00" ? "1" : incompleteDateFragments[1];
                string date = incompleteDateFragments[2] == "00" ? "1" : incompleteDateFragments[2];
                return DateTime.Parse(incompleteDateFragments[0] + "/" + month + "/" + date);
            } else {
                return null;
            }
        }

        public static string ToSortableShortString(long value)
        {
            //This function is used on a hot-path in Programma and Resultaten export - it needs to be fast.

            char[] buffer = new char[14]; // log(2^31)/log(36) < 6 char; +1 for length+sign.
            int index = 0;
            bool isNeg = value < 0;
            if (isNeg) {
                while (value < 0) {
                    int digit = (int)(value % 36); //in range -35..0!!
                    value = value / 36;
                    buffer[index++] = MapToBase36Char(35 + digit);
                }
            } else {
                while (value > 0) {
                    int digit = (int)(value % 36);
                    value = value / 36;
                    buffer[index++] = MapToBase36Char(digit);
                }
            }
            Debug.Assert(index <= 13);
            int encodedLength = (isNeg ? -index : index) + 13; //-6..6; but for 64-bit -13..13 so to futureproof this offset by 13
            buffer[index++] = MapToBase36Char(encodedLength);

            for (int i = 0, j = index - 1; i < j; i++, j--) {
                var tmp = buffer[i];
                buffer[i] = buffer[j];
                buffer[j] = tmp;
            }

            return new string(buffer, 0, index);
        }

        static char MapToBase36Char(int digit) => (char)((digit < 10 ? '0' : 'a' - 10) + digit);

        /// <summary>
        /// This is almost equivalent to num.ToString("f"+precision), but around 10 times faster.
        /// 
        /// Differences: 
        ///   - rounding differences may exist for doubles like 1.005 which are not precisely representable.
        ///   - numbers over (2^64 - 2^10)/(2^precision) are slow.
        /// </summary>
        public static string ToFixedPointString(double number, CultureInfo culture, int precision)
        {
            //TODO:add tests
            var fI = culture.NumberFormat;
            var str = new char[32]; //64-bit:20 digits, leaves 12 for ridiculous separators.
            int idx = 0;
            bool isNeg = number < 0;
            if (isNeg) {
                number = -number;
            }

            ulong mult = 1;
            for (int i = 0; i < precision; i++) {
                mult *= 10;
            }
            var rounded = number * mult + 0.5;
            if (!(rounded <= ulong.MaxValue - 1024)) {
                if (double.IsNaN(number)) {
                    return fI.NaNSymbol;
                }
                if (double.IsInfinity(number)) {
                    return isNeg ? fI.NegativeInfinitySymbol : fI.PositiveInfinitySymbol;
                }
                return number.ToString("f" + precision, culture);
            }
            var x = (ulong)rounded;

            isNeg = isNeg && x > 0;

            if (precision > 0) {
                do {
                    var tmp = x;
                    x = x / 10;
                    str[idx++] = (char)('0' + (tmp - x * 10));
                }
                while (idx < precision);
                str[idx++] = fI.PercentDecimalSeparator[0];
            }
            do {
                var tmp = x;
                x = x / 10;
                str[idx++] = (char)('0' + (tmp - x * 10));
            }
            while (x != 0);
            if (isNeg) {
                str[idx++] = fI.NegativeSign[0];
            }
            for (int i = 0, j = idx - 1; i < j; i++, j--) {
                var tmp = str[i];
                str[i] = str[j];
                str[j] = tmp;
            }
            return new string(str, 0, idx);
        }

        public static DateTime? DateMax(DateTime? d1, DateTime? d2)
        {
            if (d1 == null) {
                return d2;
            }

            if (d2 == null) {
                return d1;
            }

            return d2 > d1 ? d2 : d1;
        }

        public static bool GenerateForLanguage(DocumentLanguage doc, Taal language)
        {
            switch (doc) {
                case DocumentLanguage.Dutch:
                    return language == Taal.NL;
                case DocumentLanguage.English:
                    return language == Taal.EN;
                case DocumentLanguage.German:
                    return language == Taal.DU;
                case DocumentLanguage.StudentPreferenceNlEn:
                case DocumentLanguage.CoursePreferenceNlEn:
                case DocumentLanguage.ProgramPreferenceNlEn:
                    return language == Taal.NL || language == Taal.EN;
                case DocumentLanguage.StudentPreferenceNlEnDu:
                    return language == Taal.NL || language == Taal.EN || language == Taal.DU;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static decimal RoundUp(decimal input, int places)
        {
            var multiplier = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(places)));
            return Math.Ceiling(input * multiplier) / multiplier;
        }
    }

    public class ComparisonComparer<T> : IComparer<T>
    {
        readonly Comparison<T> comparer;
        public ComparisonComparer(Comparison<T> comparer) { this.comparer = comparer; }
        public int Compare(T x, T y) => comparer(x, y);
    }

    public class EqualsEqualityComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> equals;
        readonly Func<T, int> hashCode;

        public EqualsEqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode = null)
        {
            this.equals = equals;
            this.hashCode = hashCode;
        }

        public bool Equals(T x, T y) => equals(x, y);
        public int GetHashCode(T obj) => hashCode == null ? obj.GetHashCode() : hashCode(obj);
    }
}
