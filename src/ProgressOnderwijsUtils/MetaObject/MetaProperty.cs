﻿//#define ExtraTyped
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
#if ExtraTyped
		Func<TOwner, TProperty> GetTyped { get; }
		Action<TOwner, TProperty> SetTyped { get; }
#endif
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

			public readonly MemberInfo memberInfo;
			public MemberInfo MemberInfo { get { return memberInfo; } }
			public Expression GetterExpression(Expression paramExpr)
			{
				return
					memberInfo is PropertyInfo ? Expression.Property(paramExpr, (PropertyInfo)memberInfo)
					: Expression.Field(paramExpr, (FieldInfo)memberInfo);
			}

			public readonly bool isKey;
			public bool IsKey { get { return isKey; } }

			static T MkDel<T>(MethodInfo mi) { return (T)(object)Delegate.CreateDelegate(typeof(T), mi); }
			public Impl(PropertyInfo pi, int implicitOrder)
			{
				if (pi.PropertyType != typeof(TProperty))
					throw new InvalidOperationException("Cannot initialize metaproperty: type mismatch.");
				memberInfo = pi;
				dataType = pi.PropertyType;

				ParameterExpression typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				MemberExpression typedPropExpr = Expression.Property(typedParamExpr, pi);

#if ExtraTyped
				bool canCallTypedDirectly = !typeof(TOwner).IsValueType;
				getTyped = canCallTypedDirectly ? MkDel<Func<TOwner, TProperty>>(pi.GetGetMethod()) : Expression.Lambda<Func<TOwner, TProperty>>(typedPropExpr, typedParamExpr).Compile();
#endif

				bool canCallDirectly = !(typeof(TOwner).IsValueType || pi.PropertyType.IsValueType);
				getter =
					canCallDirectly ? MkDel<Func<TOwner, object>>(pi.GetGetMethod()) :
					Expression.Lambda<Func<TOwner, object>>(Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr).Compile();


				untypedGetter = o => getter((TOwner)o);


				var valParamExpr = Expression.Parameter(typeof(object), "newValue");


				bool cannotWrite = !pi.CanWrite || pi.GetSetMethod() == null;

#if ExtraTyped
				var typedValParamExpr = Expression.Parameter(typeof(TProperty), "newValue");
				setTyped = cannotWrite ? null :
						canCallDirectly ? MkDel<Action<TOwner, TProperty>>(pi.GetSetMethod())
						: Expression.Lambda<Action<TOwner, TProperty>>(
																			Expression.Assign(typedPropExpr, typedValParamExpr)
																			, typedParamExpr, typedValParamExpr
																			).Compile();
#endif


				setter = cannotWrite ? default(Action<TOwner, object>) :
																		Expression.Lambda<Action<TOwner, object>>(
																			Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)
																				), typedParamExpr, valParamExpr
																			).Compile();

				untypedSetter = cannotWrite ? default(Action<object, object>) : (o, v) => setter((TOwner)o, v);



				name = pi.Name;
				var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(pi);
				volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;

				var labelNoTt = OrDefault(Attr<MpLabelAttribute>(pi), mkAttr => mkAttr.ToTranslatable());
				var untranslatedLabelNoTt = OrDefault(Attr<MpLabelUntranslatedAttribute>(pi), mkAttr => mkAttr.ToTranslatable());
				if (untranslatedLabelNoTt != null)
				{
					if (labelNoTt != null)
						throw new Exception("Cannot define both an untranslated and a translated label on the same property " + ObjectToCode.GetCSharpFriendlyTypeName(pi.DeclaringType) + "." + Name);
					else
						labelNoTt = untranslatedLabelNoTt;
				}

				if (labelNoTt == null && Attr<MpLabelsRequiredAttribute>(pi.DeclaringType) != null)
					throw new ArgumentException("You must specify an MpLabel on " + Name + ", since the class " + ObjectToCode.GetCSharpFriendlyTypeName(pi.DeclaringType) + " is marked MpLabelsRequired");
				if (labelNoTt == null)
				{
					var prettyName = StringUtils.PrettyCapitalizedPrintCamelCased(pi.Name);
					labelNoTt = Translatable.Literal(prettyName, prettyName, prettyName);
				}
				label = OrDefault(Attr<MpTooltipAttribute>(pi), mkAttr => labelNoTt.WithTooltip(mkAttr.NL, mkAttr.EN, mkAttr.DE), labelNoTt);

				koppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(pi), mkAttr => mkAttr.KoppelTabelNaam ?? pi.Name);
				lijstCssClass = OrDefault(Attr<MpColumnCssAttribute>(pi), mkAttr => mkAttr.CssClass);
				required = OrDefault(Attr<MpVerplichtAttribute>(pi), mkAttr => true);
				hide = OrDefault(Attr<HideAttribute>(pi), mkAttr => true);
				allowNull = OrDefault(Attr<MpAllowNullAttribute>(pi), mkAttr => true);
				isKey = OrDefault(Attr<KeyAttribute>(pi), mkAttr => true);
				showDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(pi), mkAttr => true);
				isReadonly = UntypedSetter == null || OrDefault(Attr<MpReadonlyAttribute>(pi), mkAttr => true);
				length = OrDefault(Attr<MpLengteAttribute>(pi), mkAttr => mkAttr.Lengte, default(int?));
				regex = OrDefault(Attr<MpRegexAttribute>(pi), mkAttr => mkAttr.Regex);
				datumtijd = OrDefault(Attr<MpDatumFormaatAttribute>(pi), mkAttr => mkAttr.Formaat, default(DatumFormaat?));

				if (KoppelTabelNaam != null && DataType.GetNonNullableUnderlyingType() != typeof(int))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Name + " heeft koppeltabel " + KoppelTabelNaam + " maar is van type " + DataType + "!");
			}

			public bool CanRead { get { return untypedGetter != null; } }

			readonly Func<object, object> untypedGetter;
			public Func<object, object> UntypedGetter { get { return untypedGetter; } }
			readonly Action<object, object> untypedSetter;
			public Action<object, object> UntypedSetter { get { return untypedSetter; } }


			readonly Func<TOwner, object> getter;
			public Func<TOwner, object> Getter { get { return getter; } }
			readonly Action<TOwner, object> setter;
			public Action<TOwner, object> Setter { get { return setter; } }

#if	 ExtraTyped
			readonly Func<TOwner, TProperty> getTyped;
			public Func<TOwner, TProperty> GetTyped { get { return getTyped; } }
			readonly Action<TOwner, TProperty> setTyped;
			public Action<TOwner, TProperty> SetTyped { get { return setTyped; } }
#endif

		}

		static T Attr<T>(MemberInfo mi) where T : Attribute { return mi.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault(); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }
	}
}