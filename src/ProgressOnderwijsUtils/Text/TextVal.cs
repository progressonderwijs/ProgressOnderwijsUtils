using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public struct TextVal
    {
        public string Text { get; }
        public string ExtraText { get; }
        public static string UndefinedExtraText => "Sorry, this text is not available in your language";
        [CodeDieAlleenWordtGebruiktInTests]
        public bool IsDefined => !(UndefinedExtraText == ExtraText || Text != null && Text.StartsWith("~") && ExtraText != null && ExtraText.StartsWith("~"));
        public bool IsEmpty => string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(ExtraText);

        /// <summary>
        /// Creates a new fully resolved text with help-text.
        /// </summary>
        /// <param name="text">The primary text to be shown</param>
        /// <param name="helptext">Any additional text to potentially be shown in a tooltip.</param>
        public TextVal(string text, string helptext)
        {
            Text = text;
            ExtraText = helptext;
        }

        [Pure]
        public static TextVal CreateUndefined(string hint) => new TextVal("ONVERTAALD: " + hint, UndefinedExtraText);

        [Pure]
        public static bool operator ==(TextVal a, TextVal b)
        {
            return a.Text == b.Text && a.ExtraText == b.ExtraText;
        }

        [Pure]
        public static bool operator !=(TextVal a, TextVal b)
        {
            return !(a == b);
        }

        [Pure]
        public override bool Equals(object obj) => obj is TextVal && this == (TextVal)obj;

        [Pure]
        public override int GetHashCode() => (Text?.GetHashCode() ?? 0) + (137 * ExtraText?.GetHashCode() ?? 0);

        [Pure]
        public override string ToString() => "\"" + Text + "\"/\"" + ExtraText + "\"";

        [Pure]
        public static TextVal Create(string text, string helptext) => new TextVal(text, helptext);

        [Pure]
        public static TextVal Create(string text) => new TextVal(text, "");
    }
}
