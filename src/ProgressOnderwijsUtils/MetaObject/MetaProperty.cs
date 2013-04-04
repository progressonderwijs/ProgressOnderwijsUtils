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
		MemberInfo MemberInfo { get; }
		Expression GetterExpression(Expression paramExpr);
	}


	public interface IMetaProperty<in TOwner> : IMetaProperty
	{
		Func<TOwner, object> Getter { get; }
		Action<TOwner, object> Setter { get; }
	}


	public interface IMetaProperty<in TOwner, TProperty> : IMetaProperty<TOwner>
	{
		Func<TOwner, TProperty> GetTyped { get; }
		Action<TOwner, TProperty> SetTyped { get; }
	}


	public static class MetaProperty
	{
		public sealed class Impl<TOwner, TProperty> : IMetaProperty<TOwner, TProperty>
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
			readonly Type dataType;
			public Type DataType { get { return dataType; } }
			readonly MemberInfo memberInfo;
			public MemberInfo MemberInfo { get { return memberInfo; } }
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
			readonly Func<TOwner, TProperty> getTyped;
			public Func<TOwner, TProperty> GetTyped { get { return getTyped; } }
			readonly Action<TOwner, TProperty> setTyped;
			public Action<TOwner, TProperty> SetTyped { get { return setTyped; } }

			public Expression GetterExpression(Expression paramExpr)
			{
				return
					memberInfo is PropertyInfo ? Expression.Property(paramExpr, (PropertyInfo)memberInfo)
						: Expression.Field(paramExpr, (FieldInfo)memberInfo);
			}

			Impl(MemberInfo mi, int implicitOrder, Type memberType)
			{
				if (memberType != typeof(TProperty))
					throw new InvalidOperationException("Cannot initialize metaproperty: type mismatch.");
				memberInfo = mi;
				dataType = memberType;
				name = mi.Name;
				var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(memberInfo);
				volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;

				var labelNoTt = LabelNoTt(memberInfo);
				label = OrDefault(Attr<MpTooltipAttribute>(memberInfo)
					, mkAttr => labelNoTt.WithTooltip(mkAttr.NL, mkAttr.EN, mkAttr.DE)
					, labelNoTt);

				koppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(memberInfo),
					mkAttr => mkAttr.KoppelTabelNaam ?? memberInfo.Name);
				lijstCssClass = OrDefault(Attr<MpColumnCssAttribute>(memberInfo), mkAttr => mkAttr.CssClass);
				required = OrDefault(Attr<MpVerplichtAttribute>(memberInfo), mkAttr => true);
				hide = OrDefault(Attr<HideAttribute>(memberInfo), mkAttr => true);
				allowNull = OrDefault(Attr<MpAllowNullAttribute>(memberInfo), mkAttr => true);
				isKey = OrDefault(Attr<KeyAttribute>(memberInfo), mkAttr => true);
				showDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(memberInfo), mkAttr => true);
				isReadonly = UntypedSetter == null || OrDefault(Attr<MpReadonlyAttribute>(memberInfo), mkAttr => true);
				length = OrDefault(Attr<MpLengteAttribute>(memberInfo), mkAttr => mkAttr.Lengte, default(int?));
				regex = OrDefault(Attr<MpRegexAttribute>(memberInfo), mkAttr => mkAttr.Regex);
				datumtijd = OrDefault(Attr<MpDatumFormaatAttribute>(memberInfo), mkAttr => mkAttr.Formaat, default(DatumFormaat?));

				if (KoppelTabelNaam != null && DataType.GetNonNullableUnderlyingType() != typeof(int))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Name + " heeft koppeltabel " +
						KoppelTabelNaam + " maar is van type " + DataType + "!");

			}

			public Impl(PropertyInfo pi, int implicitOrder)
				: this(pi, implicitOrder, pi.PropertyType)
			{

				var getterMethod = pi.GetGetMethod();

				getTyped = MkTypedGetter(pi, getterMethod);
				getter = MkGetter(pi, getterMethod);
				untypedGetter = !(getterMethod != null) ? default(Func<object, object>) : o => getter((TOwner)o);



				var setterMethod = pi.GetSetMethod();
			setTyped = MkTypedSetter(pi, setterMethod);
				setter = MkSetter(pi, setterMethod);
				untypedSetter = setterMethod == null ? default(Action<object, object>) : (o, v) => setter((TOwner)o, v);
			}

			public Impl(FieldInfo fi, int implicitOrder)
				: this(fi, implicitOrder, fi.FieldType)
			{
				var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				var typedPropExpr = Expression.Field(typedParamExpr, fi);

				getTyped = Expression.Lambda<Func<TOwner, TProperty>>(typedPropExpr, typedParamExpr).Compile();
				getter = getTyped as Func<TOwner, object> ?? Expression.Lambda<Func<TOwner, object>>(Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr).Compile();
				untypedGetter = o => getTyped((TOwner)o);

				if (fi.IsInitOnly)
				{
					setTyped = null;
					setter = null;
					untypedSetter = null;
				}
				else
				{
					var typedValParamExpr = Expression.Parameter(typeof(TProperty), "newValue");
					var valParamExpr = Expression.Parameter(typeof(object), "newValue");

					setTyped = Expression.Lambda<Action<TOwner, TProperty>>(
								Expression.Assign(typedPropExpr, typedValParamExpr),
								typedParamExpr, typedValParamExpr
								).Compile();
					setter = Expression.Lambda<Action<TOwner, object>>(
								Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, typeof(TProperty))),
								typedParamExpr, valParamExpr
								).Compile();
					untypedSetter = (o, v) => setTyped((TOwner)o, (TProperty)v);
				}
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

			static Action<TOwner, object> MkSetter(PropertyInfo pi, MethodInfo setterMethod)
			{
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

			static Action<TOwner, TProperty> MkTypedSetter(PropertyInfo pi, MethodInfo setterMethod)
			{
				if (setterMethod == null)
					return null;
				else if (typeof(TOwner).IsValueType)
				{ //cannot call directly; value type methods pass self by reference.
					var typedValParamExpr = Expression.Parameter(typeof(TProperty), "newValue");
					var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
					var typedPropExpr = Expression.Property(typedParamExpr, pi);

					return
						Expression.Lambda<Action<TOwner, TProperty>>(
							Expression.Assign(typedPropExpr, typedValParamExpr),
							typedParamExpr, typedValParamExpr
							).Compile();
				}
				else
					return MkDel<Action<TOwner, TProperty>>(setterMethod);
			}

			static Func<TOwner, TProperty> MkTypedGetter(PropertyInfo pi, MethodInfo getterMethod)
			{

				if (getterMethod == null)
					return null;
				else if (!typeof(TOwner).IsValueType)
					return MkDel<Func<TOwner, TProperty>>(getterMethod);

				// direct call not allowed; value type methods pass self by reference.
				var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				var typedPropExpr = Expression.Property(typedParamExpr, pi);
				return Expression.Lambda<Func<TOwner, TProperty>>(typedPropExpr, typedParamExpr).Compile();
			}

			static Func<TOwner, object> MkGetter(PropertyInfo pi, MethodInfo getterMethod)
			{
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