using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
    public enum TagSafety
    {
        /// <summary>
        /// This tag is black-listed; the entire tag with all its descendants should be excluded from the output.
        /// </summary>
        Unsafe,

        /// <summary>
        /// This tag is not white-listed; it's contents should be included in the output but the tag itself (and its attributes) should be removed.
        /// </summary>
        Unknown,

        /// <summary>
        /// This tag is safe even when a hostile hacker controls its contents and white-listed attributes; it can be included in output.
        /// </summary>
        Safe
    }

    public interface IHtmlFilter
    {
        /// <summary>
        /// Returns whether the tag should be allowed.
        /// </summary>
        /// <param name="elem">The element being checked for security</param>
        /// <returns>The status of the tag.</returns>
        TagSafety AllowTag(XElement elem);

        /// <summary>
        /// Whether the attribute is secure and should be allowed.
        /// </summary>
        bool AllowAttribute(XAttribute attr);
    }

    public static class HtmlFilter
    {
        sealed class DelegateHtmlFilter : IHtmlFilter
        {
            readonly Func<XElement, TagSafety> filterTag;
            readonly Func<XAttribute, bool> filterAttr;

            public DelegateHtmlFilter(Func<XElement, TagSafety> filterTag, Func<XAttribute, bool> filterAttr)
            {
                this.filterTag = filterTag;
                this.filterAttr = filterAttr;
            }

            public TagSafety AllowTag(XElement elem) => filterTag(elem);
            public bool AllowAttribute(XAttribute attr) => filterAttr(attr);
        }

        sealed class SafeStyleFilter : IHtmlFilter
        {
            public static readonly SafeStyleFilter Instance = new SafeStyleFilter();
            public TagSafety AllowTag(XElement elem) => TagSafety.Unsafe;
            public bool AllowAttribute(XAttribute attr) => IsSafeStyleAttribute(attr);

            static readonly Regex
                SafeStyleRegex = new Regex(@"^
	\s*margin(-(left|right|top|bottom))?	\s*:\s*
		\d+(px|em|cm|mm|)\s*;?\s*
$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            static bool IsSafeStyleAttribute(XAttribute attr) => attr.Name.LocalName == "style" && SafeStyleRegex.IsMatch(attr.Value);
        }

        sealed class AllowWhenAny : IHtmlFilter
        {
            readonly IHtmlFilter[] filters;
            public AllowWhenAny(params IHtmlFilter[] filters) { this.filters = filters ?? new IHtmlFilter[0]; }

            public TagSafety AllowTag(XElement elem)
            {
                var retval = TagSafety.Unsafe;
                foreach (var filter in filters) {
                    var tagSafety = filter.AllowTag(elem);
                    if (tagSafety == TagSafety.Safe) {
                        return TagSafety.Safe;
                    } else if (tagSafety > retval) {
                        retval = tagSafety;
                    }
                }
                return retval;
            }

            public bool AllowAttribute(XAttribute attr) => filters.Any(filter => filter.AllowAttribute(attr));
        }

        sealed class SetBasedHtmlFilter : IHtmlFilter
        {
            readonly HashSet<string> bannedElements, safeElements, safeAttributes;

            public SetBasedHtmlFilter(IEnumerable<string> bannedElements, IEnumerable<string> safeElements, IEnumerable<string> safeAttributes)
            {
                this.bannedElements = MkSet(bannedElements);
                this.safeElements = MkSet(safeElements);
                this.safeAttributes = MkSet(safeAttributes);
            }

            static HashSet<string> MkSet(IEnumerable<string> elems) => new HashSet<string>(elems ?? new string[0], StringComparer.OrdinalIgnoreCase);

            public TagSafety AllowTag(XElement elem)
            {
                return bannedElements.Contains(elem.Name.LocalName)
                    ? TagSafety.Unsafe
                    : safeElements.Contains(elem.Name.LocalName)
                        ? TagSafety.Safe
                        : TagSafety.Unknown;
            }

            public bool AllowAttribute(XAttribute attr) => safeAttributes.Contains(attr.Name.LocalName);
        }

        static readonly string[]
            banned = "script style".Split(' '),
            safe =
                "b i u big small em strong hr br p span div center font table thead col colgroup tbody tfoot caption tr td th h1 h2 h3 h4 h5 h6 a cite dfn code samp var dl dt dd ins del sub sup tt ul ol li pre q abbr acronym blockquote fieldset legend img"
                    .Split(' '),
            safeAttr = "lang title href dir color border face size align alt bgcolor cellspacing cellpadding char charoff cite height width colspan rowspan".Split(' ');

        static readonly SetBasedHtmlFilter defaultSets = new SetBasedHtmlFilter(banned, safe, safeAttr);

        //om tracer elements te vermijden zijn is img wel maar attribuut src niet toegestaan Bovendien kan src="javascript:..." dus src mag echt niet! Om geen form-problemen te hebben mogen form elementen niet.
        public static readonly IHtmlFilter
            Default = new AllowWhenAny(defaultSets, SafeStyleFilter.Instance),
            InternalNameAllowed =
                new AllowWhenAny(new SetBasedHtmlFilter(banned, safe, safeAttr.Concat(new[] { "name" })), SafeStyleFilter.Instance)
            ;
    }

    //TODO: we should implement a real wrapper around html tidy sometime to support invalid but almost valid XHTML - but until we do, we'll just need to fail on
    //invalid html.
    public static class XhtmlCleaner
    {
        static readonly Regex xmlStripRegex = new Regex(
            @"(<\/?[A-Za-z_][^<]*>)",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        static readonly Regex symbolRegex = new Regex(
            @"\<|\>|\&(?![A-Za-z][A-Za-z]+;)",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        static readonly Regex entityRegex = new Regex(
            @"\&[A-Za-z][A-Za-z]+;",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// Attempts to parse an html fragment as xml.  This works because our html is generally well formed xml.
        /// When parsing fails, the method returns a best-guess string representation which tries to decode entities into unicode.
        /// 
        /// This is not a security check; the output of this function can still contain dangerous tags!
        /// 
        /// Use Sanitze() to clean up parsed html.
        /// </summary>
        /// <returns>The html fragment.</returns> 
        public static XhtmlData HeuristicParse(string str)
        {
            XhtmlData? result = TryParse(str);
            if (result != null) {
                return result.Value;
            }

            // decoding failed; the string is invalid.  To get at least something readable, 
            // we'll manually strip something that looks like tags
            str = xmlStripRegex.Replace(str, ""); //strip everything looking like tags
            str = symbolRegex.Replace(str, match => new XText(match.Value).ToString());
            //and then replace odd (i.e. non-conforming) symbols by their encoded representations...

            return TryParse(str) //then return markup without tags
                ?? XhtmlData.Create(str); //if all else fails, return raw markup safely encoded as content.
        }

        public static XhtmlData? TryParse(string str)
        {
            try {
                var strWithoutNonXmlEntities = StrWithoutNonXmlEntities(str);

                return XhtmlData.Create(XElement.Parse("<x>" + strWithoutNonXmlEntities + "</x>", LoadOptions.PreserveWhitespace).Nodes());
            } catch (XmlException) {
                return null;
            }
        }

        public static XDocument ParseCompleteXhtmlDocument(string str)
        {
            var strWithoutNonXmlEntities = StrWithoutNonXmlEntities(str);

            return XDocument.Parse(strWithoutNonXmlEntities, LoadOptions.PreserveWhitespace);
        }

        static string StrWithoutNonXmlEntities(string str)
        {
            string strWithoutNonXmlEntities =
                entityRegex.Replace(
                    str,
                    match => {
                        string entityReference = match.Value.Substring(1, match.Length - 2);

                        char? refersTo =
                            entityReference == "lt" || entityReference == "gt" || entityReference == "amp" || entityReference == "apos" || entityReference == "quot"
                                ? default(char?) //no need to decode xml entities and to do so might interpret real content as markup accidentally...
                                : HtmlEntityLookup.Lookup(entityReference);
                        return refersTo.HasValue ? new string(refersTo.Value, 1) : match.Value;
                    });
            return strWithoutNonXmlEntities;
        }

        /// <summary>
        /// Takes an xml element and truncates its contents such that the unindented string-representation fits in the 
        /// given length
        /// </summary>
        /// <param name="input">The Xml to truncate</param>
        /// <param name="length">The maximum length</param>
        /// <returns>an xml segment that, if serialized without extra spacing, fits in the given length.</returns>
        static XElement LimitLength(XElement input, int length)
        {
            var output = new XElement(input);
            XNode current = output;
            int currentMax = length;
            var currentLen = current.ToString(SaveOptions.DisableFormatting).Length;
            //assume that xml length is additive; i.e. that adding a child adds precisely as much length as the child itself is long.
            //in debug mode, this assumption is asserted; but in release mode, this assumption avoids quadratic complexity
            while (true) {
                if (currentLen <= currentMax) {
                    return output;
                } else if (current is XComment
                    || current is XElement && ((XElement)current).IsEmpty
                    || current is XDocumentType
                    || current is XProcessingInstruction) {
                    current.Remove();
                    return output;
                } else if (current is XText) {
                    var text = current as XText;
                    //since XText does some \r magic, we can't just assume current.ToString is equivalent to text.Value
                    text.Value = current.ToString(SaveOptions.DisableFormatting).Replace('\r', ' ').Substring(0, currentMax);
                    return output;
                }
                var currentEl = (XElement)current;

                int lastKidLen = currentEl.LastNode.ToString(SaveOptions.DisableFormatting).Length;
                if (currentLen - lastKidLen > currentMax) {
                    currentEl.LastNode.Remove();
                    currentLen = !currentEl.IsEmpty ? currentLen - lastKidLen : current.ToString(SaveOptions.DisableFormatting).Length;
                    Debug.Assert(currentLen == current.ToString(SaveOptions.DisableFormatting).Length, "Current length is inconsistent!");
                } else {
                    int restLen = currentLen - lastKidLen;
                    int lastKidMax = currentMax - restLen;
                    current = currentEl.LastNode;
                    currentLen = lastKidLen;
                    Debug.Assert(currentLen == current.ToString(SaveOptions.DisableFormatting).Length, "Current length is inconsistent!");
                    currentMax = lastKidMax;
                }
            }
        }

        /// <summary>
        /// Takes an insecure html fragment and tidies and truncates its contents such that the result fits in the 
        /// given length and is safe to display.
        /// </summary>
        public static string TidyHtmlStringAndLimitLength(string input, int length)
        {
            int wrapperLength = "<x></x>".Length;
            XElement sanitizedWrappedInput = new XElement("x", HeuristicParse(input).Sanitize());
            XElement trimmedVersion = int.MaxValue - wrapperLength > length
                ? LimitLength(sanitizedWrappedInput, length + wrapperLength)
                : sanitizedWrappedInput;
            return XWrappedToString(trimmedVersion);
        }

        /// <summary>
        /// Takes an insecure html fragment and cleans it up.
        /// </summary>
        public static string SanitizeHtmlString(string input) => HeuristicParse(input).Sanitize().ToString();

        /// <summary>
        /// Strips xml tags from the string for readability.  The resulting string still needs to be encoded (i.e. it is not disable-output escaping safe.)
        /// This function also decodes xml entities and &amp;nbsp; into readable characters.
        /// </summary>
        public static string HtmlToTextParser(string str) => new XElement("x", HeuristicParse(str)).Value;

        /// <summary>
        /// Serializes a document fragment as passed by Progress.NET - i.e. all children of a meaningless "&lt;x&gt;" root node.
        /// </summary>
        static string XWrappedToString(XElement xRootEl)
        {
            return xRootEl.Name == "x"
                ? xRootEl.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)).JoinStrings()
                : xRootEl.ToString(SaveOptions.DisableFormatting);
        }

        static IEnumerable<XNode> FilterElem(XNode node, IHtmlFilter filter)
        {
            if (node is XText) {
                return Enumerable.Repeat(node, 1);
            } else if (node is XElement) {
                var elem = (XElement)node;
                var safety = filter.AllowTag(elem);
                if (safety == TagSafety.Unsafe) {
                    return Enumerable.Empty<XNode>();
                } else if (safety == TagSafety.Unknown) {
                    return elem.Nodes().SelectMany(childnode => FilterElem(childnode, filter));
                } else if (safety == TagSafety.Safe) {
                    return
                        Enumerable.Repeat<XNode>(
                            new XElement(
                                elem.Name,
                                elem.Attributes().Where(filter.AllowAttribute),
                                elem.Nodes().Select(childnode => FilterElem(childnode, filter))
                                ),
                            1);
                } else {
                    throw new ProgressNetException("enum error: " + safety + " is not a legal value");
                }
            } else {
                return Enumerable.Empty<XNode>(); //don't copy comments, processing instructions, etc.
            }
        }

        /// <summary>This function sanitizes an html tree.
        ///  - Any html that isn't recognized as html is considered content (e.g. a lone ampersand)
        ///  - Any html that can't be parsed (say, an unclosed element) is stripped.
        ///  - Any html that can be parsed but is explicitly considered "safe" list removed.
        ///  - Html that can be parsed and is explicitly on the "safe" list is retained.
        ///  
        ///  Be aware that the filters may receive a mixture of upper and/or lowercase tags, depending on the source!
        /// 
        /// For practical purposes, there is a HashSet based version that simply filters on element name.
        /// </summary>
        /// <param name="sourceHtml">The xhtml to sanitize.  The root element is ignored.</param>
        /// <returns>The parsed xhtml fragments without non-validating or unsafe tags.</returns>
        public static XhtmlData Sanitize(this XhtmlData sourceHtml) => sourceHtml.Sanitize(HtmlFilter.Default);

        /// <summary>This function sanitizes an html tree.
        ///  - Any html that isn't recognized as html is considered content (e.g. a lone ampersand)
        ///  - Any html that can't be parsed (say, an unclosed element) is stripped.
        ///  - Any html that can be parsed but is explicitly considered "safe" list removed.
        ///  - Html that can be parsed and is explicitly on the "safe" list is retained.
        ///  
        ///  Be aware that the filters may receive a mixture of upper and/or lowercase tags, depending on the source!
        /// 
        /// For practical purposes, there is a HashSet based version that simply filters on element name.
        /// </summary>
        /// <param name="sourceHtml">The xhtml to sanitize.  The root element is ignored.</param>
        /// <param name="filter">The filter to apply to elements (called for each element in the source).</param>
        /// <returns>The parsed xhtml fragments without non-validating or unsafe tags.</returns>
        public static XhtmlData Sanitize(this XhtmlData sourceHtml, IHtmlFilter filter)
        {
            filter = filter ?? HtmlFilter.Default;
            return XhtmlData.Create(sourceHtml.SelectMany(node => FilterElem(node, filter)));
        }
    }
}
