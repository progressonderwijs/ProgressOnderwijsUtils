using System;
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
		Func<object, object> Getter { get; }
		Action<object, object> Setter { get; }
		int Volgorde { get; }
		bool Verplicht { get; }
		bool AllowNull { get; }
		int Lengte { get; }
		string Regex { get; }
		ITranslatable Label { get; }
		string KoppelTabelNaam { get; }
		bool IsReadonly { get; }
		bool ShowDefaultOnNew { get; }
		bool CanRead { get; }
		Type DataType { get; }
		PropertyInfo PropertyInfo { get; }
	}

	public static class MetaProperty
	{
		public sealed class Impl<TOwner> : IMetaProperty
		{
			readonly string naam;
			public string Naam { get { return naam; } }

			readonly int volgorde;
			public int Volgorde { get { return volgorde; } }

			readonly bool verplicht;
			public bool Verplicht { get { return verplicht; } }

			readonly bool allowNull;
			public bool AllowNull { get { return allowNull; } }

			readonly int lengte;
			public int Lengte { get { return lengte; } }

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

			public Impl(PropertyInfo pi, int implicitOrder)
			{
				propertyInfo = pi;

				ParameterExpression objParamExpr = Expression.Parameter(typeof(object), "propertyOwner");
				MemberExpression propExpr = Expression.Property(Expression.Convert(objParamExpr, typeof(TOwner)), pi);
				getter = Expression.Lambda<Func<object, object>>(Expression.Convert(propExpr, typeof(object)), objParamExpr).Compile();


				ParameterExpression typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
				MemberExpression typedPropExpr = Expression.Property(typedParamExpr, pi);
				TypedGetter = Expression.Lambda<Func<TOwner, object>>(Expression.Convert(typedPropExpr, typeof(object)), typedParamExpr).Compile();


				var valParamExpr = Expression.Parameter(typeof(object), "newValue");

				bool canWrite = !pi.CanWrite || pi.GetSetMethod() == null;
				setter = canWrite ? default(Action<object, object>) :
						Expression.Lambda<Action<object, object>>(
							Expression.Assign(propExpr,
								Expression.Convert(valParamExpr, pi.PropertyType)
							), objParamExpr, valParamExpr
						).Compile();

				TypedSetter = canWrite ? default(Action<TOwner, object>) :
						Expression.Lambda<Action<TOwner, object>>(
							Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)
							), typedParamExpr, valParamExpr
						).Compile();


				naam = pi.Name;
				var mpVolgordeAttribute = Attr<MpVolgordeAttribute>(pi);
				volgorde = mpVolgordeAttribute == null ? implicitOrder * 10 : mpVolgordeAttribute.Volgorde;
				label = new[] {
					OrDefault(Attr<MpSimpleLabelAttribute>(pi), mkAttr => mkAttr.Label),
					OrDefault(Attr<MpTextDefKeyAttribute>(pi), mkAttr => mkAttr.Label) }
					.SingleOrDefault(text => text != null);
				if (Label == null && Attr<MpLabelsRequiredAttribute>(pi.DeclaringType) != null)
					throw new ArgumentException("You must specify an MpSimpleLabel or MpTextDefKey on " + Naam + ", since the class " + ObjectToCode.GetCSharpFriendlyTypeName(pi.DeclaringType) + " is marked MpLabelsRequired");
				koppelTabelNaam = OrDefault(Attr<MpKoppelTabelAttribute>(pi), mkAttr => mkAttr.KoppelTabelNaam ?? pi.Name);
				verplicht = OrDefault(Attr<MpVerplichtAttribute>(pi), mkAttr => true);
				allowNull = OrDefault(Attr<MpAllowNullAttribute>(pi), mkAttr => true);
				showDefaultOnNew = OrDefault(Attr<MpShowDefaultOnNewAttribute>(pi), mkAttr => true);
				isReadonly = Setter == null || OrDefault(Attr<MpReadonlyAttribute>(pi), mkAttr => true);
				lengte = OrDefault(Attr<MpLengteAttribute>(pi), mkAttr => mkAttr.Lengte);
				regex = OrDefault(Attr<MpRegexAttribute>(pi), mkAttr => mkAttr.Regex);

				if (KoppelTabelNaam != null && DataType != typeof(int) && DataType != typeof(int?))
					throw new ProgressNetException(typeof(TOwner) + " heeft Kolom " + Naam + " heeft koppeltabel " + KoppelTabelNaam + " maar is van type " + DataType + "!");

			}


			public bool CanRead { get { return getter != null; } }

			public readonly Func<object, object> getter;
			public readonly Func<TOwner, object> TypedGetter;
			public Func<object, object> Getter { get { return getter; } }

			public readonly Action<object, object> setter;
			public readonly Action<TOwner, object> TypedSetter;
			public Action<object, object> Setter { get { return setter; } }
		}


		static T Attr<T>(MemberInfo mi) where T : Attribute { return mi.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault(); }
		static TR OrDefault<T, TR>(T val, Func<T, TR> project, TR defaultVal = default(TR)) { return Equals(val, default(T)) ? defaultVal : project(val); }
		public static IMetaProperty LoadIfMetaProperty(PropertyInfo pi, int implicitOrder)
		{
			return Attr<MpNotMappedAttribute>(pi) != null ? null :
				(IMetaProperty)Activator.CreateInstance(typeof(Impl<>).MakeGenericType(pi.DeclaringType), pi, implicitOrder);
		}

	}
}