using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlFragment : IConvertibleToFragment
    {
        /// <summary>
        /// Either a string, an IHtmlTag, a non-empty HtmlFragment[], or null (the empty fragment).
        /// </summary>
        public readonly object Content;

        public bool IsTextContent => Content is string;
        public bool IsHtmlElement => Content is IHtmlTag;
        public bool IsCollectionOfFragments => Content is HtmlFragment[];
        public bool IsEmpty => Content == null;
        HtmlFragment(object content) => Content = content;

        [Pure]
        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent);

        [Pure]
        public static HtmlFragment HtmlElement(IHtmlTag element) => new HtmlFragment(element);

        [Pure]
        public static HtmlFragment HtmlElement(CustomHtmlElement element) => new HtmlFragment(element.Canonicalize());

        [Pure]
        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes) => HtmlElement(new CustomHtmlElement(tagName, attributes, childNodes));

        [Pure]
        public static HtmlFragment Fragment() => Empty;

        [Pure]
        public static HtmlFragment Fragment(HtmlFragment htmlEl) => htmlEl;

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
                var totalKids = 0;
                var flattenRelevant = false;
                foreach (var child in htmlEls) {
                    if (child.Content is HtmlFragment[] childContents) {
                        totalKids += childContents.Length;
                        flattenRelevant = true;
                    } else if (child.IsEmpty) {
                        flattenRelevant = true;
                    } else {
                        totalKids++;
                    }
                }
                if (flattenRelevant && totalKids < 64) {
                    var retval = new HtmlFragment[totalKids];
                    var writeIdx = 0;
                    foreach (var child in htmlEls) {
                        if (child.Content is HtmlFragment[] childContents) {
                            foreach (var grandChild in childContents) {
                                retval[writeIdx++] = grandChild;
                            }
                        } else if (!child.IsEmpty) {
                            retval[writeIdx++] = child;
                        }
                    }
                    Debug.Assert(writeIdx == totalKids);
                    return new HtmlFragment(retval);
                }
            }

            return new HtmlFragment(htmlEls);
        }

        [Pure]
        public static HtmlFragment Fragment<T>([NotNull] IEnumerable<T> htmlEls)
            where T : IConvertibleToFragment
        {
            var retval = FastArrayBuilder<HtmlFragment>.Create();
            foreach (var el in htmlEls) {
                var htmlFragment = el.AsFragment();
                if (htmlFragment.Content is HtmlFragment[] kids && kids.Length < 64) {
                    foreach (var grandchild in kids) {
                        retval.Add(grandchild);
                    }
                } else if (!htmlFragment.IsEmpty) {
                    retval.Add(htmlFragment);
                }
            }
            return new HtmlFragment(retval.ToArray());
        }

        public override string ToString() => "HtmlFragment: " + this.SerializeToStringWithoutDoctype();
        public static HtmlFragment Empty => default(HtmlFragment);
        public static implicit operator HtmlFragment(CustomHtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);
        public HtmlFragment Append(HtmlFragment tail) => Fragment(this, tail);
        public HtmlFragment Append(HtmlFragment tail, params HtmlFragment[] longTail) => Fragment(Fragment(this, tail), Fragment(longTail));
        public static HtmlFragment operator +(HtmlFragment left, HtmlFragment right) => left.Append(right);

        [Pure]
        HtmlFragment IConvertibleToFragment.AsFragment() => this;
    }
}
