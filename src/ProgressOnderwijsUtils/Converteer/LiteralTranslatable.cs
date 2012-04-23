using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public sealed class LiteralTranslatable : ITranslatable
	{
		readonly TextVal nl, en;

		public LiteralTranslatable(TextVal nl, TextVal en)
		{
			this.nl = nl;
			this.en = en;
		}

		public LiteralTranslatable(string nl, string en) : this(TextVal.Create(nl), TextVal.Create(en))
		{
		}

		public string GenerateUid()
		{
			return Convert.ToString(nl.GetHashCode(), 16) + " " + Convert.ToString(en.GetHashCode(), 16);
		}

		public TextVal Translate(ITranslationKeyLookup connectionOrContext, Taal lang)
		{
			if (lang == Taal.NL)
				return nl;
			else if (lang == Taal.EN)
				return en;
			else
				return TextVal.CreateUndefined(nl.Text ?? en.Text);
		}
	}
}