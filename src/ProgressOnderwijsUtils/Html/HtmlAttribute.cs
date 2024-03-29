namespace ProgressOnderwijsUtils.Html;

public record struct HtmlAttribute(string Name, string Value)
{
    public string Name = Name;
    public string Value = Value;

    public override string ToString()
        => $"{Name}={Value}";
}

public readonly struct HtmlAttributes : IReadOnlyList<HtmlAttribute>
{
    readonly HtmlAttribute[]? attributes;

    HtmlAttributes(HtmlAttribute[] attributes, int count)
    {
        this.attributes = attributes;
        Count = count;
    }

    public int Count { get; }

    public HtmlAttribute this[int i]
        // ReSharper disable once NullableWarningSuppressionIsUsed
        //only null if default, but then count is null... so this *should* crash.
        => attributes![i];

    public string? this[string attrName]
    {
        get {
            if (attrName == "class") {
                var className = default(string);
                foreach (var attr in this) {
                    if (attr.Name == "class") {
                        className = className == null ? attr.Value : $"{className} {attr.Value}";
                    }
                }
                return className;
            } else {
                foreach (var attr in this) {
                    if (attr.Name == attrName) {
                        return attr.Value;
                    }
                }
                return null;
            }
        }
    }

    public bool HasClass(CssClass cssClass)
    {
        //this is essentially Classes copy-pasted except checking for equality instead of accumulating in an array
        //this avoid the allocations for the strings and the array.
        var classChars = cssClass.ClassName.AsSpan();
        foreach (var attr in this) {
            if (attr.Name == "class") {
                var haystack = attr.Value.AsSpan();
                while (haystack.Length > 0) {
                    var endIdx = haystack.IndexOf(' ');

                    ReadOnlySpan<char> head;
                    if (endIdx == -1) {
                        head = haystack;
                        haystack = new();
                    } else {
                        head = haystack[..endIdx];
                        haystack = haystack[(endIdx + 1)..];
                    }
                    if (head.SequenceEqual(classChars) && head.Length > 0) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public string[] Classes()
    {
        var classes = new ArrayBuilder<string>();

        foreach (var attr in this) {
            if (attr.Name == "class") {
                var haystack = attr.Value.AsSpan();
                while (haystack.Length > 0) {
                    var endIdx = haystack.IndexOf(' ');

                    ReadOnlySpan<char> head;
                    if (endIdx == -1) {
                        head = haystack;
                        haystack = new();
                    } else {
                        head = haystack[..endIdx];
                        haystack = haystack[(endIdx + 1)..];
                    }
                    if (head.Length > 0) {
                        classes.Add(head.Length == attr.Value.Length ? attr.Value : new(head));
                    }
                }
            }
        }
        return classes.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HtmlAttributes Add(string name, string val)
    {
        var array = attributes;
        if (array != null) {
            if (Count < array.Length
                // ReSharper disable NullableWarningSuppressionIsUsed
                // when growing the array of structs, semantically non-nullable field "Name" is null.
                // this is the indication that the array member is still uninitialized.
                && Interlocked.CompareExchange(ref array[Count].Name, name, null! /*null is placeholder*/) == null!
                // ReSharper restore NullableWarningSuppressionIsUsed
            ) {
                array[Count].Value = val;
                return new(array, Count + 1);
            } else {
                var oldArray = array;
                array = new HtmlAttribute[Count + 4 + (Count >> 2) & ~1 | 2];
                for (var i = 0; i < Count; i++) {
                    array[i] = oldArray[i];
                }
            }
        } else {
            array = new HtmlAttribute[2];
        }
        array[Count].Name = name;
        array[Count].Value = val;
        return new(array, Count + 1);
    }

    IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => new Enumerator(this);

    public Enumerator GetEnumerator()
        => new(this);

    public struct Enumerator(HtmlAttributes attrs) : IEnumerator<HtmlAttribute>
    {
        readonly HtmlAttribute[] attributes = attrs.attributes ?? [];
        readonly int count = attrs.Count;
        int pos = -1;

        public HtmlAttribute Current
            => attributes[pos];

        object IEnumerator.Current
            => Current;

        public void Dispose() { }

        public bool MoveNext()
            => ++pos < count;

        public void Reset()
            => pos = -1;
    }

    public static HtmlAttributes Empty
        => new();

    public static HtmlAttributes FromArray(HtmlAttribute[] arr)
        => new(arr, arr.Length);

    public override string ToString()
        => this.Select(a => a.ToString()).JoinStrings("; ");
}
