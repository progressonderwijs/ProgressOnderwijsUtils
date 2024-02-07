namespace ProgressOnderwijsUtils.Collections;

public static class SortedArraySet
{
    public static SortedArraySet<T> MergeSets<T>(this IEnumerable<SortedArraySet<T>> inputSets)
        => SortedArraySet<T>.Algorithms.MergeSets(inputSets);

    public static bool LessThan<T>(this IComparer<T> comparer, T a, T b)
        => comparer.Compare(a, b) < 0;

    public static SortedArraySet<T> Empty<T>()
        => FromSortedValues([], Comparer<T>.Default);

    public static SortedArraySet<T> Empty<T>(IComparer<T> comparer)
        => FromSortedValues([], comparer);

    static SortedArraySet<T> FromSortedValues<T>(T[] comparables, IComparer<T> comparer)
        => new(comparables, comparer);

    public static SortedArraySet<T> FromValues<T>(IEnumerable<T> values)
        => FromValues(values, Comparer<T>.Default);

    public static SortedArraySet<T> FromValues<T>(IEnumerable<T> values, IComparer<T> comparer)
        => FromValues(values as T[] ?? values.ToArray(), comparer);

    public static SortedArraySet<T> FromValues<T>(T[] values)
        => FromValues(values, Comparer<T>.Default);

    public static SortedArraySet<T> FromValues<T>(T[] values, IComparer<T> comparer)
    {
        for (var i = 1; i < values.Length; i++) {
            if (!comparer.LessThan(values[i - 1], values[i])) {
                return FromMutableUnsortedTmpArray(values.ToArray(), comparer);
            }
        }
        return FromSortedValues(values, comparer);
    }

    internal static SortedArraySet<T> FromMutableUnsortedTmpArray<T>(T[] privateMutableArray, IComparer<T> comparer)
    {
        privateMutableArray.AsSpan().Sort(comparer);
        var newLength = SortedArraySet<T>.Algorithms.CountAndMoveDistinctValuesToFront(privateMutableArray, comparer);
        Array.Resize(ref privateMutableArray, newLength);
        return FromSortedValues(privateMutableArray, comparer);
    }
}

