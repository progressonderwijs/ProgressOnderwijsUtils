namespace ProgressOnderwijsUtils
{
    public struct TextVal
    {
        public string Text { get; }
        public string ExtraText { get; }
        public static string UndefinedExtraText => "Sorry, this text is not available in your language";
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

        public static TextVal CreateUndefined(string hint) => new TextVal("ONVERTAALD: " + hint, UndefinedExtraText);
        public static bool operator ==(TextVal a, TextVal b) { return a.Text == b.Text && a.ExtraText == b.ExtraText; }
        public static bool operator !=(TextVal a, TextVal b) { return !(a == b); }
        public override bool Equals(object obj) => obj is TextVal && this == (TextVal)obj;
        public override int GetHashCode() => (Text?.GetHashCode() ?? 0) + (137 * ExtraText?.GetHashCode() ?? 0);
        public override string ToString() => "\"" + Text + "\"/\"" + ExtraText + "\"";
        public static TextVal Create(string text, string helptext) => new TextVal(text, helptext);
        public static TextVal Create(string text) => new TextVal(text, "");
    }
}
