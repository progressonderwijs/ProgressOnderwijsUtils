using System;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpVolgordeAttribute : Attribute
	{
		public MpVolgordeAttribute(int volgorde) { Volgorde = volgorde; }
		public readonly int Volgorde;
	}

	[Flags]
	public enum ColumnCss
	{
		None = 0,
		Fout = 1 << 0,
		AlignRight = 1 << 1,
		AlignCenter = 1 << 2,
		ExplicitLines = 1 << 3,
		ExtraSpaceRight = 1 << 4,
	}
	public static class ColumnCssHelpers
	{
		public static string JoinedClasses(this ColumnCss columnCss, string prefix)
		{
			return EnumHelpers.GetValues<ColumnCss>().Where(css => css != ColumnCss.None && columnCss.HasFlag(css)).Select(css => prefix + css.ToString() + " ").JoinStrings();
		}
	}


	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpColumnCssAttribute : Attribute
	{
		public MpColumnCssAttribute(ColumnCss cssClass) { CssClass = cssClass; }
		public readonly ColumnCss CssClass;
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
	public sealed class MpLabelsRequiredAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class MpNotMappedAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpLengteAttribute : Attribute
	{
		public MpLengteAttribute(int lengte) { Lengte = lengte; }
		public readonly int Lengte;
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpVerplichtAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class HideAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpReadonlyAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpAllowNullAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpShowDefaultOnNewAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpRegexAttribute : Attribute
	{
		public MpRegexAttribute(string regex) { Regex = regex; }
		public readonly string Regex;
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class MpTooltipAttribute : Attribute
	{
		public MpTooltipAttribute(string nl, string en, string de) { NL = nl; EN = en; DE = de; }
		public MpTooltipAttribute(string nl, string en) { NL = nl; EN = en; }
		public MpTooltipAttribute(string nl) { NL = nl; }
		public readonly string NL, EN, DE;
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class MpLabelAttribute : Attribute
	{
		public MpLabelAttribute(string nl, string en, string de) { NL = nl; EN = en; DE = de; }
		public MpLabelAttribute(string nl, string en) { NL = nl; EN = en; }
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
