using System;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public sealed class MetaProperty
	{
		public readonly string Naam;
		public readonly IPropertyAccessors Accessors;
		public readonly int Volgorde;
		public readonly bool Verplicht;
		public readonly bool AllowNull;
		public readonly int Lengte;
		public readonly string Regex;
		public readonly ITranslatable Label;
		public readonly string KoppelTabelNaam;
		public readonly bool IsReadonly;
		public readonly bool ShowDefaultOnNew;
		public Type DataType { get { return Accessors.PropertyType; } }


		public MetaProperty(IPropertyAccessors accessors, string naam, int volgorde, ITranslatable label, string koppelTabelNaam, bool verplicht, bool allowNull, bool isReadonly, int lengte, string regex, bool showDefaultOnNew)
		{
			Accessors = accessors;
			Verplicht = verplicht;
			AllowNull = allowNull;
			Naam = naam;
			Volgorde = volgorde;
			Lengte = lengte;
			Regex = regex;
			Label = label;
			KoppelTabelNaam = koppelTabelNaam;
			IsReadonly = isReadonly || accessors.Setter == null;
			ShowDefaultOnNew = showDefaultOnNew;
		}

		MetaProperty(PropertyInfo pi, int implicitOrder)
		{
			Accessors = GetAccessors(pi);
			Naam = pi.Name;
			var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(pi);
			Volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;
			Label = new[] {
					OrDefault(Attr<MpSimpleLabelAttribute>(pi), mkAttr => mkAttr.Label),
					OrDefault(Attr<MpTextDefKeyAttribute>(pi), mkAttr => mkAttr.Label) }
				.SingleOrDefault(label => label != null);
			if (Label == null && Attr<MpNoLabelsRequiredAttribute>(pi.DeclaringType) == null)
				throw new ArgumentException("You must specify an MpSimpleLabel or MpTextDefKey on " + Naam + ", or the class " + ObjectToCode.GetCSharpFriendlyTypeName(pi.DeclaringType) + " must be marked MpNoLabelsRequired");
			KoppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(pi), mkAttr => mkAttr.KoppelTabelNaam ?? pi.Name);
			Verplicht = OrDefault(Attr<MpVerplichtAttribute>(pi), mkAttr => true);
			AllowNull = OrDefault(Attr<MpAllowNullAttribute>(pi), mkAttr => true);
			ShowDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(pi), mkAttr => true);
			IsReadonly = Accessors.Setter == null || OrDefault(Attr<MpReadonlyAttribute>(pi), mkAttr => true);
			Lengte = OrDefault(Attr<MpLengteAttribute>(pi), mkAttr => mkAttr.Lengte);
			Regex = OrDefault(Attr<MpRegexAttribute>(pi), mkAttr => mkAttr.Regex);

			if (KoppelTabelNaam != null && DataType != typeof(int) && DataType != typeof(int?))
				throw new ProgressNetException(Accessors.OwnerType + " heeft Kolom " + Naam + " heeft koppeltabel " + KoppelTabelNaam + " maar is van type " + DataType + "!");
		}

		public static MetaProperty LoadIfMetaProperty(PropertyInfo pi, int implicitOrder)
		{
			return Attr<MpNotMappedAttribute>(pi) != null ? null : new MetaProperty(pi, implicitOrder);
		}

		static T Attr<T>(MemberInfo mi) where T : Attribute { return mi.GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault(); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }

		static IPropertyAccessors GetAccessors(PropertyInfo pi)
		{
			return (IPropertyAccessors)Activator.CreateInstance(typeof(MetaPropertyAccessors<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType), pi);
		}

		public bool CanRead { get { return Accessors.Getter != null; } }
	}
}