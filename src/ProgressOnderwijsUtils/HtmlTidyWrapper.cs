using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils.Extensions;
using System.Xml;

namespace ProgressOnderwijsUtils
{
	public enum TagSafety
	{
		ReplaceWithContent,
		RemoveCompletely,
		Secure
	}


	//TODO: we should implement a real wrapper around html tidy sometime to support invalid but almost valid XHTML - but until we do, we'll just need to fail on
	//invalid html.
	public static class HtmlTidyWrapper
	{
		static Regex xmlStripRegex = new Regex(@"(<[^<]*>)",
						RegexOptions.Singleline |
						RegexOptions.ExplicitCapture |
						RegexOptions.Compiled);
		static Regex symbolRegex = new Regex(@"\<|\>|\&(?!\w\w\w?;)",
						RegexOptions.Singleline | RegexOptions.IgnoreCase |
						RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture |
						RegexOptions.Compiled);

		/// <summary>
		/// Attempts to parse an html fragment as xml.  This works because our html is generally well formed xml.
		/// When parsing fails, the method returns a best-guess string representation (which tries to at least decode entities into unicode.
		///
		/// This method should eventually be replaced by a real call to Tidy.NET.
		/// 
		/// This is not a security check; the output of this function can still contain dangerous tags!
		/// </summary>
		/// <returns>The html fragment wrapped in a top-level "x" element - this toplevel element just
		/// serves to permit a sequence of html nodes to be returned.  This function always returns a non-null single element named "x".</returns> 
		public static XElement XHtmlParser(string str)
		{
			try { return XElement.Parse("<x>" + str + "</x>"); }
			catch (System.Xml.XmlException) { ; }
			str = str.Replace("&nbsp;", "&#160;");
			try { return XElement.Parse("<x>" + str + "</x>"); }
			catch (System.Xml.XmlException) { ; }

			// decoding failed; the string is invalid.  To get at least something readable, 
			// we'll manually strip something that looks like tags
			str = xmlStripRegex.Replace(str, ""); //strip everything looking like tags
			str = symbolRegex.Replace(str, match => new XText(match.Value).ToString());
			//and then replace odd symbols by their encoded representations.
			try { return XElement.Parse("<x>" + str + "</x>"); }
			catch (System.Xml.XmlException) { ; }

			//can't parse; give up and return the best we can.
			return new XElement("x", str);
		}

		public static XElement LimitLength(XElement input, int length)
		{

			XElement output = new XElement(input);
			XNode current = output;
			int currentMax = length;
			while (true)
			{
				var stringRep = current.ToString(SaveOptions.DisableFormatting);
				int currentLength = stringRep.Length;

				if (currentLength < currentMax) return output;
				else if (current is XComment
					|| current is XElement && ((XElement)current).IsEmpty
					|| current is XDocumentType
					|| current is XProcessingInstruction)
				{
					current.Remove();
					return output;
				}
				else if (current is XText)
				{
					XText text = current as XText;
					text.Value = text.Value.Substring(0, currentMax);
					return output;
				}
				XElement currentEl = (XElement)current;

				int lastKidLen = currentEl.LastNode.ToString(SaveOptions.DisableFormatting).Length;
				if (currentLength - lastKidLen > currentMax)
					currentEl.LastNode.Remove();
				else
				{
					int restLen = currentLength - lastKidLen;
					int lastKidMax = currentMax - restLen;
					current = currentEl.LastNode;
					currentMax = lastKidMax;
				}
			}
		}

		/// <summary>
		/// Stips xml tags from the string for readability.  The resulting string still needs to be encoded (i.e. it is not disable-output escaping safe.)
		/// This function also decodes entities into readable characters.
		/// </summary>
		public static string HtmlToTextParser(string str) { return XHtmlParser(str).Value; }


