using System;

namespace ProgressOnderwijsUtils
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpVolgordeAttribute : Attribute
	{
		public MpVolgordeAttribute(int volgorde) { Volgorde = volgorde; }
		public readonly int Volgorde;
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class MpNoLabelsRequiredAttribute : Attribute { }

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

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpSimpleLabelAttribute : Attribute
	{
		public MpSimpleLabelAttribute(string text, string helptext) { Label = new TextDefSimple(text, helptext); }
		public MpSimpleLabelAttribute(string text) { Label = new TextDefSimple(text, ""); }
		public readonly ITranslatable Label;
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpTextDefKeyAttribute : Attribute
	{
		public MpTextDefKeyAttribute(string webmodule, string sleutel) { Label = new TextDefKey(webmodule, sleutel); }
		public readonly ITranslatable Label;
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MpKoppelTabelAttribute : Attribute
	{
		public MpKoppelTabelAttribute(string tabelnaam) { KoppelTabelNaam = tabelnaam; }
		public MpKoppelTabelAttribute() { KoppelTabelNaam = null; } //means "same as property-name"!
		public readonly string KoppelTabelNaam;
	}
}
