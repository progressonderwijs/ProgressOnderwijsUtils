using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{

	//TODO: we should implement a real wrapper around html tidy sometime to support invalid but almost valid XHTML - but until we do, we'll just need to fail on
	//invalid html.
	public static class HtmlTidyWrapper
	{
		static Regex xmlStripRegex = new Regex(@"\<[^<]*\>", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		static Regex symbolRegex = new Regex(@"\<|\>|\&(?!\w\w\w?;)", RegexOptions.Singleline|RegexOptions.IgnoreCase|RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		/// <summary>
		/// Stips xml tags from the string for readability.  This is not a security check; the output of this function should still be encoded.
		/// This function also decodes entities into readable characters.
		/// </summary>
		public static string HtmlToTextParser(string str)
		{
			try	{ return XElement.Parse("<x>" + str + "</x>").Value; } catch (System.Xml.XmlException) { }

			// decoding failed; the string is invalid.  To get at least something readable, we'll manually strip something that looks like tags
			str = xmlStripRegex.Replace(str, ""); //strip everything looking like tags
			str = symbolRegex.Replace(str, match => new XText(match.Value).ToString()); //and then replace odd symbols by their encoded representations.
			
			
			try	{ return XElement.Parse("<x>" + str + "</x>").Value; } catch (System.Xml.XmlException) { } 
			
			//can't parse; give up and return the best we can.
			return str;
		}

	}
}
