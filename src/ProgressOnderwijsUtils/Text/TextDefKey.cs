using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	//public sealed class TextDefKey :ITranslatable
	//{
	//    readonly string webmodule;
	//    readonly string sleutel;

	//    public TextDefKey(string webmodule, string sleutel) { this.webmodule = webmodule; this.sleutel = sleutel; }
	//    public static string cleanKey(string messyKey) { return messyKey.Replace(' ', '_').Replace('.', '_').Replace('-', '_').Replace("]", "").Replace("[", ""); }

	//    public string GenerateUid() { return (webmodule + "/" + cleanKey(sleutel)).ToLowerInvariant(); }
	//    public override string ToString() { return "KEY:" + GenerateUid(); }

	//    public TextVal Translate(Taal taal)
	//    {
	//        string uid = GenerateUid();
	//        var lookedup = teksten.GetOrDefault(uid, null);
	//        if (lookedup is LiteralTranslatable || lookedup is LiteralTranslatableWithToolTip)
	//        {
	//            return lookedup.Translate(taal);
	//        }
	//        else return TextVal.CreateUndefined(uid);
	//    }

	//    static readonly Dictionary<string, ITranslatable> teksten =
	//        typeof(Texts).GetNestedTypes(BindingFlags.Public)
	//            .SelectMany(type =>
	//                        type.GetFields(BindingFlags.Public | BindingFlags.Static)
	//            ).ToDictionary(fi => fi.DeclaringType.Name.ToLowerInvariant() + "/" + fi.Name.ToLowerInvariant(), fi => (ITranslatable)fi.GetValue(null));
	//}

	public sealed class NonsenseTranslatable : ITranslatable
	{
		readonly string webmodule;
		readonly string sleutel;


		public NonsenseTranslatable(string webmodule, string sleutel) { this.webmodule = webmodule; this.sleutel = sleutel; }
		public static string cleanKey(string messyKey) { return messyKey.Replace(' ', '_').Replace('.', '_').Replace('-', '_').Replace("]", "").Replace("[", ""); }

		public string GenerateUid() { return (webmodule + "/" + cleanKey(sleutel)).ToLowerInvariant(); }
		public override string ToString() { return "KEY:" + GenerateUid(); }

		public TextVal Translate(Taal taal)
		{
			return TextVal.CreateUndefined(GenerateUid());
		}
	}

}