public readonly struct SortedArraySet<T> : IEquatable<SortedArraySet<T>>, IReadOnlyList<T>
{
    public SortedArraySet() { 
        comparer = Comparer<T>.Default;
        sortedDistinctValues = [];
    }
    internal SortedArraySet(T[] sortedDistinctValues, IComparer<T> comparer)
    {
        this.sortedDistinctValues = sortedDistinctValues;
        this.comparer = comparer;
    }

    readonly IComparer<T> comparer;
    readonly T[] sortedDistinctValues;

    public T[] ValuesInOrder
        => sortedDistinctValues;

    [Pure]
    public bool Contains(T value)
        => sortedDistinctValues is not null
            && Algorithms.IdxAfterLastLtNode(sortedDistinctValues, value, comparer) is var idxAfterLastLtNode
            && idxAfterLastLtNode < sortedDistinctValues.Length
            && !comparer.LessThan(value, sortedDistinctValues[idxAfterLastLtNode]);

    [Pure]
    public bool IsSubsetOf(SortedArraySet<T> potentialSuperset)
        => IsSubset_OfLargeSuperset_Recursive(sortedDistinctValues, potentialSuperset.sortedDistinctValues, comparer);

    public static SortedArraySet<T> Create(ReadOnlySpan<T> content) 
        => SortedArraySet.FromMutableUnsortedTmpArray(content.ToArray(), Comparer<T>.Default);

    static bool IsSubset_OfLargeSuperset_Recursive(ReadOnlySpan<T> sortedSubSet, ReadOnlySpan<T> sortedSuperSet, IComparer<T> comparer)
    {
        if (sortedSubSet.Length == 0) {
            return true;
        }
        if ((sortedSuperSet.Length >> 1) - 9 <= sortedSubSet.Length) {
            return IsSubset_Scan(sortedSubSet, sortedSuperSet, comparer);
        }
        var midIdx = sortedSubSet.Length >> 1;
        var value = sortedSubSet[midIdx];
        var idxAfterLastLtNode = Algorithms.IdxAfterLastLtNode(sortedSuperSet, value, comparer);
        if (idxAfterLastLtNode >= sortedSuperSet.Length || comparer.LessThan(value, sortedSuperSet[idxAfterLastLtNode])) {
            return false;
        }
        return IsSubset_OfLargeSuperset_Recursive(sortedSubSet[..midIdx], sortedSuperSet[..idxAfterLastLtNode], comparer)
            && IsSubset_OfLargeSuperset_Recursive(sortedSubSet[(midIdx + 1)..], sortedSuperSet[(idxAfterLastLtNode + 1)..], comparer);
    }

    static bool IsSubset_Scan(ReadOnlySpan<T> sortedSubSet, ReadOnlySpan<T> sortedSuperSet, IComparer<T> comparer)
    {
        var subValue = sortedSubSet[0];
        foreach (var supValue in sortedSuperSet) {
            var compare = comparer.Compare(supValue, subValue);
            if (compare >= 0) {
                if (compare == 0) {
                    if (sortedSubSet.Length > 1) {
                        sortedSubSet = sortedSubSet[1..];
                        subValue = sortedSubSet[0];
                    } else {
                        return true; //found the last subValue in sortedSuperSet; sortedSubSet is a true subset of sortedSuperSet;
                    }
                } else {
                    return false; //i.e. subValue < supValue, and since all further elements of sortedSuperSet are even greater, subValue cannot exist in sortedSuperSet: subset isn't a subset of superset.
                }
            }
        }
        return false; //we've gone through the entire sortedSuperSet, but haven't yet found all of sortedSubSet; i.e. NOT a subset.
    }

    public static class Algorithms
    {
        public static int CountAndMoveDistinctValuesToFront(Span<T> arr, IComparer<T> comparer)
        {
            if (arr.Length < 2) {
                return arr.Length;
            }
            var distinctUptoIdx = 0;
            var readIdx = 1;
            do {
                if (comparer.Compare(arr[distinctUptoIdx], arr[readIdx]) != 0) {
                    distinctUptoIdx++;
                    arr[distinctUptoIdx] = arr[readIdx];
                }
                readIdx++;
            } while (readIdx < arr.Length);
            return distinctUptoIdx + 1;
        }

        public static int IdxAfterLastLtNode(ReadOnlySpan<T> sortedArray, T needle, IComparer<T> comparer)
        {
            int start = 0, end = sortedArray.Length;
            //invariant: only LT nodes before start
            //invariant: only GTE nodes at or past end

            while (end != start) {
                var midpoint = end + start >> 1;
                // start <= midpoint < end
                if (comparer.LessThan(sortedArray[midpoint], needle)) {
                    start = midpoint + 1; //i.e.  midpoint < start1 so start0 < start1
                } else {
                    end = midpoint; //i.e end1 = midpoint so end1 < end0
                }
            }
            return end;
        }

        public static T[] Merge_RemovingDuplicates(T[] aInput, T[] bInput, IComparer<T> comparer)
        {
            if (aInput.Length == 0) {
                return bInput;
            }
            if (bInput.Length == 0) {
                return aInput;
            }
            var a = aInput.AsSpan();
            var b = bInput.AsSpan();
            var outputArr = new T[a.Length + b.Length];
            var writePtr = outputArr.AsSpan();

            while (true) {
                var compare = comparer.Compare(b[0], a[0]);
                if (compare < 0) {
                    writePtr[0] = b[0];
                    writePtr = writePtr[1..];
                    if (b.Length > 1) {
                        b = b[1..];
                    } else {
                        a.CopyTo(writePtr);
                        writePtr = writePtr[a.Length..];
                        break;
                    }
                } else {
                    if (compare > 0) {
                        writePtr[0] = a[0];
                        writePtr = writePtr[1..];
                    }
                    if (a.Length > 1) {
                        a = a[1..];
                    } else {
                        b.CopyTo(writePtr);
                        writePtr = writePtr[b.Length..];
                        break;
                    }
                }
            }

            if (writePtr.Length != 0) {
                Array.Resize(ref outputArr, outputArr.Length - writePtr.Length);
            }
            return outputArr;
        }

        public static SortedArraySet<T> MergeSets(IEnumerable<SortedArraySet<T>> inputSets)
        {
            var inputSetArray = inputSets as IReadOnlyList<SortedArraySet<T>> ?? inputSets.ToArray();
            if (inputSetArray.Count <= 2) {
                return inputSetArray.Count switch {
                    0 => SortedArraySet.Empty<T>(),
                    1 => inputSetArray[0],
                    _ => inputSetArray[0].MergeWith(inputSetArray[1]),
                };
            }
            var outputLen = 0;
            foreach (var inputSet in inputSetArray) {
                outputLen += inputSet.Count;
            }
            var output = new T[outputLen];
            var outputCursor = output.AsSpan();
            foreach (var inputSet in inputSetArray) {
                var input = inputSet.sortedDistinctValues.AsSpan();
                input.CopyTo(outputCursor);
                outputCursor = outputCursor[input.Length..];
            }
            return SortedArraySet.FromMutableUnsortedTmpArray(output, inputSetArray[0].comparer);
        }
    }

    public bool Equals(SortedArraySet<T> other)
    {
        if (ValuesInOrder.Length != other.ValuesInOrder.Length) {
            return false;
        }
        for (var i = 0; i < ValuesInOrder.Length; i++) {
            if (comparer.Compare(ValuesInOrder[i], other.ValuesInOrder[i]) != 0) {
                return false;
            }
        }
        return true;
    }

    public IEnumerator<T> GetEnumerator()
        => ((IEnumerable<T>)ValuesInOrder).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count
        => ValuesInOrder.Length;

    public T this[int index]
        => ValuesInOrder[index];

    public SortedArraySet<T> MergeWith(SortedArraySet<T> b)
        => new(Algorithms.Merge_RemovingDuplicates(ValuesInOrder, b.ValuesInOrder, comparer), comparer);
}
