using System;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    [Flags]
    public enum ColumnCss
    {
        None = 0,
        Fout = 1 << 0,
        AlignRight = 1 << 1,
        AlignCenter = 1 << 2,
        ExplicitLines = 1 << 3,
        ExtraSpaceRight = 1 << 4,
        LimitedWidth = 1 << 5,
    }

    public static class ColumnCssHelpers
    {
        public static string JoinedClasses(this ColumnCss columnCss, string prefix)
        {
            //TODO:optimize: do some perf testing here, this is called very very often.
            return EnumHelpers.GetValues<ColumnCss>().Where(css => css != ColumnCss.None && columnCss.HasFlag(css)).Select(css => prefix + css.ToString() + " ").JoinStrings();
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpColumnCssAttribute : Attribute
    {
        public MpColumnCssAttribute(ColumnCss cssClass) { CssClass = cssClass; }
        public readonly ColumnCss CssClass;
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpHtmlEditModeAttribute : Attribute
    {
        public MpHtmlEditModeAttribute(HtmlEditMode htmlMode) { HtmlMode = htmlMode; }
        public readonly HtmlEditMode HtmlMode;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class MpLabelsRequiredAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class MpNotMappedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpMaxLengthAttribute : Attribute
    {
        public MpMaxLengthAttribute(int maxLength) { MaxLength = maxLength; }
        public readonly int MaxLength;
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpDisplayLengthAttribute : Attribute
    {
        public MpDisplayLengthAttribute(int displayLength) { DisplayLength = displayLength; }
        public readonly int DisplayLength;
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpVerplichtAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class HideAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpReadonlyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpAllowNullInEditorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpShowDefaultOnNewAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpRegexAttribute : Attribute
    {
        public MpRegexAttribute(string regex) { Regex = regex; }
        public readonly string Regex;
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpDatumFormaatAttribute : Attribute
    {
        public MpDatumFormaatAttribute(DatumFormaat formaat) { Formaat = formaat; }
        public readonly DatumFormaat Formaat;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MpTooltipAttribute : Attribute
    {
        public MpTooltipAttribute(string nl, string en, string de)
        {
            NL = nl;
            EN = en;
            DE = de;
        }

        public MpTooltipAttribute(string nl, string en)
        {
            NL = nl;
            EN = en;
        }

        public MpTooltipAttribute(string nl) { NL = nl; }
        public readonly string NL, EN, DE;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MpTooltipUntranslatedAttribute : Attribute
    {
        public MpTooltipUntranslatedAttribute(string tooltip) { Tooltip = tooltip; }
        public readonly string Tooltip;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MpLabelAttribute : Attribute
    {
        public MpLabelAttribute(string nl, string en, string de)
        {
            NL = nl;
            EN = en;
            DE = de;
        }

        public MpLabelAttribute(string nl, string en)
        {
            NL = nl;
            EN = en;
        }

        public MpLabelAttribute(string nl) { NL = nl; }
        public readonly string NL, EN, DE;
        public LiteralTranslatable ToTranslatable() { return Translatable.Literal(NL, EN, DE); }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MpLabelUntranslatedAttribute : Attribute
    {
        public MpLabelUntranslatedAttribute(string label) { Label = label; }
        public readonly string Label;
        public LiteralTranslatable ToTranslatable() { return Translatable.Literal(Label, Label, Label); }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MpKoppelTabelAttribute : Attribute
    {
        public MpKoppelTabelAttribute(string tabelnaam) { KoppelTabelNaam = tabelnaam; }
        public MpKoppelTabelAttribute() { KoppelTabelNaam = null; } //means "same as property-name"!
        public readonly string KoppelTabelNaam;
    }
}
