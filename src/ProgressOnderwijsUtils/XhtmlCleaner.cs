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
		ReplaceWithContent,
		RemoveCompletely,
		Secure
	}


	//TODO: we should implement a real wrapper around html tidy sometime to support invalid but almost valid XHTML - but until we do, we'll just need to fail on
	//invalid html.
	public static class XhtmlCleaner
	{
		static readonly Regex xmlStripRegex = new Regex(@"(<\/?[A-Za-z_][^<]*>)",
						RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		static readonly Regex symbolRegex = new Regex(@"\<|\>|\&(?![A-Za-z][A-Za-z]+;)",
						RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		static readonly Regex entityRegex = new Regex(@"\&[A-Za-z][A-Za-z]+;",
			RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		/// <summary>
		/// Attempts to parse an html fragment as xml.  This works because our html is generally well formed xml.
		/// When parsing fails, the method returns a best-guess string representation (which tries to at least decode entities into unicode.
		/// To fit the result into an element, it is wrapped in a top level "x" element.
		/// 
		/// This method should eventually be replaced by a real call to Tidy.NET.
		/// 
		/// This is not a security check; the output of this function can still contain dangerous tags!
		/// 
		/// </summary>
		/// <returns>The html fragment.</returns> 
		static XhtmlData HeuristicParse(string str)
		{
			XhtmlData? result = TryParse(str);
			if (result != null) return result.Value;

			// decoding failed; the string is invalid.  To get at least something readable, 
			// we'll manually strip something that looks like tags
			str = xmlStripRegex.Replace(str, ""); //strip everything looking like tags
			str = symbolRegex.Replace(str, match => new XText(match.Value).ToString());
			//and then replace odd (i.e. non-conforming) symbols by their encoded representations...

			return TryParse(str) //then return markup without tags
				?? XhtmlData.Create(str);//if all else fails, return raw markup safely encoded as content.
		}

		public static XhtmlData? TryParse(string str)
		{
			try
			{
				var strWithoutNonXmlEntities = StrWithoutNonXmlEntities(str);

				return XhtmlData.Create(XElement.Parse("<x>" + strWithoutNonXmlEntities + "</x>", LoadOptions.PreserveWhitespace).Nodes());
			}
			catch (XmlException)
			{
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
				entityRegex.Replace(str, match => {
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
			while (true)
			{
				if (currentLen <= currentMax) return output;
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
					var text = current as XText;
					//since XText does some \r magic, we can't just assume current.ToString is equivalent to text.Value
					text.Value = current.ToString(SaveOptions.DisableFormatting).Replace('\r', ' ').Substring(0, currentMax);
					return output;
				}
				var currentEl = (XElement)current;

				int lastKidLen = currentEl.LastNode.ToString(SaveOptions.DisableFormatting).Length;
				if (currentLen - lastKidLen > currentMax)
				{
					currentEl.LastNode.Remove();
					currentLen = !currentEl.IsEmpty ? currentLen - lastKidLen : current.ToString(SaveOptions.DisableFormatting).Length;
					Debug.Assert(currentLen == current.ToString(SaveOptions.DisableFormatting).Length, "Current length is inconsistent!");
				}
				else
				{
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
			XElement sanitizedWrappedInput = new XElement("x", HtmlSanitizer(input));
			XElement trimmedVersion = int.MaxValue - wrapperLength > length
				? LimitLength(sanitizedWrappedInput, length + wrapperLength)
				: sanitizedWrappedInput;
			return XWrappedToString(trimmedVersion);
		}

		/// <summary>
		/// Takes an insecure html fragment and cleans it up.
		/// </summary>
		public static string TidyHtmlString(string input) { return HtmlSanitizer(input).ToString(); }


		/// <summary>
		/// Stips xml tags from the string for readability.  The resulting string still needs to be encoded (i.e. it is not disable-output escaping safe.)
		/// This function also decodes xml entities and &amp;nbsp; into readable characters.
		/// </summary>
		public static string HtmlToTextParser(string str) { return new XElement("x", HeuristicParse(str)).Value; }

		/// <summary>
		/// Serializes a document fragment as passed by Progress.NET - i.e. all children of a meaningless "&lt;x&gt;" root node.
		/// </summary>
		static string XWrappedToString(XElement xRootEl)
		{
			return xRootEl.Name == "x"
				? xRootEl.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)).JoinStrings()
				: xRootEl.ToString(SaveOptions.DisableFormatting);
		}

		static IEnumerable<XNode> FilterElem(XNode node, Func<XElement, TagSafety> elemFilter, Func<XAttribute, bool> attrFilter)
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
		/// <param name="elemFilter">The filter to apply to elements (called for each element in the source).</param>
		/// <param name="attrFilter">The filter to apply to attributes (called for each attribute in the source).</param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.</returns>
		static XhtmlData HtmlSanitizer(XhtmlData sourceHtml, Func<XElement, TagSafety> elemFilter, Func<XAttribute, bool> attrFilter)
		{
			return XhtmlData.Create(
				sourceHtml.SelectMany(node => FilterElem(node, elemFilter, attrFilter))
				);
		}

		/// <summary>This function sanitizes an html tree fragment and returns it wrapped in a single toplevel "x" element.
		///  - Any element that is banned is removed along with all its attributes and descendants.
		///  - Any element that is in the safe set is retained, and its attributes and descendants are recursively verified in the same way.
		///    If a "safe" element contains an unsafe element, the unsafe element is thus removed.
		///  - Any html that is neither banned nor safe is removed, but its (safe) children are included in the output.
		///  - text nodes are considered safe.
		///  - Only attributes that are in the safe set and themselves are defined on the safe set are copied to the output.
		/// </summary>
		/// <param name="sourceHtml">The xhtml to sanitize.</param>
		/// <param name="bannedElements">Elements and their descendents with a name in this list are removed completely.  All names
		/// in this list must be lowercase.  Matching occurs case insensitively.  Null means use default selection.</param>
		/// <param name="safeElements">Elements with a case-sensitive name in this list are retained in the output.  
		/// The attributes and descendents are recursively filtered.  Null means use default selection.</param>
		/// <param name="safeAttributes">Attributes on elements deemed safe with a case-sensitive name in this list are retained in the output.  Null means use default selection.</param>
		/// <returns>The parsed xhtml fragments without non-validating or unsafe tags.</returns>
		public static XhtmlData HtmlSanitizer(XhtmlData sourceHtml, HashSet<string> safeElements = null, HashSet<string> safeAttributes = null, HashSet<string> bannedElements = null)
		{
			safeAttributes = safeAttributes ?? defaultSafeAttr;
			safeElements = safeElements ?? defaultSafeElements;
			bannedElements = bannedElements ?? defaultBannedElements;
			if (bannedElements.Overlaps(safeElements.Select(elem => elem.ToLowerInvariant())))
				throw new ProgressNetException("Some elements are both banned and safe: this makes no sense.");
			else if (bannedElements.Any(name => name.ToLowerInvariant() != name))
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
		/// This overload takes a string as input and attempts to parse it.
		/// </summary>
		public static XhtmlData HtmlSanitizer(string sourceString, HashSet<string> safeElements = null, HashSet<string> safeAttributes = null, HashSet<string> bannedElements = null)
		{
			XhtmlData parsed = HeuristicParse(sourceString);
			return HtmlSanitizer(parsed, safeElements, safeAttributes, bannedElements);
		}

		//door http://www.w3schools.com/tags/ en redelijke selectie gekozen.
		//om tracer elements te vermijden zijn is img wel maar attribuut src niet toegestaan Bovendien kan src="javascript:..." dus src mag echt niet! Om geen form-problemen te hebben mogen form elementen niet.
		static readonly HashSet<string> defaultSafeElements = new HashSet<string>(
			"b i big small em strong hr br p span div center font table thead col colgroup tbody tfoot caption tr td th h1 h2 h3 h4 h5 h6 a cite dfn code samp var dl dt dd ins del sub sup tt ul ol li pre q abbr acronym blockquote fieldset legend img".Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
		//public static IEnumerable<string> DefaultSafeElements { get { return defaultSafeElements; } }
		//id's en names zijn niet toegestaan zodat er geen conflict kan onstaan met onze code.  style is niet secuur en kan tevens tracing info bevatten.
		static readonly HashSet<string> defaultSafeAttr = new HashSet<string>(new[] { "lang", "title", "href", "dir", "color", "border", "face", "size", "align", "alt", "bgcolor", "cellspacing", "cellpadding", "char", "charoff", "cite", "height", "width", "colspan", "rowspan" });
		public static IEnumerable<string> DefaultSafeAttributes { get { return defaultSafeAttr; } }

		//de inhoud van deze elementen is volledig oninteressant.
		static readonly HashSet<string> defaultBannedElements = new HashSet<string>(new[] { "script", "style" });
		//public static IEnumerable<string> DefaultBannedElements { get { return defaultBannedElements; } }
	}


}
