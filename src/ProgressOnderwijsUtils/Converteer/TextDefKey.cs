using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public sealed class TextDefKey : ITextDefKey
	{
		readonly string webmodule;
		readonly string sleutel;

		public string WebModule { get { return webmodule; } }
		public string Sleutel { get { return sleutel; } }

		public TextDefKey(string webmodule, string sleutel) { this.webmodule = webmodule; this.sleutel = sleutel; }

		public string GenerateUid() { return (webmodule + "/" + sleutel).ToLowerInvariant(); }
		public override string ToString() { return "KEY:" + GenerateUid(); }

		public TextVal Translate(ITranslationKeyLookup conn, Taal taal)
		{
			return conn.Lookup(taal, GenerateUid());
		}
	}
}
