using System.Collections.Generic;

namespace ProgressOnderwijsUtils.Html
{
    public static class LimitLengthExtension
    {
        /// <summary>
        /// Takes as fragment and truncates its contents such that the unindented string-representation fits in the
        /// given length
        /// </summary>
        /// <returns>a fragment that, when serialized without doctype, fits in the given number of characters.</returns>
        static (HtmlFragment fragment, int remainingLength) LimitLengthWithStats(HtmlFragment input, int length)
        {
            if (input.Implementation is HtmlFragment[] children) {
                var limitedContents = new List<HtmlFragment>();
                foreach (var child in children) {
                    var (limitedChild, remainingLenth) = LimitLengthWithStats(child, length);
                    if (!limitedChild.IsEmpty) {
                        limitedContents.Add(limitedChild);
                    }
                    length = remainingLenth;
                    if (length == 0) {
                        break;
                    }
                }
                return (limitedContents.AsFragment(), length);
            } else if (input.Implementation is string textContent) {
                var limitedLength = 0;
                while (limitedLength < textContent.Length) {
                    var charlen = HtmlFragment.TextContent(textContent.Substring(limitedLength, 1)).ToStringWithoutDoctype().Length;
                    if (charlen > length) {
                        break;
                    }
                    length -= charlen;
                    limitedLength++;
                }
                return (HtmlFragment.TextContent(textContent.Substring(0, limitedLength)), length);
            } else if (input.Implementation is IHtmlElementAllowingContent tag) {
                var emptyTagLength = tag.ReplaceContentWith(HtmlFragment.Empty).ToStringWithoutDoctype().Length;
                if (emptyTagLength <= length) {
                    length -= emptyTagLength;
                    var (limitedContent, remainingLenth) = LimitLengthWithStats(tag.GetContent(), length);
                    return (tag.ReplaceContentWith(limitedContent).AsFragment(), remainingLenth);
                }
            } else if (input.Implementation is IHtmlElement emptyTag) {
                var emptyTagLength = emptyTag.ToStringWithoutDoctype().Length;
                if (emptyTagLength <= length) {
                    return (input, length - emptyTagLength);
                }
            }
            return (HtmlFragment.Empty, length);
        }

        public static HtmlFragment LimitLength(this HtmlFragment inputHtml, int? length)
            => length.HasValue ? inputHtml.LimitLength(length.Value) : inputHtml;

        public static HtmlFragment LimitLength(this HtmlFragment inputHtml, int length)
            => LimitLengthWithStats(inputHtml, length).fragment;
    }
}
