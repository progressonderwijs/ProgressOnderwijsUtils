using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Text;

namespace ProgressOnderwijsUtils
{
	public static class Translatable
	{
		public static ITranslatable WithReplacement(this ITranslatable textdef, params ITranslatable[] toreplace) { return new TextDefReplacing(textdef, toreplace); }

		public static ITranslatable Append(this ITranslatable a, ITranslatable b) { return new ConcatTranslatable(a, b); }
		public static ITranslatable Append(this ITranslatable a, ITranslatable b, ITranslatable c) { return new ConcatTranslatable(a, b, c); }
		public static ITranslatable Append(this ITranslatable a, params ITranslatable[] rest) { return new ConcatTranslatable(new[] { a }.Concat(rest).ToArray()); }
		public static ITranslatable AppendAuto(this ITranslatable a, params object[] objects) { return new ConcatTranslatable(new[] { a }.Concat(objects.Select(o => Converteer.ToText(o))).ToArray()); }

		public static ITranslatable LimitLength(this ITranslatable translatable, int maxwidth)
		{
			LimitLengthTranslatable input_as_LL = translatable as LimitLengthTranslatable;
			return input_as_LL != null
					? (input_as_LL.m_maxwidth < maxwidth ? input_as_LL : new LimitLengthTranslatable(input_as_LL.m_text, maxwidth))
					: new LimitLengthTranslatable(translatable, maxwidth);
		}
		public static ITranslatable LimitLength(this ITranslatable translatable, int? maxwidth)
		{
			return maxwidth.HasValue ? LimitLength(translatable, maxwidth.Value) : translatable;
		}
		public static ITranslatable ForceLanguage(this ITranslatable translatable, Taal taal)
		{
			return (translatable as ForcedLanguageTranslatable) ?? new ForcedLanguageTranslatable(translatable, taal);
		}

		public static ITranslatable CreateTranslatable(TranslateFunction text)
		{
			return new SingleTranslatable(text);
		}
		public static ITranslatable CreateTranslatable(TranslateFunction text, TranslateFunction extratext)
		{
			return new DoubleTranslatable(text, extratext);
		}
		public static ITranslatable CreateLazyTranslatable(Func<ITranslatable> lazyCreator)
		{
			return new LazyTranslatable(lazyCreator);
		}

		public static ITranslatable Literal(string nl, string en)
		{
			return new BilingualTranslatable(TextVal.Create(nl), TextVal.Create(en));
		}
		public static ITranslatable Literal(TextVal nl, TextVal en)
		{
			return new BilingualTranslatable(nl, en);
		}
		public static ITranslatable Literal(string nl, string en, string du)
		{
			return new TrilingualTranslatable(TextVal.Create(nl), TextVal.Create(en), TextVal.Create(du));
		}
		public static ITranslatable Literal(TextVal nl, TextVal en, TextVal du)
		{
			return new TrilingualTranslatable(nl, en, du);
		}


		sealed class LimitLengthTranslatable : ITranslatable
		{
			public readonly ITranslatable m_text;
			public readonly int m_maxwidth;
			public LimitLengthTranslatable(ITranslatable text, int maxwidth) { m_text = text; m_maxwidth = maxwidth; }

			public string GenerateUid() { return m_maxwidth + "<" + m_text.GenerateUid(); }
			public TextVal Translate(ITranslationKeyLookup conn, Taal lang)
			{
				TextVal raw = m_text.Translate(conn, lang);
				var shortened = StringMeasurement.LimitTextLength(raw.Text, m_maxwidth);
				return TextVal.Create(shortened.Item1, string.IsNullOrEmpty(raw.ExtraText) && shortened.Item2 ? raw.Text : raw.ExtraText);
			}
			public override string ToString() { return GenerateUid(); }
		}
		sealed class SingleTranslatable : ITranslatable
		{
			readonly TranslateFunction stringify;
			public SingleTranslatable(TranslateFunction stringify) { this.stringify = stringify; }
			public string GenerateUid() { return stringify(); }
			public TextVal Translate(ITranslationKeyLookup conn, Taal lang) { return TextVal.Create(stringify(lang)); }
			public override string ToString() { return GenerateUid(); }
		}
		sealed class DoubleTranslatable : ITranslatable
		{
			readonly TranslateFunction textF, extratextF;
			public DoubleTranslatable(TranslateFunction textFactory, TranslateFunction extratextFactory) { textF = textFactory; extratextF = extratextFactory; }
			public string GenerateUid() { return Translate(null, Taal.NL).ToString(); }
			public TextVal Translate(ITranslationKeyLookup conn, Taal lang) { return TextVal.Create(textF(lang), extratextF(lang)); }
			public override string ToString() { return GenerateUid(); }

		}
		sealed class LazyTranslatable : ITranslatable
		{
			readonly Func<ITranslatable> currentText;
			readonly string uid;
			public LazyTranslatable(Func<ITranslatable> currentText) { this.currentText = currentText; uid = "LZ:" + currentText().GenerateUid(); }
			public string GenerateUid() { return uid; }
			public TextVal Translate(ITranslationKeyLookup conn, Taal lang) { return currentText().Translate(conn, lang); }
			public override string ToString() { return GenerateUid(); }

		}

		sealed class ForcedLanguageTranslatable : ITranslatable
		{
			readonly ITranslatable underlying;
			readonly Taal taal;
			public ForcedLanguageTranslatable(ITranslatable underlying, Taal taal) { this.underlying = underlying; this.taal = taal; }
			public string GenerateUid() { return underlying.GenerateUid(); }
			public TextVal Translate(ITranslationKeyLookup conn, Taal lang) { return underlying.Translate(conn, taal); }
			public override string ToString() { return GenerateUid(); }
		}
	}
}
