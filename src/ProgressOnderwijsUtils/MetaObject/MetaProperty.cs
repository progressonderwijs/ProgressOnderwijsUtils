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
		int Index { get; }
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
		bool CanWrite { get; }
		PropertyInfo PropertyInfo { get; }
		Expression GetterExpression(Expression paramExpr);
		HtmlEditMode HtmlMode { get; }
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
			readonly HtmlEditMode htmlMode;
			public HtmlEditMode HtmlMode { get { return htmlMode; } }
			readonly int index;
			public int Index { get { return index; } }
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
			ITranslatable label;
			public ITranslatable Label
			{
				get
				{
					if (label == null)
					{
						var labelNoTt = LabelNoTt(propertyInfo);
						label = OrDefault(propertyInfo.Attr<MpTooltipAttribute>()
							, mkAttr => labelNoTt.WithTooltip(mkAttr.NL, mkAttr.EN, mkAttr.DE)
							, labelNoTt);
					}
					return label;
				}
			}

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

			public bool CanRead
			{
				get
				{
					return getter == UninitializedTGetter
						? propertyInfo.GetGetMethod() != null
						: getter != null;
				}
			}
			public bool CanWrite
			{
				get
				{
					return setter == UninitializedTSetter
						? propertyInfo.GetSetMethod() != null
						: setter != null;
				}
			}

			Func<TOwner, object> getter = UninitializedTGetter;
			public Func<TOwner, object> Getter
			{
				get
				{
					if (getter == UninitializedTGetter)
						getter = MkGetter(propertyInfo);
					return getter;
				}
			}

			Action<TOwner, object> setter = UninitializedTSetter;
			public Action<TOwner, object> Setter
			{
				get
				{
					if (setter == UninitializedTSetter)
						setter = MkSetter(propertyInfo);
					return setter;
				}
			}

			Func<object, object> untypedGetter = UninitializedGetter;
			public Func<object, object> UntypedGetter
			{
				get
				{
					if (untypedGetter == UninitializedGetter)
					{
						var localGetter = Getter;
						untypedGetter = localGetter == null ? default(Func<object, object>) : o => localGetter((TOwner)o);
					}
					return untypedGetter;
				}
			}

			Action<object, object> untypedSetter = UninitializedSetter;
			public Action<object, object> UntypedSetter
			{
				get
				{
					if (untypedSetter == UninitializedSetter)
					{
						var localSetter = Setter;
						untypedSetter = localSetter == null ? default(Action<object, object>) : (o, v) => localSetter((TOwner)o, v);
					}
					return untypedSetter;
				}
			}

			public Expression GetterExpression(Expression paramExpr)
			{
				return Expression.Property(paramExpr, propertyInfo);
			}

			public Impl(PropertyInfo pi, int implicitOrder)
			{
				// first define the getters/setters, they are used below
				//getter = MkGetter(pi);
				//untypedGetter = getter == null ? default(Func<object, object>) : o => getter((TOwner)o);

				//setter = MkSetter(pi);
				//untypedSetter = setter == null ? default(Action<object, object>) : (o, v) => setter((TOwner)o, v);

				propertyInfo = pi;
				name = pi.Name;
				index = implicitOrder;


				koppelTabelNaam = OrDefault(propertyInfo.Attr<MpKoppelTabelAttribute>(),
					mkAttr => mkAttr.KoppelTabelNaam ?? propertyInfo.Name);
				lijstCssClass = OrDefault(propertyInfo.Attr<MpColumnCssAttribute>(), mkAttr => mkAttr.CssClass);
				htmlMode = OrDefault(propertyInfo.Attr<MpHtmlEditModeAttribute>(), mkAttr => mkAttr.HtmlMode);
				required = OrDefault(propertyInfo.Attr<MpVerplichtAttribute>(), mkAttr => true);
				hide = OrDefault(propertyInfo.Attr<HideAttribute>(), mkAttr => true);
				allowNull = OrDefault(propertyInfo.Attr<MpAllowNullAttribute>(), mkAttr => true);
				isKey = OrDefault(propertyInfo.Attr<KeyAttribute>(), mkAttr => true);
				showDefaultOnNew = OrDefault(propertyInfo.Attr<MpShowDefaultOnNewAttribute>(), mkAttr => true);
				isReadonly = !pi.CanWrite || OrDefault(propertyInfo.Attr<MpReadonlyAttribute>(), mkAttr => true);
				length = OrDefault(propertyInfo.Attr<MpLengteAttribute>(), mkAttr => mkAttr.Lengte, default(int?));
				regex = OrDefault(propertyInfo.Attr<MpRegexAttribute>(), mkAttr => mkAttr.Regex);
				datumtijd = OrDefault(propertyInfo.Attr<MpDatumFormaatAttribute>(), mkAttr => mkAttr.Formaat, default(DatumFormaat?));

				if (KoppelTabelNaam != null && DataType.GetNonNullableUnderlyingType() != typeof(int))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Name + " heeft koppeltabel " +
						KoppelTabelNaam + " maar is van type " + DataType + "!");

			}

			public override string ToString()
			{
				return ObjectToCode.GetCSharpFriendlyTypeName(typeof(TOwner)) + "." + name;
			}


			LiteralTranslatable LabelNoTt(MemberInfo memberInfo)
			{
				var labelNoTt = OrDefault(memberInfo.Attr<MpLabelAttribute>(), mkAttr => mkAttr.ToTranslatable());
				var untranslatedLabelNoTt = OrDefault(memberInfo.Attr<MpLabelUntranslatedAttribute>(),
					mkAttr => mkAttr.ToTranslatable());
				if (untranslatedLabelNoTt != null)
					if (labelNoTt != null)
						throw new Exception(
							"Cannot define both an untranslated and a translated label on the same property " +
								ObjectToCode.GetCSharpFriendlyTypeName(memberInfo.DeclaringType) + "." + Name);
					else
						labelNoTt = untranslatedLabelNoTt;

				if (labelNoTt == null && memberInfo.DeclaringType.Attr<MpLabelsRequiredAttribute>() != null)
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
				if (typeof(TOwner).IsValueType)
					return (Action<TOwner, object>)StructSetterM().MakeGenericMethod(pi.PropertyType).Invoke(null, new[] { setterMethod });
				else
					return (Action<TOwner, object>)ClassSetterM().MakeGenericMethod(pi.PropertyType).Invoke(null, new[] { setterMethod });

				//faster code, slower startup:				
				//var valParamExpr = Expression.Parameter(typeof(object), "newValue");
				//var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				//var typedPropExpr = Expression.Property(typedParamExpr, pi);

				//return Expression.Lambda<Action<TOwner, object>>(
				//		Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)),
				//		typedParamExpr, valParamExpr
				//		).Compile();
			}

			static Func<TOwner, object> MkGetter(PropertyInfo pi)
			{
				var getterMethod = pi.GetGetMethod();
				if (getterMethod == null)
					return null;
				else if (pi.PropertyType.IsValueType)
				{
					if (typeof(TOwner).IsValueType)
						return (Func<TOwner, object>)StructStructGetterM().MakeGenericMethod(pi.PropertyType).Invoke(null, new[] { getterMethod });
					else
						return (Func<TOwner, object>)ClassStructGetterM().MakeGenericMethod(pi.PropertyType).Invoke(null, new[] { getterMethod });
				}
				else
				{
					if (typeof(TOwner).IsValueType)
						return StructClassGetter(getterMethod);
					else
						return MkDelegate<Func<TOwner, object>>(getterMethod);
				}

				//faster code, slower startup:				
				//var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				//var typedPropExpr = Expression.Property(typedParamExpr, pi);
				//return Expression.Lambda<Func<TOwner, object>>(
				//		Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr
				//		).Compile();
			}

			static MethodInfo MkGenGetter(Func<MethodInfo, Func<TOwner, object>> f) { return f.Method.GetGenericMethodDefinition(); }
			static MethodInfo MkGenSetter(Func<MethodInfo, Action<TOwner, object>> f) { return f.Method.GetGenericMethodDefinition(); }

			static MethodInfo ClassStructGetterM() { return classStructGetterM ?? (classStructGetterM = MkGenGetter(ClassStructGetter<object>)); }

			static MethodInfo StructStructGetterM() { return structStructGetterM ?? (structStructGetterM = MkGenGetter(StructStructGetter<object>)); }
			static MethodInfo ClassSetterM() { return classSetterM ?? (classSetterM = MkGenSetter(ClassSetter<object>)); }
			static MethodInfo StructSetterM() { return structSetterM ?? (structSetterM = MkGenSetter(StructSetter<object>)); }


			static MethodInfo classStructGetterM, structStructGetterM, classSetterM, structSetterM;

			internal static Func<TOwner, object> StructClassGetter(MethodInfo mi)
			{
				var del = MkDelegate<StructGetterDel<object>>(mi);
				return o => del(ref o);
			}
			internal static Func<TOwner, object> ClassStructGetter<TVal>(MethodInfo mi)
			{
				var del = MkDelegate<Func<TOwner, TVal>>(mi);
				return o => del(o);
			}
			internal static Func<TOwner, object> StructStructGetter<TVal>(MethodInfo mi)
			{
				var del = MkDelegate<StructGetterDel<TVal>>(mi);
				return o => del(ref o);
			}

			internal static Action<TOwner, object> ClassSetter<TVal>(MethodInfo mi)
			{
				var del = MkDelegate<Action<TOwner, TVal>>(mi);
				return (o, v) => del(o, (TVal)v);
			}
			internal static Action<TOwner, object> StructSetter<TVal>(MethodInfo mi)
			{
				var del = MkDelegate<StructSetterDel<TVal>>(mi);
				return (o, v) => del(ref o, (TVal)v);
			}

			delegate TVal StructGetterDel<out TVal>(ref TOwner obj);
			delegate void StructSetterDel<in TVal>(ref TOwner obj, TVal val);
			static readonly Func<TOwner, object> UninitializedTGetter = o => o;
			static readonly Action<TOwner, object> UninitializedTSetter = (o, v) => { };
		}

		static readonly Func<object, object> UninitializedGetter = o => o;
		static readonly Action<object, object> UninitializedSetter = (o, v) => { };

		static T MkDelegate<T>(MethodInfo mi) { return (T)(object)Delegate.CreateDelegate(typeof(T), mi); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }
	}
}