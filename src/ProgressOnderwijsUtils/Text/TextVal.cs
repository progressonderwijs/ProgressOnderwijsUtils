using System;
using System.Collections.Generic;
using System.Linq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
    public struct TextVal
    {
        readonly string m_text, m_helptext;
        public string Text { get { return m_text; } }
        public string ExtraText { get { return m_helptext; } }
        public static string UndefinedExtraText { get { return "Sorry, this text is not available in your language"; } }
        public bool IsDefined { get { return !(UndefinedExtraText == ExtraText || Text != null && Text.StartsWith("~") && ExtraText != null && ExtraText.StartsWith("~")); } }
        public bool IsEmpty { get { return string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(ExtraText); } }

        /// <summary>
        /// Creates a new fully resolved text with help-text.
        /// </summary>
        /// <param name="text">The primary text to be shown</param>
        /// <param name="helptext">Any additional text to potentially be shown in a tooltip.</param>
        public TextVal(string text, string helptext)
        {
            m_text = text;
            m_helptext = helptext;
        }

        public static TextVal CreateUndefined(string hint) { return new TextVal("ONVERTAALD: " + hint, UndefinedExtraText); }
        public static bool operator ==(TextVal a, TextVal b) { return a.m_text == b.m_text && a.m_helptext == b.m_helptext; }
        public static bool operator !=(TextVal a, TextVal b) { return !(a == b); }
        public override bool Equals(object obj) { return obj != null && obj is TextVal && this == (TextVal)obj; }
        public override int GetHashCode() { return (m_text == null ? 0 : m_text.GetHashCode()) + (m_helptext == null ? 0 : 137 * m_helptext.GetHashCode()); }
        public override string ToString() { return "\"" + Text + "\"/\"" + ExtraText + "\""; }
        public static TextVal Create(string text, string helptext) { return new TextVal(text, helptext); }
        public static TextVal Create(string text) { return new TextVal(text, ""); }
        public static TextVal EmptyText { get { return new TextVal("", ""); } }
    }
}
