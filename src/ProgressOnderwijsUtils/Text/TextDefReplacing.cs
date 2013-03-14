using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	public sealed class TextDefReplacing : ITranslatable
	{
		readonly ITranslatable[] toreplace;
		readonly ITranslatable core;

		public TextDefReplacing(ITranslatable core, params ITranslatable[] toreplace)
		{
			this.core = core;
			this.toreplace = toreplace;
		}

		public string GenerateUid()
		{
			return core.GenerateUid() + "|" + toreplace.Select(it => it.GenerateUid()).JoinStrings(";");
		}

		public TextVal Translate(Taal taal)
		{
			TextVal retval = core.Translate(taal);
			TextVal[] replacements = toreplace.Select(it => it.Translate(taal)).ToArray();
			string[] mainTextRep = replacements.Select(tv => tv.Text).ToArray();
			string[] helpTextRep = replacements.Select(tv => string.IsNullOrEmpty(tv.ExtraText) ? tv.Text : tv.ExtraText).ToArray();
			return TextVal.Create(retval.Text == null ? null : string.Format(retval.Text, mainTextRep), retval.ExtraText == null ? null : string.Format(retval.ExtraText, helpTextRep));
		}

		public ITranslatable Core { get { return core; } } // needed for testing purposes ...
	}
}
