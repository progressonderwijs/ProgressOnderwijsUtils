using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Text;

namespace ProgressOnderwijsUtils
{
	public static class Translatable
	{
		public static ITranslatable WithReplacement(this ITranslatable textdef, params ITranslatable[] toreplace) { return new TextDefReplacing(textdef, toreplace); }

		public static ITranslatable Append(this ITranslatable a, ITranslatable b) { return new ConcatTranslatable(a, b); }
		public static ITranslatable Append(this ITranslatable a, ITranslatable b, ITranslatable c) { return new ConcatTranslatable(a, b, c); }
		public static ITranslatable Append(this ITranslatable a, params ITranslatable[] rest) { return new ConcatTranslatable(new[] { a }.Concat(rest).ToArray()); }
		public static ITranslatable AppendAuto(this ITranslatable a, params object[] objects) {
			var args = new ITranslatable[objects.Length + 1];
			int i = 1;
			args[0] = a;
			foreach (var obj in objects) //perf: no LINQ
				args[i++] = Converteer.ToText(obj);

			return new ConcatTranslatable(args); 
		}
		public static ITranslatable AppendTooltipAuto(this ITranslatable a, params object[] objects) {
			var args = new ITranslatable[objects.Length + 1];
			int i = 1;
			args[0] = a;
			foreach (var obj in objects) //perf: no LINQ
				args[i++] = Converteer.ToText(obj).TextToTooltip();

			return new ConcatTranslatable(args);
		}

		public static ITranslatable JoinTexts(this IEnumerable<ITranslatable> a, ITranslatable splitter) {
			var builder = FastArrayBuilder<ITranslatable>.Create();
			using (var iter = a.GetEnumerator())
			{
				if(iter.MoveNext())
					builder.Add(iter.Current);
				while (iter.MoveNext())
				{
					builder.Add(splitter);
					builder.Add(iter.Current);//perf:no LINQ
				}
			}
			return new ConcatTranslatable(builder.ToArray()); 
		}

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

		//public static LiteralTranslatable Literal(string nl, string en = null, string du = null)
		//{
		//	return new LiteralTranslatable(nl, en, du);
		//}

		public static LiteralTranslatable Empty()
		{ 
			return new LiteralTranslatable("", "", "");
		}

		public static ITranslatable EmptyWithTooltip(ITranslatable tooltipnl)
		{
			return Translatable.Empty().AppendTooltipAuto(tooltipnl);
		}

		public static ITranslatable EmptyWithTooltip(ITranslatable tooltipnl, ITranslatable tooltipen)
		{
			return Translatable.Empty().AppendTooltipAuto(tooltipnl, tooltipen);
		}

		public static ITranslatable EmptyWithTooltip(ITranslatable tooltipnl, ITranslatable tooltipen, ITranslatable tooltipdu)
		{
			return Translatable.Empty().AppendTooltipAuto(tooltipnl, tooltipen, tooltipdu);
		}

		public static LiteralTranslatable Literal(string nl)
		{
			return new LiteralTranslatable(nl, null, null);
		}
		public static LiteralTranslatable Literal(string nl, string en)
		{
			return new LiteralTranslatable(nl, en, null);
		}
		public static LiteralTranslatable Literal(string nl, string en, string du)
		{
			return new LiteralTranslatable(nl, en, du);
		}

		public static ITranslatable ReplaceTooltipWithText(this ITranslatable translatable, ITranslatable tt)
		{
			return CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt.Translate(taal).Text);
		}

		public static ITranslatable TextToTooltip(this ITranslatable translatable)
		{
			return CreateTranslatable(taal => "", taal => translatable.Translate(taal).Text);
		}


		public static ITranslatable ReplaceTooltipWithTooltip(this ITranslatable translatable, ITranslatable tt)
		{
			return CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt.Translate(taal).ExtraText);
		}

		public static ITranslatable ReplaceTooltipWithText(this ITranslatable translatable, string tt)
		{
			return CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt);
		}

		sealed class LimitLengthTranslatable : ITranslatable
		{
			public readonly ITranslatable m_text;
			public readonly int m_maxwidth;
			public LimitLengthTranslatable(ITranslatable text, int maxwidth) { m_text = text; m_maxwidth = maxwidth; }

			public string GenerateUid() { return m_maxwidth + "<" + m_text.GenerateUid(); }
			public TextVal Translate(Taal lang)
			{
				TextVal raw = m_text.Translate(lang);
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
			public TextVal Translate(Taal lang) { return TextVal.Create(stringify(lang)); }
			public override string ToString() { return GenerateUid(); }
		}
		sealed class DoubleTranslatable : ITranslatable
		{
			readonly TranslateFunction textF, extratextF;
			public DoubleTranslatable(TranslateFunction textFactory, TranslateFunction extratextFactory) { textF = textFactory; extratextF = extratextFactory; }
			public string GenerateUid() { return Translate(Taal.NL).ToString(); }
			public TextVal Translate(Taal lang) { return TextVal.Create(textF(lang), extratextF(lang)); }
			public override string ToString() { return GenerateUid(); }

		}
		sealed class LazyTranslatable : ITranslatable
		{
			readonly Func<ITranslatable> currentText;
			readonly string uid;
			public LazyTranslatable(Func<ITranslatable> currentText) { this.currentText = currentText; uid = "LZ:" + currentText().GenerateUid(); }
			public string GenerateUid() { return uid; }
			public TextVal Translate(Taal lang) { return currentText().Translate(lang); }
			public override string ToString() { return GenerateUid(); }

		}

		sealed class ForcedLanguageTranslatable : ITranslatable
		{
			readonly ITranslatable underlying;
			readonly Taal taal;
			public ForcedLanguageTranslatable(ITranslatable underlying, Taal taal) { this.underlying = underlying; this.taal = taal; }
			public string GenerateUid() { return underlying.GenerateUid(); }
			public TextVal Translate(Taal lang) { return underlying.Translate(taal); }
			public override string ToString() { return GenerateUid(); }
		}
	}
}