		private static IEnumerable<XNode> FilterElem(XNode node, Func<XElement, TagSafety> elemFilter, Func<XAttribute, bool> attrFilter)
		{
			if (node is XText)
				return Enumerable.Repeat(node, 1);
			else if (node is XElement)
			{
				XElement elem = (XElement)node;
				TagSafety safety = elemFilter(elem);
				if (safety == TagSafety.RemoveCompletely)
					return Enumerable.Empty<XNode>();
				else if (safety == TagSafety.ReplaceWithContent)
					return elem.Nodes().SelectMany(childnode => FilterElem(childnode, elemFilter, attrFilter));
				else if (safety == TagSafety.Secure)
					return
						Enumerable.Repeat<XNode>(
							new XElement(elem.Name,
								elem.Attributes().Where(attrFilter),
								elem.Nodes().Select(childnode => FilterElem(childnode, elemFilter, attrFilter))
							)
							, 1);
				else
					throw new ProgressNetException("enum error: " + safety + " is not a legal value");
			}
			else
				return Enumerable.Empty<XNode>();//don't copy comments, processing instructions, etc.
		}

		//            string htmlTagsAllowed = allowedHtml == "ProgressAllowed"
		//                                      ? @"(<)(br|hr|p|\/p|table|\/table|tr|\/tr|thead|\/thead|
		//										tbody|\/tbody|td|\/td|th|\/th|span|\/span|input|div|\/div)[^<]*>"
		//                                      : allowedHtml;
		//if (str != null)
		//    str = str.MultiReplace(RegexOptions.IgnoreCase,
		//            Utils.ToTuples(htmlTagsAllowed, "@~#$2#~@"));


		/// <summary>This function sanitizes an html tree.
		///  - Any html that isn't recognized as html is considered content (e.g. a lone ampersand)
		///  - Any html that can't be parsed (say, an unclosed element) is stripped.
		///  - Any html that can be parsed but is explicitly considered "safe" list removed.
		///  - Html that can be parsed and is explicitly on the "safe" list is retained.
		///  
		///  Note that the filters may receive a mixture of upper and/or lowercase tags, depending on the source!
		/// 
		/// For practical purposes, there is a HashSet based version that simply filters on element name.
		/// </summary>
		/// <param name="sourceHtml">The xhtml to sanitize.  The root element is ignored.</param>
		/// <param name="elemFilter">The filter to apply to elements (called for each element in the source).</param>
		/// <param name="attrFilter">The filter to apply to attributes (called for each attribute in the source).</param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.  The content is
		/// wrapped in a top-level meaningless "x" tag for transport</returns>
		public static XElement HtmlSanitizer(XElement sourceHtml, Func<XElement, TagSafety> elemFilter, Func<XAttribute, bool> attrFilter)
		{
			return new XElement("x",
				sourceHtml.Nodes().SelectMany(node => FilterElem(node, elemFilter, attrFilter))
				);
		}

		/// <summary>This function sanitizes an html tree.
		///  - Any element that is banned is removed along with all its attributes and descendants.
		///  - Any element that is in the safe set is retained, and its attributes and descendants are recursively verified in the same way.
		///    If a "safe" element contains an unsafe element, the unsafe element is thus removed.
		///  - Any html that is neither banned nor safe is removed, but its (safe) children are included in the output.
		///  - text nodes are considered safe.
		///  - Only attributes that are in the safe set and themselves are defined on the safe set are copied to the output.
		/// </summary>
		/// <param name="sourceHtml">The xhtml to sanitize.  The root element is ignored.</param>
		/// <param name="bannedElements">Elements and their descendents with a name in this list are removed completely.  All names
		/// in this list must be lowercase.  Matching occurs case insensitively.</param>
		/// <param name="safeElements">Elements with a case-sensitive name in this list are retained in the output.  
		/// The attributes and descendents are recursively filtered.</param>
		/// <param name="safeAttributes">Attributes on elements deemed safe with a case-sensitive name in this list are retained in the output.</param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.  The content is
		/// wrapped in a top-level meaningless "x" tag for transport</returns>
		public static XElement HtmlSanitizer(XElement sourceHtml, HashSet<string> safeElements, HashSet<string> safeAttributes, HashSet<string> bannedElements)
		{
			if (bannedElements.Overlaps(safeElements.Select(elem => elem.ToLowerInvariant())))
				throw new ProgressNetException("Some elements are both banned and safe: this makes no sense.");
			else if (!bannedElements.All(name => name.ToLowerInvariant() == name))
				throw new ProgressNetException("All banned elements must be in lower case.");

			return HtmlSanitizer(sourceHtml,
				elem => safeElements.Contains(elem.Name.LocalName) ? TagSafety.Secure :
						bannedElements.Contains(elem.Name.LocalName.ToLowerInvariant()) ? TagSafety.RemoveCompletely : TagSafety.ReplaceWithContent,
				attr => safeAttributes.Contains(attr.Name.LocalName));
		}

