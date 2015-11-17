using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class Translatable
    {
        public const string HtmlTooltipToken = "\aHTML\a";

        [Pure]
        public static ITranslatable WithReplacement(this ITranslatable textdef, params ITranslatable[] toreplace) => new ReplacingTranslatable(textdef, toreplace);

        [Pure]
        public static ITranslatable Append(this ITranslatable a, ITranslatable b) => new ConcatTranslatable(a, b);

        [Pure]
        public static ITranslatable Append(this ITranslatable a, ITranslatable b, ITranslatable c) => new ConcatTranslatable(a, b, c);

        [Pure]
        public static ITranslatable AppendAuto(this ITranslatable a, params object[] objects)
        {
            var args = new ITranslatable[objects.Length + 1];
            int i = 1;
            args[0] = a;
            foreach (var obj in objects) { //perf: no LINQ
                args[i++] = Converteer.ToText(obj);
            }

            return new ConcatTranslatable(args);
        }

        [Pure]
        public static ITranslatable TooltipIsHtml(this ITranslatable translatable)
        {
            return new HtmlTooltipTranslatable(translatable);
        }

        class HtmlTooltipTranslatable : ITranslatable
        {
            readonly ITranslatable underlying;

            public HtmlTooltipTranslatable(ITranslatable underlying)
            {
                this.underlying = underlying;
            }

            public string GenerateUid()
            {
                return underlying.GenerateUid() + "<>";
            }

            public TextVal Translate(Taal lang)
            {
                var textVal = underlying.Translate(lang);
                return new TextVal(textVal.Text, textVal.ExtraText + HtmlTooltipToken);
            }
        }

        [Pure]
        public static ITranslatable AppendTooltipAuto(this ITranslatable a, params object[] objects)
        {
            var args = new ITranslatable[objects.Length + 1];
            int i = 1;
            args[0] = a;
            foreach (var obj in objects) { //perf: no LINQ
                args[i++] = Converteer.ToText(obj).TextToTooltip();
            }

            return new ConcatTranslatable(args);
        }

        [Pure]
        public static ITranslatable JoinTexts(this IEnumerable<ITranslatable> a, ITranslatable splitter)
        {
            var builder = FastArrayBuilder<ITranslatable>.Create();
            using (var iter = a.GetEnumerator()) {
                if (iter.MoveNext()) {
                    builder.Add(iter.Current);
                }
                while (iter.MoveNext()) {
                    builder.Add(splitter);
                    builder.Add(iter.Current); //perf:no LINQ
                }
            }
            return new ConcatTranslatable(builder.ToArray());
        }

        [Pure]
        public static ITranslatable LimitLength(this ITranslatable translatable, int maxwidth)
        {
            LimitLengthTranslatable input_as_LL = translatable as LimitLengthTranslatable;
            return input_as_LL != null
                ? (input_as_LL.m_maxwidth < maxwidth ? input_as_LL : new LimitLengthTranslatable(input_as_LL.m_text, maxwidth))
                : new LimitLengthTranslatable(translatable, maxwidth);
        }

        [Pure]
        public static ITranslatable LimitLength(this ITranslatable translatable, int? maxwidth) => maxwidth.HasValue ? LimitLength(translatable, maxwidth.Value) : translatable;

        [Pure]
        public static ITranslatable ForceLanguage(this ITranslatable translatable, Taal taal)
            => (translatable as ForcedLanguageTranslatable) ?? new ForcedLanguageTranslatable(translatable, taal);

        [Pure]
        public static ITranslatable CreateTranslatable(Func<Taal, string> text) => new SingleTranslatable(text);

        [Pure]
        public static ITranslatable CreateTranslatable(Func<Taal, string> text, Func<Taal, string> extratext) => new DoubleTranslatable(text, extratext);

        [Pure]
        public static ITranslatable CreateTranslatable(Func<Taal, TextVal> translator) => new SimpleTranslatable(translator);

        static readonly LiteralTranslatable empty = new LiteralTranslatable("", "", "");
        public static LiteralTranslatable Empty => empty;

        [Pure]
        public static LiteralTranslatable Literal(string nl) => new LiteralTranslatable(nl, null, null);

        [Pure]
        public static LiteralTranslatable Literal(string nl, string en) => new LiteralTranslatable(nl, en, null);

        [Pure]
        public static LiteralTranslatable Literal(string nl, string en, string du) => new LiteralTranslatable(nl, en, du);

        [Pure]
        public static ITranslatable Raw(string text) => Raw(text, null);

        [Pure]
        public static ITranslatable Raw(string text, string extratext) => Raw(TextVal.Create(text, extratext));

        [Pure]
        public static ITranslatable Raw(TextVal tv) => new RawTranslatable(tv);

        [Pure]
        public static ITranslatable ReplaceTooltipWithText(this ITranslatable translatable, ITranslatable tt)
            => CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt.Translate(taal).Text);

        [Pure]
        public static ITranslatable TextToTooltip(this ITranslatable translatable) => CreateTranslatable(taal => "", taal => translatable.Translate(taal).Text);

        [Pure]
        public static ITranslatable TooltipToText(this ITranslatable translatable) => CreateTranslatable(taal => translatable.Translate(taal).ExtraText, taal => "");

        [Pure]
        public static ITranslatable ReplaceTooltipWithTooltip(this ITranslatable translatable, ITranslatable tt)
            => CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt.Translate(taal).ExtraText);

        [Pure]
        public static ITranslatable ReplaceTooltipWithText(this ITranslatable translatable, string tt)
            => CreateTranslatable(taal => translatable.Translate(taal).Text, taal => tt);

        sealed class LimitLengthTranslatable : ITranslatable
        {
            public readonly ITranslatable m_text;
            public readonly int m_maxwidth;

            public LimitLengthTranslatable(ITranslatable text, int maxwidth)
            {
                m_text = text;
                m_maxwidth = maxwidth;
            }

            public string GenerateUid() => m_maxwidth + "<" + m_text.GenerateUid();

            public TextVal Translate(Taal lang)
            {
                TextVal raw = m_text.Translate(lang);
                var shortened = StringMeasurement.LimitTextLength(raw.Text, m_maxwidth);
                return TextVal.Create(shortened.Item1, string.IsNullOrEmpty(raw.ExtraText) && shortened.Item2 ? raw.Text : raw.ExtraText);
            }

            public override string ToString() => GenerateUid();
        }

        sealed class SingleTranslatable : ITranslatable
        {
            readonly Func<Taal, string> stringify;

            public SingleTranslatable(Func<Taal, string> stringify)
            {
                this.stringify = stringify;
            }

            public string GenerateUid() => stringify(Taal.NL);
            public TextVal Translate(Taal lang) => TextVal.Create(stringify(lang));
            public override string ToString() => GenerateUid();
        }

        sealed class DoubleTranslatable : ITranslatable
        {
            readonly Func<Taal, string> textF, extratextF;

            public DoubleTranslatable(Func<Taal, string> textFactory, Func<Taal, string> extratextFactory)
            {
                textF = textFactory;
                extratextF = extratextFactory;
            }

            public string GenerateUid() => Translate(Taal.NL).ToString();
            public TextVal Translate(Taal lang) => TextVal.Create(textF(lang), extratextF(lang));
            public override string ToString() => GenerateUid();
        }

        sealed class SimpleTranslatable : ITranslatable
        {
            readonly Func<Taal, TextVal> translator;

            public SimpleTranslatable(Func<Taal, TextVal> translator)
            {
                this.translator = translator;
            }

            public string GenerateUid() => Translate(Taal.NL).ToString();
            public TextVal Translate(Taal lang) => translator(lang);
            public override string ToString() => GenerateUid();
        }

        sealed class ForcedLanguageTranslatable : ITranslatable
        {
            readonly ITranslatable underlying;
            readonly Taal taal;

            public ForcedLanguageTranslatable(ITranslatable underlying, Taal taal)
            {
                this.underlying = underlying;
                this.taal = taal;
            }

            public string GenerateUid() => underlying.GenerateUid();
            public TextVal Translate(Taal lang) => underlying.Translate(taal);
            public override string ToString() => GenerateUid();
        }

        sealed class RawTranslatable : ITranslatable
        {
            readonly TextVal tv;

            public RawTranslatable(TextVal tv)
            {
                this.tv = tv;
            }

            public string GenerateUid() => "TV:" + tv.Text + "\n" + tv.ExtraText;
            public override string ToString() => GenerateUid();
            public TextVal Translate(Taal taal) => tv;
        }
    }
}
