using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	public sealed class LiteralTranslatable : ByValueHelperBase<LiteralTranslatable>, ITranslatable
	{
		public readonly string nl, en, du;

		internal LiteralTranslatable(string nl, string en, string du)
		{
			this.nl = nl;
			this.en = en;
			this.du = du;
		}

		public string GenerateUid()
		{
			return Convert.ToString((nl ?? "").GetHashCode(), 16) + " " + Convert.ToString((en ?? "").GetHashCode(), 16)
				   + " " + Convert.ToString((du ?? "").GetHashCode(), 16);
		}

		public TextVal Translate(ITranslationKeyLookup connectionOrContext, Taal lang)
		{
			var retval =
				lang == Taal.NL ? TextVal.Create(nl)
				: lang == Taal.EN ? TextVal.Create(en)
				: lang == Taal.DU ? TextVal.Create(du)
				: TextVal.CreateUndefined(nl ?? en ?? du);

			if (!retval.IsEmpty)
				return retval;
			else
				return TextVal.Create("~" + nl);
		}

		public LiteralTranslatableWithToolTip WithTooltip(string tooltipNL, string tooltipEN, string tooltipDU = null)
		{
			return new LiteralTranslatableWithToolTip(TextVal.Create(nl, tooltipNL), TextVal.Create(en, tooltipEN), TextVal.Create(du, tooltipDU));
		}
	}


	public sealed class LiteralTranslatableWithToolTip : ByValueHelperBase<LiteralTranslatableWithToolTip>, ITranslatable
	{
		public readonly TextVal nl, en, du;

		internal LiteralTranslatableWithToolTip(TextVal nl, TextVal en, TextVal du)
		{
			this.nl = nl;
			this.en = en;
			this.du = du;
		}

		public string GenerateUid()
		{
			return Convert.ToString(nl.GetHashCode(), 16) + " " + Convert.ToString(en.GetHashCode(), 16)
			+ " " + Convert.ToString(du.GetHashCode(), 16);
		}

		public TextVal Translate(ITranslationKeyLookup connectionOrContext, Taal lang)
		{
			var retval =
				lang == Taal.NL ? nl
				: lang == Taal.EN ? en
				: lang == Taal.DU ? du
				: nl;


			if (!retval.IsEmpty)
				return retval;
			else
				return TextVal.Create("~" + nl.Text, "~" + nl.ExtraText);
		}
	}
}