		/// <summary>This function parses and sanitizes an html tree.  
		///  - Any html that isn't recognized as html is considered content (e.g. a lone ampersand)
		///  - Any html that can't be parsed (say, an unclosed element) is stripped.
		///  - Any html that can be parsed is processed according the above overloads.
		/// 
		/// This overload uses default values for filtering elements and attributes.
		/// </summary>
		/// <param name="sourceString">The string to parse as xhtml and then sanitize.  The root element is ignored.</param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.  The content is
		/// wrapped in a top-level meaningless "x" tag for transport</returns>
		public static XElement HtmlSanitizer(string sourceString)
		{
			XElement parsed = XHtmlParser(sourceString);
			return HtmlSanitizer(parsed, defaultSafeElements, defaultSafeAttr, defaultBannedElements);
		}
		/// <summary>This function parses and sanitizes an html tree.  
		///  - Any html that isn't recognized as html is considered content (e.g. a lone ampersand)
		///  - Any html that can't be parsed (say, an unclosed element) is stripped.
		///  - Any html that can be parsed is processed according the above overloads.
		/// 
		/// This overload uses default values for safe attributes and banned elements.
		/// </summary>
		/// <param name="sourceString">The string to parse as xhtml and then sanitize.  The root element is ignored.</param>
		/// <param name="safeElements">The safe elements to permit.  </param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.  The content is
		/// wrapped in a top-level meaningless "x" tag for transport</returns>
		public static XElement HtmlSanitizer(string sourceString, HashSet<string> safeElements)
		{
			XElement parsed = XHtmlParser(sourceString);
			return HtmlSanitizer(parsed, safeElements, defaultSafeAttr, defaultBannedElements);
		}


		//door http://www.w3schools.com/tags/ en redelijke selectie gekozen.
		//om tracer elements te vermijden zijn is img wel maar attribuut src niet toegestaan.  Om geen form-problemen te hebben mogen form elementen niet.
		static HashSet<string> defaultSafeElements = new HashSet<string>(
			"b i big small em strong hr br p span div center font table thead col colgroup tbody tfoot caption tr td th h1 h2 h3 h4 h5 h6 a cite dfn code samp var dl dt dd ins del sub sup tt ul ol li pre q abbr acronym blockquote fieldset legend img".Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
		//id's en names zijn niet toegestaan zodat er geen conflict kan onstaan met onze code.  style is niet secuur en kan tevens tracing info bevatten.
		static HashSet<string> defaultSafeAttr = new HashSet<string>(new[]{
			"lang", "title", "href", "dir", "color", "border", "face", "size", "align", "alt", 
			"bgcolor", "cellspacing", "cellpadding", "char", "charoff", "cite", "height", "width"
		});
		//de inhoud van deze elemented is volledig oninteressant.
		static HashSet<string> defaultBannedElements = new HashSet<string>(new[]{
			"script","style"
		});

		public static IEnumerable<string> DefaultSafeElements { get { return defaultSafeElements.AsEnumerable(); } }
		public static IEnumerable<string> DefaultSafeAttributes { get { return defaultSafeAttr.AsEnumerable(); } }
		public static IEnumerable<string> DefaultBannedElements { get { return defaultBannedElements.AsEnumerable(); } }

	}
}
