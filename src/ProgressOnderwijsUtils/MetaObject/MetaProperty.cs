using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public interface IMetaProperty : IColumnDefinition
	{
		ColumnCss LijstCssClass { get; }
		Func<object, object> UntypedGetter { get; }
		Action<object, object> UntypedSetter { get; }
		int Volgorde { get; }
		bool Required { get; }
		bool AllowNull { get; }
		int? Length { get; }
		string Regex { get; }
		DatumFormaat? DatumTijd { get; }
		ITranslatable Label { get; }
		string KoppelTabelNaam { get; }
		bool IsReadonly { get; }
		bool IsKey { get; }
		bool Hide { get; }
		bool ShowDefaultOnNew { get; }
		bool CanRead { get; }
		PropertyInfo PropertyInfo { get; }
		Expression GetterExpression(Expression paramExpr);
	}


	public interface IMetaProperty<in TOwner> : IMetaProperty
	{
		Func<TOwner, object> Getter { get; }
		Action<TOwner, object> Setter { get; }
	}

	public static class MetaProperty
	{
		public sealed class Impl<TOwner> : IMetaProperty<TOwner>
		{
			readonly string name;
			public string Name { get { return name; } }
			readonly ColumnCss lijstCssClass;
			public ColumnCss LijstCssClass { get { return lijstCssClass; } }
			readonly int volgorde;
			public int Volgorde { get { return volgorde; } }
			readonly bool required;
			public bool Required { get { return required; } }
			readonly bool hide;
			public bool Hide { get { return hide; } }
			readonly bool allowNull;
			public bool AllowNull { get { return allowNull; } }
			readonly int? length;
			public int? Length { get { return length; } }
			readonly string regex;
			public string Regex { get { return regex; } }
			readonly DatumFormaat? datumtijd;
			public DatumFormaat? DatumTijd { get { return datumtijd; } }
			readonly ITranslatable label;
			public ITranslatable Label { get { return label; } }
			readonly string koppelTabelNaam;
			public string KoppelTabelNaam { get { return koppelTabelNaam; } }
			readonly bool isReadonly;
			public bool IsReadonly { get { return isReadonly; } }
			readonly bool showDefaultOnNew;
			public bool ShowDefaultOnNew { get { return showDefaultOnNew; } }
			public Type DataType { get { return propertyInfo.PropertyType; } }
			readonly PropertyInfo propertyInfo;
			public PropertyInfo PropertyInfo { get { return propertyInfo; } }
			readonly bool isKey;
			public bool IsKey { get { return isKey; } }

			public bool CanRead { get { return untypedGetter != null; } }
			readonly Func<object, object> untypedGetter;
			public Func<object, object> UntypedGetter { get { return untypedGetter; } }
			readonly Action<object, object> untypedSetter;
			public Action<object, object> UntypedSetter { get { return untypedSetter; } }
			readonly Func<TOwner, object> getter;
			public Func<TOwner, object> Getter { get { return getter; } }
			readonly Action<TOwner, object> setter;
			public Action<TOwner, object> Setter { get { return setter; } }

			public Expression GetterExpression(Expression paramExpr)
			{
				return Expression.Property(paramExpr, propertyInfo);
			}

			public Impl(PropertyInfo pi, int implicitOrder)
			{
				propertyInfo = pi;
				name = pi.Name;
				var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(propertyInfo);
				volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;

				var labelNoTt = LabelNoTt(propertyInfo);
				label = OrDefault(Attr<MpTooltipAttribute>(propertyInfo)
					, mkAttr => labelNoTt.WithTooltip(mkAttr.NL, mkAttr.EN, mkAttr.DE)
					, labelNoTt);

				koppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(propertyInfo),
					mkAttr => mkAttr.KoppelTabelNaam ?? propertyInfo.Name);
				lijstCssClass = OrDefault(Attr<MpColumnCssAttribute>(propertyInfo), mkAttr => mkAttr.CssClass);
				required = OrDefault(Attr<MpVerplichtAttribute>(propertyInfo), mkAttr => true);
				hide = OrDefault(Attr<HideAttribute>(propertyInfo), mkAttr => true);
				allowNull = OrDefault(Attr<MpAllowNullAttribute>(propertyInfo), mkAttr => true);
				isKey = OrDefault(Attr<KeyAttribute>(propertyInfo), mkAttr => true);
				showDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(propertyInfo), mkAttr => true);
				isReadonly = UntypedSetter == null || OrDefault(Attr<MpReadonlyAttribute>(propertyInfo), mkAttr => true);
				length = OrDefault(Attr<MpLengteAttribute>(propertyInfo), mkAttr => mkAttr.Lengte, default(int?));
				regex = OrDefault(Attr<MpRegexAttribute>(propertyInfo), mkAttr => mkAttr.Regex);
				datumtijd = OrDefault(Attr<MpDatumFormaatAttribute>(propertyInfo), mkAttr => mkAttr.Formaat, default(DatumFormaat?));

				if (KoppelTabelNaam != null && DataType.GetNonNullableUnderlyingType() != typeof(int))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Name + " heeft koppeltabel " +
						KoppelTabelNaam + " maar is van type " + DataType + "!");

				getter = MkGetter(pi);
				untypedGetter = getter == null ? default(Func<object, object>) : o => getter((TOwner)o);

				setter = MkSetter(pi);
				untypedSetter = setter == null ? default(Action<object, object>) : (o, v) => setter((TOwner)o, v);
			}




			LiteralTranslatable LabelNoTt(MemberInfo memberInfo)
			{
				var labelNoTt = OrDefault(Attr<MpLabelAttribute>(memberInfo), mkAttr => mkAttr.ToTranslatable());
				var untranslatedLabelNoTt = OrDefault(Attr<MpLabelUntranslatedAttribute>(memberInfo),
					mkAttr => mkAttr.ToTranslatable());
				if (untranslatedLabelNoTt != null)
					if (labelNoTt != null)
						throw new Exception(
							"Cannot define both an untranslated and a translated label on the same property " +
								ObjectToCode.GetCSharpFriendlyTypeName(memberInfo.DeclaringType) + "." + Name);
					else
						labelNoTt = untranslatedLabelNoTt;

				if (labelNoTt == null && Attr<MpLabelsRequiredAttribute>(memberInfo.DeclaringType) != null)
					throw new ArgumentException("You must specify an MpLabel on " + Name + ", since the class " +
						ObjectToCode.GetCSharpFriendlyTypeName(memberInfo.DeclaringType) + " is marked MpLabelsRequired");
				if (labelNoTt == null)
				{
					var prettyName = StringUtils.PrettyCapitalizedPrintCamelCased(memberInfo.Name);
					labelNoTt = Translatable.Literal(prettyName, prettyName, prettyName);
				}
				return labelNoTt;
			}

			static Action<TOwner, object> MkSetter(PropertyInfo pi)
			{
				var setterMethod = pi.GetSetMethod();
				if (setterMethod == null)
					return null;
				var valParamExpr = Expression.Parameter(typeof(object), "newValue");
				var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				var typedPropExpr = Expression.Property(typedParamExpr, pi);

				return Expression.Lambda<Action<TOwner, object>>(
						Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)),
						typedParamExpr, valParamExpr
						).Compile();
			}

			static Func<TOwner, object> MkGetter(PropertyInfo pi)
			{
				var getterMethod = pi.GetGetMethod();
				if (getterMethod == null)
					return null;
				else if (!typeof(TOwner).IsValueType && !pi.PropertyType.IsValueType)
					return MkDel<Func<TOwner, object>>(getterMethod);

				var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				var typedPropExpr = Expression.Property(typedParamExpr, pi);
				return Expression.Lambda<Func<TOwner, object>>(
						Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr
						).Compile();
			}
		}

		static T MkDel<T>(MethodInfo mi) { return (T)(object)Delegate.CreateDelegate(typeof(T), mi); }
		static T Attr<T>(MemberInfo mi) where T : Attribute { return mi.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault(); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }
	}
}