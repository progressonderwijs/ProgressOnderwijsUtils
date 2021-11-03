using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Html
{
    public enum TagSafety
    {
        /// <summary>
        /// This tag is black-listed; the entire tag with all its descendants should be excluded from the output.
        /// </summary>
        ShouldRemoveCompletely,

        /// <summary>
        /// This tag is not white-listed; it's contents should be included in the output but the tag itself (and its attributes) should be removed.
        /// </summary>
        ShouldRemoveButKeepContent,

        /// <summary>
        /// This tag is safe even when a hostile hacker controls its contents and white-listed attributes; it can be included in output.
        /// </summary>
        SafeToKeep
    }

    public interface IHtmlFilter
    {
        /// <summary>
        /// Returns whether the tag should be allowed.
        /// </summary>
        /// <param name="elem">The element being checked for security</param>
        /// <returns>The status of the tag.</returns>
        TagSafety AllowTag(IHtmlElement elem);

        /// <summary>
        /// Whether the attribute is safe and should be allowed.
        /// </summary>
        bool AllowAttribute(HtmlAttribute attr);
    }

    public static class HtmlFilters
    {
        public sealed class StripUnsafeStyleTagsFilter : IHtmlFilter
        {
            StripUnsafeStyleTagsFilter() { }
            public static readonly IHtmlFilter Instance = new StripUnsafeStyleTagsFilter();

            public TagSafety AllowTag(IHtmlElement elem)
                => TagSafety.SafeToKeep;

            public bool AllowAttribute(HtmlAttribute attr)
                => !attr.Name.EqualsOrdinalCaseInsensitive("style") || SafeStyleRegex.IsMatch(attr.Value);

            static readonly Regex
                SafeStyleRegex = new Regex(
                    @"^
                    \s*margin(-(left|right|top|bottom))?\s*:\s*
                    \d+(px|em|cm|mm|)\s*;?\s*
                    $",
                    RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                );
        }

        public sealed class StripElementsWithInlineJavascriptOrBadUrisFilter : IHtmlFilter
        {
            StripElementsWithInlineJavascriptOrBadUrisFilter() { }
            public static readonly IHtmlFilter Instance = new StripElementsWithInlineJavascriptOrBadUrisFilter();

            public TagSafety AllowTag(IHtmlElement elem)
            {
                foreach (var attr in elem.Attributes) {
                    if (attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)) {
                        return TagSafety.ShouldRemoveButKeepContent;
                    }
                    if ((attr.Name.EqualsOrdinalCaseInsensitive("href") || attr.Name.EqualsOrdinalCaseInsensitive("src"))
                        && !attr.Value.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase)
                        && !attr.Value.StartsWith("https:", StringComparison.InvariantCultureIgnoreCase)) {
                        return TagSafety.ShouldRemoveButKeepContent;
                    }
                }
                return TagSafety.SafeToKeep;
            }

            public bool AllowAttribute(HtmlAttribute attr)
                => true;
        }

        public sealed class PickMostRestrictiveFilter : IHtmlFilter
        {
            readonly IHtmlFilter[] filters;

            public PickMostRestrictiveFilter(params IHtmlFilter[] filters)
                => this.filters = filters;

            public TagSafety AllowTag(IHtmlElement elem)
            {
                var retval = TagSafety.SafeToKeep;
                foreach (var filter in filters) {
                    retval = MostRestrictiveFilter(retval, filter.AllowTag(elem));
                }
                return retval;
            }

            static TagSafety MostRestrictiveFilter(TagSafety retval, TagSafety tagSafety)
                => tagSafety < retval ? tagSafety : retval;

            public bool AllowAttribute(HtmlAttribute attr)
            {
                var retval = true;
                foreach (var filter in filters) {
                    retval = retval && filter.AllowAttribute(attr);
                }
                return retval;
            }
        }

        public sealed class SetBasedHtmlFilter : IHtmlFilter
        {
            readonly HashSet<string> bannedElements, safeElements, safeAttributes;

            public SetBasedHtmlFilter(IEnumerable<string>? bannedElements, IEnumerable<string>? safeElements, IEnumerable<string>? safeAttributes)
            {
                this.bannedElements = MkSet(bannedElements);
                this.safeElements = MkSet(safeElements);
                this.safeAttributes = MkSet(safeAttributes);
            }

            static HashSet<string> MkSet(IEnumerable<string>? elems)
                => new HashSet<string>(elems ?? new string[0], StringComparer.OrdinalIgnoreCase);

            public TagSafety AllowTag(IHtmlElement elem)
                => bannedElements.Contains(elem.TagName)
                    ? TagSafety.ShouldRemoveCompletely
                    : safeElements.Contains(elem.TagName)
                        ? TagSafety.SafeToKeep
                        : TagSafety.ShouldRemoveButKeepContent;

            public bool AllowAttribute(HtmlAttribute attr)
                => safeAttributes.Contains(attr.Name);
        }

        static readonly string[]
            banned = "script style".Split(' '),
            safe = Regex.Split(
                @"a abbr acronym b big blockquote br caption center
                cite code col colgroup dd del dfn div dl dt em fieldset font h1 h2
                h3 h4 h5 h6 hr i img ins legend li ol p pre q samp small span strong
                sub sup table tbody td tfoot th thead tr tt u ul var",
                @"\s+"
            ),
            //TODO: we should allow html5 stuff too: article aside details figcaption figure footer header main mark nav section summary time bdi meter progress ruby rp rt wbr
            safeAttr = Regex.Split(
                @"align alt bgcolor border cellpadding cellspacing
                color colspan dir face height href lang rowspan size
                style title width",
                @"\s+"
            );

        //om tracer elements te vermijden zijn is img wel maar attribuut src niet toegestaan Bovendien kan src="javascript:..." dus src mag echt niet! Om geen form-problemen te hebben mogen form elementen niet.
        public static readonly IHtmlFilter Default = new PickMostRestrictiveFilter(StripUnsafeStyleTagsFilter.Instance, StripElementsWithInlineJavascriptOrBadUrisFilter.Instance, new SetBasedHtmlFilter(banned, safe, safeAttr));
    }

    public static class HtmlFragmentSanitizeExtension
    {
        static HtmlFragment FilterElem(HtmlFragment node, IHtmlFilter filter)
        {
            if (node.Implementation is string) {
                return node;
            }
            if (node.Implementation is HtmlFragment[] fragments) {
                return fragments.ArraySelect(child => FilterElem(child, filter)).AsFragment(); //don't copy comments, processing instructions, etc.
            }
            if (node.Implementation is IHtmlElement elem) {
                var safety = filter.AllowTag(elem);
                var elemChildren = elem.Contents().NodesOfFragment();
                var safeChildren = elemChildren.Select(childnode => FilterElem(childnode, filter)).Where(frag => !frag.IsEmpty).ToArray();
                if (safety == TagSafety.ShouldRemoveButKeepContent) {
                    return safeChildren.AsFragment();
                }
                if (safety == TagSafety.SafeToKeep) {
                    return elem.ReplaceAttributesAndContents(
                        HtmlAttributes.FromArray(elem.Attributes.Where(filter.AllowAttribute).ToArray()),
                        safeChildren
                    ).AsFragment();
                }
            }

            return HtmlFragment.Empty; //unsafe or empty
        }

        /// <summary>This function sanitizes an html tree.  By default, it uses the filter HtmlFilters.Default, but you might consider constructing different filters for different cases. </summary>
        public static HtmlFragment Sanitize(this HtmlFragment sourceHtml)
            => sourceHtml.Sanitize(null);

        /// <summary>This function sanitizes an html tree.  By default, it uses the filter HtmlFilters.Default, but you might consider constructing different filters for different cases. </summary>
        public static HtmlFragment Sanitize(this HtmlFragment sourceHtml, IHtmlFilter? filter)
            => FilterElem(sourceHtml, filter ?? HtmlFilters.Default);
    }
}
