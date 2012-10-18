using System;

namespace ProgressOnderwijsUtils
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpVolgordeAttribute : Attribute
	{
		public MpVolgordeAttribute(int volgorde) { Volgorde = volgorde; }
		public readonly int Volgorde;
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpLijstCssAttribute : Attribute
	{
		public MpLijstCssAttribute(string cssClass) { CssClass = cssClass; }
		public readonly string CssClass;
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

		public LiteralTranslatable ToTranslatable(){return Translatable.Literal(NL, EN, DE);}
	}


	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpKoppelTabelAttribute : Attribute
	{
		public MpKoppelTabelAttribute(string tabelnaam) { KoppelTabelNaam = tabelnaam; }
		public MpKoppelTabelAttribute() { KoppelTabelNaam = null; } //means "same as property-name"!
		public readonly string KoppelTabelNaam;
	}
}
