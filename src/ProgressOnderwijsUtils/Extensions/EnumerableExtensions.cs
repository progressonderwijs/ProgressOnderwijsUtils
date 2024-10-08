namespace ProgressOnderwijsUtils;

public static class EnumerableExtensions
{
    /// <summary>
    /// returns the (first) index of an element in a collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">the enumerable to search in</param>
    /// <param name="elem">the element searched</param>
    /// <returns>an int</returns>
    /// <remarks>If you just want to test existance the native "Contains" would be sufficient</remarks>
    [Pure]
    public static int IndexOf<T>(this IEnumerable<T> list, T elem)
    {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }
        var retval = 0;
        foreach (var item in list) {
            if (Equals(elem, item)) {
                return retval;
            }
            retval++;
        }
        return -1;
    }

    [Pure]
    public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> matcher)
    {
        if (list == null) {
            throw new ArgumentNullException(nameof(list));
        }
        if (matcher == null) {
            throw new ArgumentNullException(nameof(matcher));
        }
        var retval = 0;
        foreach (var item in list) {
            if (matcher(item)) {
                return retval;
            }
            retval++;
        }
        return -1;
    }

    [Pure]
    public static bool None<TSource>(this IEnumerable<TSource> source)
        // ReSharper disable once UseNone
        => !source.Any();

    [Pure]
    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        // ReSharper disable once UseNone
        => !source.Any(predicate);

    [Pure]
    public static bool None<TSource>(this IQueryable<TSource> source)
        // ReSharper disable once UseNone
        => !source.Any();

    [Pure]
    public static bool None<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        // ReSharper disable once UseNone
        => !source.Any(predicate);

    [Pure]
    public static IReadOnlyList<T> ToReadOnly<T>(this IEnumerable<T> list)
        => list.ToArray();

    [Pure]
    public static bool SetEqual<T>(this IEnumerable<T> list, IEnumerable<T> other)
        => list.ToHashSet().SetEquals(other);

    [Pure]
    public static bool SetEqual<T>(this IEnumerable<T> list, IEnumerable<T> other, IEqualityComparer<T> comparer)
        => list.ToHashSet(comparer).SetEquals(other);

    [Pure]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? list)
        => list ?? Enumerable.Empty<T>();

    [Pure]
    public static int GetSequenceHashCode<T>(IEnumerable<T> list, IEqualityComparer<T>? elementComparer = null)
    {
        var hc = new HashCode();
        foreach (var item in list) {
            hc.Add(item, elementComparer);
        }
        return hc.ToHashCode();
    }

    [Pure]
    public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
        => ContainsDuplicates(list, EqualityComparer<T>.Default);

    [Pure]
    public static bool ContainsDuplicates<T>(this IEnumerable<T> list, IEqualityComparer<T> comparer)
    {
        var set = new HashSet<T>(comparer);
        foreach (var item in list) {
            if (!set.Add(item)) {
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> list)
        where T : class
    {
        foreach (var item in list) {
            if (item is not null) {
                yield return item;
            }
        }
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> list)
        where T : struct
    {
        foreach (var item in list) {
            if (item is not null) {
                yield return item.Value;
            }
        }
    }

    public static T[] WhereNotNull<T>(this T?[] kids)
        where T : class
    {
        var nullCount = 0;
        foreach (var k in kids) {
            nullCount += k is null ? 1 : 0;
        }
        if (nullCount is 0) {
            // ReSharper disable once NullableWarningSuppressionIsUsed
            return kids!;
        }
        var output = new T[kids.Length - nullCount];
        var outIdx = 0;
        foreach (var k in kids) {
            if (k is not null) {
                output[outIdx++] = k;
            }
        }
        return output;
    }

    public static T[] WhereNotNull<T>(this T?[] kids)
        where T : struct
    {
        var nullCount = 0;
        foreach (var k in kids) {
            nullCount += k is null ? 1 : 0;
        }
        var output = new T[kids.Length - nullCount];
        var outIdx = 0;
        foreach (var k in kids) {
            if (k is not null) {
                output[outIdx++] = k.Value;
            }
        }
        return output;
    }

    [Pure]
    public static SortedList<TKey, TVal> ToSortedList<T, TKey, TVal>(this IEnumerable<T> list, Func<T, TKey> keySelector, Func<T, TVal> valSelector)
        where TKey : notnull
        => list.ToSortedList(keySelector, valSelector, Comparer<TKey>.Default);

    [Pure]
    public static SortedList<TKey, TVal> ToSortedList<T, TKey, TVal>(
        this IEnumerable<T> list,
        Func<T, TKey> keySelector,
        Func<T, TVal> valSelector,
        IComparer<TKey> keyComparer)
        where TKey : notnull
    {
        var retval = new SortedList<TKey, TVal>(keyComparer);
        foreach (var item in list.OrderBy(keySelector, keyComparer)) {
            retval.Add(keySelector(item), valSelector(item));
        }
        return retval;
    }

    [Pure]
    public static Dictionary<TKey, TValue> ToGroupedDictionary<TElem, TKey, TValue>(
        this IEnumerable<TElem> list,
        Func<TElem, TKey> keyLookup,
        Func<TKey, IEnumerable<TElem>, TValue> groupMap
    )
        where TKey : notnull
        => list.ToGroupedDictionary(keyLookup, groupMap, EqualityComparer<TKey>.Default);

    [Pure]
    public static Dictionary<TKey, TValue> ToGroupedDictionary<TElem, TKey, TValue>(
        this IEnumerable<TElem> list,
        Func<TElem, TKey> keyLookup,
        Func<TKey, IEnumerable<TElem>, TValue> groupMap,
        IEqualityComparer<TKey> comparer
    )
        where TKey : notnull
    {
        var groups = new Dictionary<TKey, List<TElem>>(comparer);
        foreach (var elem in list) {
            var key = keyLookup(elem);
            if (!groups.TryGetValue(key, out var group)) {
                groups.Add(key, group = new());
            }
            group.Add(elem);
        }
        var retval = new Dictionary<TKey, TValue>(groups.Count, comparer);
        foreach (var group in groups) {
            retval.Add(group.Key, groupMap(group.Key, group.Value));
        }
        return retval;
    }

    [Pure]
    public static string ToCsv<T>(this IEnumerable<T> items, bool useHeader = true, string delimiter = "\t", string newline = "\n", bool useQuotesForStrings = false)
        where T : IReadImplicitly
    {
        var csvBuilder = new StringBuilder();

        var properties = PocoUtils.GetProperties<T>().Where(p => p.CanRead).ToArray();

        if (useHeader) {
            _ = csvBuilder.Append(properties.Select(p => p.Name.ToCsvValue(delimiter, useQuotesForStrings)).JoinStrings(delimiter) + newline);
        }
        foreach (var item in items) {
            var line = properties.Select(p => p.Getter.AssertNotNull()(item).ToCsvValue(delimiter, useQuotesForStrings)).JoinStrings(delimiter);
            _ = csvBuilder.Append(line + newline);
        }
        return csvBuilder.ToString();
    }

    [Pure]
    static string? ToCsvValue<T>(this T? item, string delimiter, bool useQuotesForStrings)
    {
        var csvValueWithoutQuotes = item?.ToString() ?? "";

        if (csvValueWithoutQuotes.Contains(delimiter) && !useQuotesForStrings) {
            new ArgumentException("item contains illegal characters, use useQuotesForStrings=true").ThrowPreconditionViolation();
        }

        if (!useQuotesForStrings) {
            return csvValueWithoutQuotes;
        }

        if (item == null) {
            return "\"\"";
        }

        if (item is string str) {
            return $"\"{str.Replace("\"", "\\\"")}\"";
        } else {
            return item.ToString();
        }
    }
}
