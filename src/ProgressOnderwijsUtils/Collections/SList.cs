namespace ProgressOnderwijsUtils.Collections;

/// <summary>
/// Immutable, thread-safe, singly-linked list
/// </summary>
public readonly struct SList<T> : IEnumerable<T>, IEquatable<SList<T>>
{
    sealed record Impl(T Head, Impl? Tail);

    SList(Impl? list)
        => this.list = list;

    public SList(T head, SList<T> tail)
        : this(new(head, tail.list)) { }

    readonly Impl? list;

    public static SList<T> Empty
        => new();

    public bool IsEmpty
        => list == null;

    public T Head
        => list.AssertNotNull().Head;

    public bool TryGet([MaybeNullWhen(false)] out T head, out SList<T> tail)
    {
        if (list == null) {
            head = default(T);
            tail = Empty;
            return false;
        } else {
            head = list.Head;
            tail = new(list.Tail);
            return true;
        }
    }

    public SList<T> Tail
        => new(list.AssertNotNull().Tail);

    static readonly int typeHash = typeof(T).GetHashCode();
    static readonly IEqualityComparer<T> elemEquality = EqualityComparer<T>.Default;

    [Pure]
    public bool Equals(SList<T> other)
    {
        var alist = list;
        var blist = other.list;
        while (alist != null && blist != null) {
            if (ReferenceEquals(alist, blist)) {
                return true;
            }
            if (!elemEquality.Equals(alist.Head, blist.Head)) {
                return false;
            }
            alist = alist.Tail;
            blist = blist.Tail;
        }
        return alist == null && blist == null;
    }

    [Pure]
    public override bool Equals(object? obj)
        => obj is SList<T> other && Equals(other);

    [Pure]
    public override int GetHashCode()
    {
        var hash = (ulong)(typeHash + 1);
        for (var current = list; current != null; current = current.Tail) {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            hash = hash * 137ul + (current.Head == null ? 0 : (ulong)elemEquality.GetHashCode(current.Head));
        }
        return (int)hash ^ (int)(hash >> 32);
    }

    [Pure]
    public static bool operator ==(SList<T> a, SList<T> b)
        => a.Equals(b);

    [Pure]
    public static bool operator !=(SList<T> a, SList<T> b)
        => !a.Equals(b);

    public IEnumerable<SList<T>> NonEmptySuffixes
    {
        get {
            for (var current = this; !current.IsEmpty; current = current.Tail) {
                yield return current;
            }
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        for (var current = list; current != null; current = current.Tail) {
            yield return current.Head;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        // ReSharper disable once NotDisposedResourceIsReturned
        => ((IEnumerable<T>)this).GetEnumerator();
}

public static class SList
{
    [Pure]
    public static SList<T> Prepend<T>(this SList<T> self, T head)
        => new(head, self);

    [Pure]
    [UsefulToKeep("library method")]
    public static SList<T> Prepend<T>(this SList<T> self, SList<T> heads)
    {
        var retval = self;
        for (var cur = heads.Reverse(); !cur.IsEmpty; cur = cur.Tail) {
            retval = retval.Prepend(cur.Head);
        }
        return retval;
    }

    [Pure]
    public static SList<T> Reverse<T>(this SList<T> self)
    {
        var retval = default(SList<T>);
        for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
            retval = retval.Prepend(cur.Head);
        }
        return retval;
    }

    [Pure]
    public static SList<TR> SelectEager<T, TR>(this SList<T> self, Func<T, TR> map)
        => self.SelectReverse(map).Reverse();

    [Pure]
    public static SList<TR> SelectReverse<T, TR>(this SList<T> self, Func<T, TR> map)
    {
        var retval = default(SList<TR>);
        for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
            retval = retval.Prepend(map(cur.Head));
        }
        return retval;
    }

    [Pure]
    [UsefulToKeep("library method")]
    public static SList<T> WhereReverse<T>(this SList<T> self, Func<T, bool> filter)
    {
        var retval = default(SList<T>);
        for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
            if (filter(cur.Head)) {
                retval = retval.Prepend(cur.Head);
            }
        }
        return retval;
    }

    [Pure]
    public static SList<T> Skip<T>(this SList<T> self, int count)
    {
        var retval = self;
        for (var i = 0; !retval.IsEmpty && i < count; i++) {
            retval = retval.Tail;
        }
        return retval;
    }

    [Pure]
    public static SList<T> Create<T>(IEnumerable<T> enumerable)
    {
        if (enumerable is IList<T> list) {
            return Create(list); //use IList interface for reverse iterability
        } else {
            return Create(enumerable.ToArray()); //can't help but iterate forwards, so at least stick to it with the fastest possible path.
        }
    }

    [Pure]
    public static SList<T> Create<T>(IList<T> list)
    {
        if (list is T[] array) {
            return Create(array);
        }
        var retval = default(SList<T>);
        for (var i = list.Count - 1; i >= 0; i--) {
            retval = retval.Prepend(list[i]);
        }
        return retval;
    }

    [Pure]
    public static SList<T> Create<T>(T[] list)
    {
        var retval = default(SList<T>);
        for (var i = list.Length - 1; i >= 0; i--) {
            retval = retval.Prepend(list[i]);
        }
        return retval;
    }

    [Pure]
    public static SList<T> SingleElement<T>(T element)
        => new(element, new());

    [UsefulToKeep("library method")]
    public static SList<T> Empty<T>()
        => new();
}
