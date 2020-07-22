using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Linq;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlAttribute
    {
        public string Name, Value;

        public HtmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
            => Name + "=" + Value;
    }

    public readonly struct HtmlAttributes : IReadOnlyList<HtmlAttribute>
    {
        readonly HtmlAttribute[] attributes;
        readonly int count;

        HtmlAttributes(HtmlAttribute[] attributes, int count)
        {
            this.attributes = attributes;
            this.count = count;
        }

        public int Count
            => count;

        public HtmlAttribute this[int i]
            => attributes[i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlAttributes Add(string name, string val)
        {
            var array = attributes;
            if (attributes != null) {
                if (count < array.Length && Interlocked.CompareExchange(ref array[count].Name, name, null! /*null is placeholder*/) == null) {
                    array[count].Value = val;
                    return new HtmlAttributes(array, count + 1);
                } else {
                    var oldArray = array;
                    array = new HtmlAttribute[count + 4 + (count >> 2) & ~1 | 2];
                    for (var i = 0; i < count; i++) {
                        array[i] = oldArray[i];
                    }
                }
            } else {
                array = new HtmlAttribute[2];
            }
            array[count].Name = name;
            array[count].Value = val;
            return new HtmlAttributes(array, count + 1);
        }

        IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this);

        public Enumerator GetEnumerator()
            => new Enumerator(this);

        public struct Enumerator : IEnumerator<HtmlAttribute>
        {
            readonly HtmlAttribute[] attributes;
            readonly int count;
            int pos;

            public Enumerator(HtmlAttributes attrs)
            {
                attributes = attrs.attributes;
                count = attrs.count;
                pos = -1;
            }

            public HtmlAttribute Current
                => attributes[pos];

            object IEnumerator.Current
                => Current;

            public void Dispose() { }

            public bool MoveNext()
                => ++pos < count;

            public void Reset()
                => pos = 0;
        }

        public static HtmlAttributes Empty
            => default;

        public static HtmlAttributes FromArray(HtmlAttribute[] arr)
            => new HtmlAttributes(arr, arr.Length);

        public override string ToString()
            => this.Select(a => a.ToString()).JoinStrings("; ");
    }
}
