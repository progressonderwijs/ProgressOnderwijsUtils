﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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

        public override string ToString() => Name + "=" + Value;
    }

    public struct HtmlAttributes : IReadOnlyList<HtmlAttribute>
    {
        readonly HtmlAttribute[] attributes;
        readonly int count;

        HtmlAttributes(HtmlAttribute[] attributes, int count)
        {
            this.attributes = attributes;
            this.count = count;
        }

        public int Count => count;
        public HtmlAttribute this[int i] => attributes[i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlAttributes Add(string name, string val)
        {
            var array = attributes;
            if (attributes != null) {
                if (count < array.Length && Interlocked.CompareExchange(ref array[count].Name, name, null) == null) {
                    array[count].Value = val;
                    return new HtmlAttributes(array, count + 1);
                } else {
                    Array.Resize(ref array, count + (count >> 2) + 2);
                }
            } else {
                array = new HtmlAttribute[2];
            }
            array[count].Name = name;
            array[count].Value = val;
            return new HtmlAttributes(array, count + 1);
        }

        IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<HtmlAttribute>
        {
            readonly HtmlAttribute[] attributes;
            readonly int count;
            int pos;

            public Enumerator(HtmlAttributes attrs)
            {
                attributes = attrs.attributes;
                count = attrs.count;
                pos = 0;
            }

            public HtmlAttribute Current => attributes[pos];
            object IEnumerator.Current => attributes[pos];
            public void Dispose() { }
            public bool MoveNext() => ++pos < count;
            public void Reset() => pos = 0;
        }

        public static HtmlAttributes Empty => default(HtmlAttributes);
        public static HtmlAttributes FromArray(HtmlAttribute[] arr) => new HtmlAttributes(arr, arr.Length);
        public override string ToString() => string.Join("; ", this);
    }
}
