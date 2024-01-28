namespace ProgressOnderwijsUtils.Collections;

public interface IOrdering<in T>
{
    bool LessThan(T a, T b);
}

public readonly struct SortedSet<T, TOrder> : IEquatable<SortedSet<T, TOrder>>, IReadOnlyList<T>
    where TOrder : struct, IOrdering<T>
{
    readonly T[]? sortedDistinctValues;
    static readonly T[] empty = [];

    static TOrder Ordering
        => new();

    SortedSet(T[] sortedDistinctValues)
        => this.sortedDistinctValues = sortedDistinctValues;

    public static SortedSet<T, TOrder> Empty
        => new([]);

    public static SortedSet<T, TOrder> FromValues(IEnumerable<T> values)
        => FromValues(values as T[] ?? values.ToArray());

    public static SortedSet<T, TOrder> FromValues(T[] values)
    {
        for (var i = 1; i < values.Length; i++) {
            if (!Ordering.LessThan(values[i - 1], values[i])) {
                return FromMutableUnsortedTmpArray(values.ToArray());
            }
        }
        return new(values);
    }

    static SortedSet<T, TOrder> FromMutableUnsortedTmpArray(T[] privateMutableArray)
    {
        Array.Sort(privateMutableArray, 0, privateMutableArray.Length);
        var newLength = Algorithms.CountAndMoveDistinctValuesToFront(privateMutableArray, privateMutableArray.Length);
        Array.Resize(ref privateMutableArray, newLength);
        return new(privateMutableArray);
    }

    public T[] ValuesInOrder
        => sortedDistinctValues ?? empty;

    [Pure]
    public bool Contains(T value)
        => sortedDistinctValues is not null
            && Algorithms.IdxAfterLastLtNode(sortedDistinctValues, value) is var idxAfterLastLtNode
            && idxAfterLastLtNode < sortedDistinctValues.Length
            && !Ordering.LessThan(value, sortedDistinctValues[idxAfterLastLtNode]);

    [Pure]
    public bool IsSubsetOf(SortedSet<T, TOrder> potentialSuperset)
        => IsSubset_OfLargeSuperset_Recursive(sortedDistinctValues, potentialSuperset.sortedDistinctValues);

    static bool IsSubset_OfLargeSuperset_Recursive(ReadOnlySpan<T> sortedSubSet, ReadOnlySpan<T> sortedSuperSet)
    {
        if (sortedSubSet.Length == 0) {
            return true;
        }
        if ((sortedSuperSet.Length >> 1) - 9 <= sortedSubSet.Length) {
            return IsSubset_Scan(sortedSubSet, sortedSuperSet);
        }
        var midIdx = sortedSubSet.Length >> 1;
        var value = sortedSubSet[midIdx];
        var idxAfterLastLtNode = Algorithms.IdxAfterLastLtNode(sortedSuperSet, value);
        if (idxAfterLastLtNode >= sortedSuperSet.Length || Ordering.LessThan(value, sortedSuperSet[idxAfterLastLtNode])) {
            return false;
        }
        return IsSubset_OfLargeSuperset_Recursive(sortedSubSet[..midIdx], sortedSuperSet[..idxAfterLastLtNode])
            && IsSubset_OfLargeSuperset_Recursive(sortedSubSet[(midIdx + 1)..], sortedSuperSet[(idxAfterLastLtNode + 1)..]);
    }

    static bool IsSubset_Scan(ReadOnlySpan<T> sortedSubSet, ReadOnlySpan<T> sortedSuperSet)
    {
        var subValue = sortedSubSet[0];
        foreach (var supValue in sortedSuperSet) {
            if (!Ordering.LessThan(supValue, subValue)) {
                if (!Ordering.LessThan(subValue, supValue)) {
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
        public static int CountAndMoveDistinctValuesToFront(T[] arr, int len)
        {
            if (len < 2) {
                return len;
            }
            var distinctUptoIdx = 0;
            var readIdx = 1;
            do {
                if (Ordering.LessThan(arr[distinctUptoIdx], arr[readIdx])) {
                    distinctUptoIdx++;
                    arr[distinctUptoIdx] = arr[readIdx];
                }
                readIdx++;
            } while (readIdx < len);
            return distinctUptoIdx + 1;
        }

        public static int IdxAfterLastLtNode(ReadOnlySpan<T> sortedArray, T needle)
        {
            int start = 0, end = sortedArray.Length;
            //invariant: only LT nodes before start
            //invariant: only GTE nodes at or past end

            while (end != start) {
                var midpoint = end + start >> 1;
                // start <= midpoint < end
                if (Ordering.LessThan(sortedArray[midpoint], needle)) {
                    start = midpoint + 1; //i.e.  midpoint < start1 so start0 < start1
                } else {
                    end = midpoint; //i.e end1 = midpoint so end1 < end0
                }
            }
            return end;
        }

        public static T[] Merge_RemovingDuplicates(T[] aInput, T[] bInput)
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
                if (Ordering.LessThan(b[0], a[0])) {
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
                    if (Ordering.LessThan(a[0], b[0])) {
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

        public static SortedSet<T, TOrder> MergeSets(IEnumerable<SortedSet<T, TOrder>> inputSets)
        {
            var inputSetArray = inputSets as IReadOnlyList<SortedSet<T, TOrder>> ?? inputSets.ToArray();
            if (inputSetArray.Count <= 2) {
                return inputSetArray.Count switch {
                    0 => Empty,
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
            return FromMutableUnsortedTmpArray(output);
        }
    }

    public bool Equals(SortedSet<T, TOrder> other)
    {
        if (ValuesInOrder.Length != other.ValuesInOrder.Length) {
            return false;
        }
        for (var i = 0; i < ValuesInOrder.Length; i++) {
            if (Ordering.LessThan(ValuesInOrder[i], other.ValuesInOrder[i]) || Ordering.LessThan(other.ValuesInOrder[i], ValuesInOrder[i])) {
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

    public SortedSet<T, TOrder> MergeWith(SortedSet<T, TOrder> b)
        => new(Algorithms.Merge_RemovingDuplicates(ValuesInOrder, b.ValuesInOrder));
}

public static class SortedSetHelpers
{
    public static SortedSet<T, TOrder> MergeSets<T, TOrder>(this IEnumerable<SortedSet<T, TOrder>> inputSets)
        where TOrder : struct, IOrdering<T>
        => SortedSet<T, TOrder>.Algorithms.MergeSets(inputSets);
}
