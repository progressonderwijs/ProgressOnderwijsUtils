using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	//public sealed class TrilingualTranslatable :ByValueHelperBase<TrilingualTranslatable>, ITranslatable
	//{
	//    readonly TextVal nl, en, du;

	//    internal TrilingualTranslatable(TextVal nl, TextVal en, TextVal du)
	//    {
	//        this.nl = nl;
	//        this.en = en;
	//        this.du = du;
	//    }

	//    public string GenerateUid()
	//    {
	//        return Convert.ToString(nl.GetHashCode(), 16)
	//            + " " + Convert.ToString(en.GetHashCode(), 16)
	//            + " " + Convert.ToString(du.GetHashCode(), 16);
	//    }

	//    public TextVal Translate(ITranslationKeyLookup connectionOrContext, Taal lang)
	//    {
	//        if (lang == Taal.NL)
	//            return nl;
	//        else if (lang == Taal.EN)
	//            return en;
	//        else if (lang == Taal.DU)
	//            return du;
	//        else
	//            return TextVal.CreateUndefined(nl.Text ?? en.Text);
	//    }
	//}
}