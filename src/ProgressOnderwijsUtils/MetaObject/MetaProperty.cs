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
	public interface IMetaProperty
	{
		string Naam { get; }
		string LijstCssClass { get; }
		Func<object, object> Getter { get; }
		Action<object, object> Setter { get; }
		int Volgorde { get; }
		bool Verplicht { get; }
		bool AllowNull { get; }
		int? Lengte { get; }
		string Regex { get; }
		ITranslatable Label { get; }
		string KoppelTabelNaam { get; }
		bool IsReadonly { get; }
		bool IsKey { get; }
		bool Hide { get; }
		bool ShowDefaultOnNew { get; }
		bool CanRead { get; }
		Type DataType { get; }
		PropertyInfo PropertyInfo { get; }
	}

	public interface IMetaProperty<in TOwner> : IMetaProperty
	{
		Func<TOwner, object> TypedGetter { get; }
		Action<TOwner, object> TypedSetter { get; }
		Func<TOwner, TOwner, int> CompareByFunc { get; }
	}

	public static class MetaProperty
	{
		public sealed class Impl<TOwner> : IMetaProperty<TOwner>
		{
			readonly string naam;
			public string Naam { get { return naam; } }

			readonly string lijstCssClass;
			public string LijstCssClass { get { return lijstCssClass; } }

			readonly int volgorde;
			public int Volgorde { get { return volgorde; } }

			readonly bool verplicht;
			public bool Verplicht { get { return verplicht; } }

			readonly bool hide;
			public bool Hide { get { return hide; } }

			readonly bool allowNull;
			public bool AllowNull { get { return allowNull; } }

			readonly int? lengte;
			public int? Lengte { get { return lengte; } }

			readonly string regex;
			public string Regex { get { return regex; } }

			readonly ITranslatable label;
			public ITranslatable Label { get { return label; } }

			readonly string koppelTabelNaam;
			public string KoppelTabelNaam { get { return koppelTabelNaam; } }

			readonly bool isReadonly;
			public bool IsReadonly { get { return isReadonly; } }

			readonly bool showDefaultOnNew;
			public bool ShowDefaultOnNew { get { return showDefaultOnNew; } }

			public Type DataType { get { return propertyInfo.PropertyType; } }

			public readonly PropertyInfo propertyInfo;
			public PropertyInfo PropertyInfo { get { return propertyInfo; } }

			public readonly bool isKey;
			public bool IsKey { get { return isKey; } }

			public Impl(PropertyInfo pi, int implicitOrder)
			{
				propertyInfo = pi;

				ParameterExpression objParamExpr = Expression.Parameter(typeof(object), "propertyOwner");
				MemberExpression propExpr = Expression.Property(Expression.Convert(objParamExpr, typeof(TOwner)), pi);
				getter = Expression.Lambda<Func<object, object>>(Expression.Convert(propExpr, typeof(object)), objParamExpr).Compile();


				ParameterExpression typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				MemberExpression typedPropExpr = Expression.Property(typedParamExpr, pi);
				typedGetter = Expression.Lambda<Func<TOwner, object>>(Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr).Compile();


				ParameterExpression parA = Expression.Parameter(typeof(TOwner), "a");
				ParameterExpression parB = Expression.Parameter(typeof(TOwner), "b");
				var comparer =
					pi.PropertyType == typeof(string) ? StringComparer.OrdinalIgnoreCase
					:
					typeof(Comparer<>).MakeGenericType(pi.PropertyType).GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
				var cmpMethod = typeof(IComparer<>).MakeGenericType(pi.PropertyType).GetMethod("Compare", BindingFlags.Instance | BindingFlags.Public);

				var cmpExpr = Expression.Call(Expression.Constant(comparer), cmpMethod, Expression.Property(parA, pi), Expression.Property(parB, pi));

				compareByFunc = Expression.Lambda<Func<TOwner, TOwner, int>>(cmpExpr, parA, parB).Compile();


				var valParamExpr = Expression.Parameter(typeof(object), "newValue");

				bool canWrite = !pi.CanWrite || pi.GetSetMethod() == null;
				setter = canWrite ? default(Action<object, object>) :
																		Expression.Lambda<Action<object, object>>(
																			Expression.Assign(propExpr,
																							  Expression.Convert(valParamExpr, pi.PropertyType)
																				), objParamExpr, valParamExpr
																			).Compile();

				typedSetter = canWrite ? default(Action<TOwner, object>) :
																			Expression.Lambda<Action<TOwner, object>>(
																				Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)
																					), typedParamExpr, valParamExpr
																				).Compile();


				naam = pi.Name;
				var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(pi);
				volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;

				var labelNoTt = OrDefault(Attr<MpLabelAttribute>(pi), mkAttr => Translatable.Literal(mkAttr.NL, mkAttr.EN, mkAttr.DE));
				if (labelNoTt == null && Attr<MpLabelsRequiredAttribute>(pi.DeclaringType) != null)
					throw new ArgumentException("You must specify an MpLabel on " + Naam + ", since the class " + ObjectToCode.GetCSharpFriendlyTypeName(pi.DeclaringType) + " is marked MpLabelsRequired");
				if (labelNoTt == null)
				{
					var prettyName = StringUtils.PrettyPrintCamelCased(pi.Name);
					labelNoTt = Translatable.Literal(prettyName, prettyName, prettyName);
				}
				label = OrDefault(Attr<MpTooltipAttribute>(pi), mkAttr => labelNoTt.WithTooltip(mkAttr.NL, mkAttr.EN, mkAttr.DE), labelNoTt);
				koppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(pi), mkAttr => mkAttr.KoppelTabelNaam ?? pi.Name);
				lijstCssClass = OrDefault(Attr<MpLijstCssAttribute>(pi), mkAttr => mkAttr.CssClass);
				verplicht = OrDefault(Attr<MpVerplichtAttribute>(pi), mkAttr => true);
				hide = OrDefault(Attr<HideAttribute>(pi), mkAttr => true);
				allowNull = OrDefault(Attr<MpAllowNullAttribute>(pi), mkAttr => true);
				isKey = OrDefault(Attr<KeyAttribute>(pi), mkAttr => true);
				showDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(pi), mkAttr => true);
				isReadonly = Setter == null || OrDefault(Attr<MpReadonlyAttribute>(pi), mkAttr => true);
				lengte = OrDefault(Attr<MpLengteAttribute>(pi), mkAttr => mkAttr.Lengte, default(int?));
				regex = OrDefault(Attr<MpRegexAttribute>(pi), mkAttr => mkAttr.Regex);

				if (KoppelTabelNaam != null && DataType != typeof(int) && DataType != typeof(int?))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Naam + " heeft koppeltabel " + KoppelTabelNaam + " maar is van type " + DataType + "!");

			}


			public bool CanRead { get { return getter != null; } }

			public readonly Func<object, object> getter;
			public readonly Func<TOwner, object> typedGetter;
			public readonly Func<TOwner, TOwner, int> compareByFunc;
			public Func<object, object> Getter { get { return getter; } }
			public Func<TOwner, object> TypedGetter { get { return typedGetter; } }

			public Func<TOwner, TOwner, int> CompareByFunc { get { return compareByFunc; } }

			public readonly Action<object, object> setter;
			public readonly Action<TOwner, object> typedSetter;
			public Action<object, object> Setter { get { return setter; } }
			public Action<TOwner, object> TypedSetter { get { return typedSetter; } }
		}


		static T Attr<T>(MemberInfo mi) where T : Attribute { return mi.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault(); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }
	}
}