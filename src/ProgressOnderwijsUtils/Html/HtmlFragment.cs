using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlFragment : IConvertibleToFragment
    {
        /// <summary>
        /// Either a string, an IHtmlElement, a non-empty HtmlFragment[], or null (the empty fragment).
        /// </summary>
        public readonly object Implementation;

        public bool IsTextContent()
            => Implementation is string;

        public bool IsTextContent(out string textContent)
            => (textContent = Implementation as string) != null;

        public bool IsElement()
            => Implementation is IHtmlElement;

        public bool IsElement(out IHtmlElement element)
            => (element = Implementation as IHtmlElement) != null;

        public bool IsElementAllowingContent()
            => Implementation is IHtmlElementAllowingContent;

        public bool IsElementAllowingContent(out IHtmlElementAllowingContent element)
            => (element = Implementation as IHtmlElementAllowingContent) != null;

        public bool IsMultipleNodes()
            => Implementation is HtmlFragment[];

        public bool IsMultipleNodes(out HtmlFragment[] nodes)
            => (nodes = Implementation as HtmlFragment[]) != null;

        /// <summary>
        /// Sets at most one of the out parameters to a non-null value.
        /// </summary>
        public void Deconstruct(out string textContent, out IHtmlElement element, out HtmlFragment[] nodes)
        {
            textContent = Implementation as string;
            element = Implementation as IHtmlElement;
            nodes = Implementation as HtmlFragment[];
        }

        public bool IsEmpty
            => Implementation == null;

        HtmlFragment(object content)
            => Implementation = content;

        [Pure]
        public static HtmlFragment TextContent(string textContent)
            => new HtmlFragment(textContent);

        [Pure]
        public static HtmlFragment Element(IHtmlElement element)
            => new HtmlFragment(element);

        [Pure]
        public static HtmlFragment Element(CustomHtmlElement element)
            => new HtmlFragment(element.Canonicalize());

        [Pure]
        public static HtmlFragment Element(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            => Element(new CustomHtmlElement(tagName, attributes, childNodes));

        [Pure]
        public static HtmlFragment Fragment()
            => Empty;

        [Pure]
        public static HtmlFragment Fragment(HtmlFragment htmlEl)
            => htmlEl;

        [Pure]
        public static HtmlFragment Fragment(HtmlFragment a, HtmlFragment b)
        {
            if (a.IsEmpty) {
                //optimize very common case, when appending to empty.
                return b;
            }
            var kidCounter = new KidCounter();
            kidCounter.CountForKid(a);
            kidCounter.CountForKid(b);
            if (!kidCounter.ShouldUseCollector()) {
                return new HtmlFragment(new[] { a, b });
            }
            var collector = kidCounter.MakeCollector();
            collector.AddKid(a);
            collector.AddKid(b);
            Debug.Assert(collector.IsFull());
            return new HtmlFragment(collector.retval);
        }

        [Pure]
        public static HtmlFragment Fragment([CanBeNull] params HtmlFragment[] htmlEls)
        {
            if (htmlEls == null || htmlEls.Length == 0) {
                return Empty;
            }
            if (htmlEls.Length == 1) {
                return htmlEls[0];
            }
            if (htmlEls.Length < 16) {
                var kidCounter = new KidCounter();
                foreach (var child in htmlEls) {
                    kidCounter.CountForKid(child);
                }
                if (kidCounter.ShouldUseCollector()) {
                    var collector = kidCounter.MakeCollector();
                    foreach (var child in htmlEls) {
                        collector.AddKid(child);
                    }
                    Debug.Assert(collector.IsFull());
                    return new HtmlFragment(collector.retval);
                }
            }

            return new HtmlFragment(htmlEls);
        }

        struct KidCounter
        {
            int totalKids;
            bool flattenRelevant;

            public void CountForKid(HtmlFragment child)
            {
                if (child.Implementation is HtmlFragment[] childContents) {
                    totalKids += childContents.Length;
                    flattenRelevant = true;
                } else if (child.IsEmpty) {
                    flattenRelevant = true;
                } else {
                    totalKids++;
                }
            }

            public bool ShouldUseCollector() => flattenRelevant && totalKids < 64;
            public KidCollector MakeCollector() => new KidCollector(totalKids);
        }

        struct KidCollector
        {
            public readonly HtmlFragment[] retval;
            public int writeIdx;

            public KidCollector(int totalKids)
            {
                retval = new HtmlFragment[totalKids];
                writeIdx = 0;
            }

            public void AddKid(HtmlFragment child)
            {
                if (child.Implementation is HtmlFragment[] childContents) {
                    foreach (var grandChild in childContents) {
                        retval[writeIdx++] = grandChild;
                    }
                } else if (!child.IsEmpty) {
                    retval[writeIdx++] = child;
                }
            }

            public bool IsFull() => writeIdx == retval.Length;
        }

        [Pure]
        public static HtmlFragment Fragment<T>([NotNull] IEnumerable<T> htmlEls)
            where T : IConvertibleToFragment
        {
            var retval = new ArrayBuilder<HtmlFragment>();
            foreach (var el in htmlEls) {
                var htmlFragment = el.AsFragment();
                if (htmlFragment.Implementation is HtmlFragment[] kids && kids.Length < 64) {
                    foreach (var grandchild in kids) {
                        retval.Add(grandchild);
                    }
                } else if (!htmlFragment.IsEmpty) {
                    retval.Add(htmlFragment);
                }
            }
            return new HtmlFragment(retval.ToArray());
        }

        public override string ToString()
            => "HtmlFragment: " + this.SerializeToStringWithoutDoctype();

        public static HtmlFragment Empty
            => default(HtmlFragment);

        public static implicit operator HtmlFragment(CustomHtmlElement element)
            => Element(element);

        public static implicit operator HtmlFragment(string textContent)
            => TextContent(textContent);

        public static implicit operator HtmlFragment(HtmlFragment[] fragments)
            => Fragment(fragments);

        public HtmlFragment Append(HtmlFragment tail)
            => Fragment(this, tail);

        public HtmlFragment Append(params HtmlFragment[] longTail)
            => Fragment(this, Fragment(longTail));

        public static HtmlFragment operator +(HtmlFragment left, HtmlFragment right)
            => left.Append(right);

        [Pure]
        HtmlFragment IConvertibleToFragment.AsFragment()
            => this;

        public HtmlFragment[] NodesOfFragment()
            => Implementation as HtmlFragment[] ?? (IsEmpty ? EmptyNodes : new[] { this });

        public HtmlFragment[] ChildNodes()
            => Implementation is IHtmlElementAllowingContent elem
                ? elem.Contents().NodesOfFragment()
                : Implementation as HtmlFragment[] ?? EmptyNodes;

        public static HtmlFragment[] EmptyNodes
            => Array.Empty<HtmlFragment>();
    }
}
