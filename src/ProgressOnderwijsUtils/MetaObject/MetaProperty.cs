using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public class PropertyMetaData {
        public readonly bool Required;
        public readonly int? MaxLength;
        public readonly int? DisplayLength;
        public readonly string Regex;
        public readonly DatumFormaat? DatumTijd;
        public readonly ITranslatable Label;
        public readonly string KoppelTabelNaam;
        public readonly bool IsReadonly;
        public readonly bool IsKey;
        public readonly bool Hide;
        public readonly bool ShowDefaultOnNew;
        public readonly PropertyInfo PropertyInfo;
        public readonly HtmlEditMode HtmlMode;
        public readonly ColumnCss LijstCssClass;

        public PropertyMetaData(IMetaProperty prop)
        {
            var attrs = prop.CustomAttributes;
            var mpKoppelTabelAttribute = attrs.AttrH<MpKoppelTabelAttribute>();
            KoppelTabelNaam = mpKoppelTabelAttribute == null ? null : (mpKoppelTabelAttribute.KoppelTabelNaam ?? PropertyInfo.Name);
            var mpColumnCssAttribute = attrs.AttrH<MpColumnCssAttribute>();
            LijstCssClass = mpColumnCssAttribute == null ? default(ColumnCss) : mpColumnCssAttribute.CssClass;
            var mpHtmlEditModeAttribute = attrs.AttrH<MpHtmlEditModeAttribute>();
            HtmlMode = mpHtmlEditModeAttribute == null ? default(HtmlEditMode) : mpHtmlEditModeAttribute.HtmlMode;
            Required = attrs.AttrH<MpVerplichtAttribute>() != null;
            Hide = attrs.AttrH<HideAttribute>() != null;
            IsKey = attrs.AttrH<KeyAttribute>() != null;
            var mpShowDefaultOnNewAttribute = attrs.AttrH<MpShowDefaultOnNewAttribute>();
            ShowDefaultOnNew = mpShowDefaultOnNewAttribute != null;
            IsReadonly = !prop.CanWrite || (attrs.AttrH<MpReadonlyAttribute>() != null);
            var mpLengteAttribute = attrs.AttrH<MpMaxLengthAttribute>();
            MaxLength = mpLengteAttribute == null ? default(int?) : mpLengteAttribute.MaxLength;
            var mpDisplayLengthAttribute = attrs.AttrH<MpDisplayLengthAttribute>();
            DisplayLength = mpDisplayLengthAttribute == null ? MaxLength : mpDisplayLengthAttribute.DisplayLength;
            var mpRegexAttribute = attrs.AttrH<MpRegexAttribute>();
            Regex = mpRegexAttribute == null ? null : mpRegexAttribute.Regex;
            var mpDatumFormaatAttribute = attrs.AttrH<MpDatumFormaatAttribute>();
            DatumTijd = mpDatumFormaatAttribute == null ? default(DatumFormaat?) : mpDatumFormaatAttribute.Formaat;

            var labelNoTt = LabelNoTt(prop, attrs);
            var mpTooltipAttribute = attrs.AttrH<MpTooltipAttribute>();
            Label = mpTooltipAttribute == null ? labelNoTt : labelNoTt.WithTooltip(mpTooltipAttribute.NL, mpTooltipAttribute.EN, mpTooltipAttribute.DE);

            if (KoppelTabelNaam != null && prop.DataType.GetNonNullableUnderlyingType() != typeof(int)) {
                throw new ProgressNetException(
                    prop.PropertyInfo.DeclaringType + " heeft Kolom " + prop.Name + " heeft koppeltabel " +
                        KoppelTabelNaam + " maar is van type " + prop.DataType + "!");
            }
        }

        LiteralTranslatable LabelNoTt(IMetaProperty prop, IReadOnlyList<object> attrs)
        {
            var mpLabelAttribute = attrs.AttrH<MpLabelAttribute>();
            var labelNoTt = mpLabelAttribute == null ? null : mpLabelAttribute.ToTranslatable();
            var mpLabelUntranslatedAttribute = attrs.AttrH<MpLabelUntranslatedAttribute>();
            var untranslatedLabelNoTt = mpLabelUntranslatedAttribute == null ? null : mpLabelUntranslatedAttribute.ToTranslatable();
            if (untranslatedLabelNoTt != null) {
                if (labelNoTt != null) {
                    throw new Exception(
                        "Cannot define both an untranslated and a translated label on the same property " +
                            ObjectToCode.GetCSharpFriendlyTypeName(PropertyInfo.DeclaringType) + "." + prop.Name);
                } else {
                    labelNoTt = untranslatedLabelNoTt;
                }
            }

            if (labelNoTt == null) {
                var prettyName = StringUtils.PrettyCapitalizedPrintCamelCased(PropertyInfo.Name);
                labelNoTt = Translatable.Literal(prettyName, prettyName, prettyName);
            }
            return labelNoTt;
        }
    }

    public static class MetaPropertyExtensions
    {
        static readonly ConcurrentDictionary<IMetaProperty, PropertyMetaData> ProgressMetaData = new ConcurrentDictionary<IMetaProperty, PropertyMetaData>();
        static readonly Func<IMetaProperty, PropertyMetaData> valueFactory = prop => new PropertyMetaData(prop);

        public static PropertyMetaData ExtraMetaData(this IMetaProperty property) => ProgressMetaData.GetOrAdd(property, valueFactory);
    }

    public interface IMetaProperty : IColumnDefinition
    {
        Func<object, object> UntypedGetter { get; }
        object UnsafeSetPropertyAndReturnObject(object obj, object newValue);
        int Index { get; }
        Expression PropertyAccessExpression(Expression paramExpr);
        bool CanRead { get; }
        bool CanWrite { get; }
        PropertyInfo PropertyInfo { get; }
        IReadOnlyList<object> CustomAttributes { get; }
    }

    public interface IReadonlyMetaProperty<in TOwner> : IMetaProperty
    {
        Func<TOwner, object> Getter { get; }
    }

    public interface IMetaProperty<TOwner> : IReadonlyMetaProperty<TOwner>
    {
        Setter<TOwner> Setter { get; }
    }

    public static class MetaProperty
    {
        public sealed class Impl<TOwner> : IMetaProperty<TOwner>
        {
            public string Name { get; }
            public IReadOnlyList<object> CustomAttributes { get; }
            public int Index { get; }
            public Type DataType => PropertyInfo.PropertyType;
            public PropertyInfo PropertyInfo { get; }
            public bool CanRead => getterMethod != null;
            public bool CanWrite => setterMethod != null;
            Func<TOwner, object> getter;
            public Func<TOwner, object> Getter => getter ?? (getter = MkGetter(getterMethod, PropertyInfo.PropertyType));
            Setter<TOwner> setter;
            public Setter<TOwner> Setter => setter ?? (setter = MkSetter(setterMethod, PropertyInfo.PropertyType));
            Func<object, object> untypedGetter;

            public Func<object, object> UntypedGetter
            {
                get
                {
                    if (untypedGetter == null) {
                        var localGetter = Getter;
                        untypedGetter = localGetter == null ? default(Func<object, object>) : o => localGetter((TOwner)o);
                    }
                    return untypedGetter;
                }
            }

            public object UnsafeSetPropertyAndReturnObject(object o, object newValue) {
                var typedObj = (TOwner)o;
                Setter(ref typedObj, newValue);
                return typedObj;
            }

            public Expression PropertyAccessExpression(Expression paramExpr) => Expression.Property(paramExpr, PropertyInfo);

            public Impl(PropertyInfo pi, int implicitOrder, object[] attrs)
            {
                PropertyInfo = pi;
                Name = pi.Name;
                Index = implicitOrder;
                CustomAttributes = attrs;
                getterMethod = pi.GetGetMethod();
                setterMethod = pi.GetSetMethod();
            }

            public override string ToString() => ObjectToCode.GetCSharpFriendlyTypeName(typeof(TOwner)) + "." + Name;

            static Setter<TOwner> MkSetter(MethodInfo setterMethod, Type propertyType)
            {
                if (setterMethod == null) {
                    return null;
                }
                if (typeof(TOwner).IsValueType) {
                    return GetCaster(propertyType).StructSetterChecked<TOwner>(setterMethod);
                } else {
                    return GetCaster(propertyType).SetterChecked<TOwner>(setterMethod);
                }

                //faster code, slower startup:				
                //var valParamExpr = Expression.Parameter(typeof(object), "newValue");
                //var typedParamExpr = Expression.Parameter(typeof(TOwner), "propertyOwner");
                //var typedPropExpr = Expression.Property(typedParamExpr, pi);

                //return Expression.Lambda<Action<TOwner, object>>(
                //		Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)),
                //		typedParamExpr, valParamExpr
                //		).Compile();
            }

            static Func<TOwner, object> MkGetter(MethodInfo getterMethod, Type propertyType)
            {
                //TODO:optimize: this is still a hotspot :-(
                if (getterMethod == null) {
                    return null;
                } else if (propertyType.IsValueType) {
                    if (typeof(TOwner).IsValueType) {
                        return GetCaster(propertyType).StructGetterBoxed<TOwner>(getterMethod);
                    } else {
                        return GetCaster(propertyType).GetterBoxed<TOwner>(getterMethod);
                    }
                } else {
                    if (typeof(TOwner).IsValueType) {
                        return outCasterObject.StructGetterBoxed<TOwner>(getterMethod);
                    } else {
                        return MkDelegate<Func<TOwner, object>>(getterMethod);
                    }
                }
            }

            readonly MethodInfo setterMethod;
            readonly MethodInfo getterMethod;
        }

        static T MkDelegate<T>(MethodInfo mi)
        {
            return (T)(object)Delegate.CreateDelegate(typeof(T), mi);
        }

        public static T AttrH<T>(this IReadOnlyList<object> attrs) where T : class
        {
            foreach (var obj in attrs) {
                if (obj is T) {
                    return (T)obj;
                }
            }
            return null;
        }

        interface IOutCaster
        {
            Func<TObj, object> GetterBoxed<TObj>(MethodInfo method);
            Func<TObj, object> StructGetterBoxed<TObj>(MethodInfo method);
            Setter<TObj> SetterChecked<TObj>(MethodInfo method);
            Setter<TObj> StructSetterChecked<TObj>(MethodInfo method);
        }

        delegate TVal StructGetterDel<TOwner, out TVal>(ref TOwner obj);

        delegate void StructSetterDel<TOwner, in TVal>(ref TOwner obj, TVal val);

        class OutCaster<TOut> : IOutCaster
        {
            public Func<TObj, object> GetterBoxed<TObj>(MethodInfo method)
            {
                var f = MkDelegate<Func<TObj, TOut>>(method);
                return o => f(o);
            }

            public Func<TObj, object> StructGetterBoxed<TObj>(MethodInfo method)
            {
                var f = MkDelegate<StructGetterDel<TObj, TOut>>(method);
                return o => f(ref o);
            }

            public Setter<TObj> SetterChecked<TObj>(MethodInfo method)
            {
                var f = MkDelegate<Action<TObj, TOut>>(method);
                return (ref TObj o, object v) => f(o, (TOut)v);
            }

            public Setter<TObj> StructSetterChecked<TObj>(MethodInfo method)
            {
                var f = MkDelegate<StructSetterDel<TObj, TOut>>(method);
                return (ref TObj o, object v) => f(ref o, (TOut)v);
            }
        }

        static readonly OutCaster<object> outCasterObject = new OutCaster<object>();
        static readonly ConcurrentDictionary<Type, IOutCaster> CasterFactoryCache = new ConcurrentDictionary<Type, IOutCaster>();

        static IOutCaster GetCaster(Type propType)
        {
            return CasterFactoryCache.GetOrAdd(propType, type => (IOutCaster)Activator.CreateInstance(typeof(OutCaster<>).MakeGenericType(type)));
        }
    }

    public delegate void Setter<T>(ref T obj, object value);
}
