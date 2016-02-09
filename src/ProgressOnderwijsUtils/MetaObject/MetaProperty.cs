using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public interface IMetaProperty : IColumnDefinition
    {
        ColumnCss LijstCssClass { get; }
        Func<object, object> UntypedGetter { get; }
        Func<object, object, object> UntypedSetter { get; }
        int Index { get; }
        bool Required { get; }
        bool AllowNullInEditor { get; }
        int? MaxLength { get; }
        int? DisplayLength { get; }
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
        Expression PropertyAccessExpression(Expression paramExpr);
        HtmlEditMode HtmlMode { get; }
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
            readonly string name;
            public string Name => name;
            readonly ColumnCss lijstCssClass;
            public ColumnCss LijstCssClass => lijstCssClass;
            readonly HtmlEditMode htmlMode;
            public HtmlEditMode HtmlMode => htmlMode;
            readonly int index;
            public int Index => index;
            readonly bool required;
            public bool Required => required;
            readonly bool hide;
            public bool Hide => hide;
            readonly bool allowNullInEditor;
            public bool AllowNullInEditor => allowNullInEditor;
            readonly int? maxLength;
            public int? MaxLength => maxLength;
            readonly int? displayLength;
            public int? DisplayLength => displayLength;
            readonly string regex;
            public string Regex => regex;
            readonly DatumFormaat? datumtijd;
            public DatumFormaat? DatumTijd => datumtijd;
            readonly ITranslatable label;
            public ITranslatable Label => label;
            readonly string koppelTabelNaam;
            public string KoppelTabelNaam => koppelTabelNaam;
            readonly bool isReadonly;
            public bool IsReadonly => isReadonly;
            readonly bool showDefaultOnNew;
            public bool ShowDefaultOnNew => showDefaultOnNew;
            public Type DataType => propertyInfo.PropertyType;
            readonly PropertyInfo propertyInfo;
            public PropertyInfo PropertyInfo => propertyInfo;
            readonly bool isKey;
            public bool IsKey => isKey;
            public bool CanRead => getterMethod != null;
            public bool CanWrite => setterMethod != null;
            Func<TOwner, object> getter;
            public Func<TOwner, object> Getter => getter ?? (getter = MkGetter(getterMethod, propertyInfo.PropertyType));
            Setter<TOwner> setter;
            public Setter<TOwner> Setter => setter ?? (setter = MkSetter(setterMethod, propertyInfo.PropertyType));
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

            Func<object, object, object> untypedSetter;

            public Func<object, object, object> UntypedSetter
            {
                get
                {
                    if (untypedSetter == null) {
                        var localSetter = Setter;
                        untypedSetter = localSetter == null ? default(Func<object, object, object>) : (o, v) => {
                            var typedObj = (TOwner)o;
                            localSetter(ref typedObj, v);
                            return typedObj;
                        };
                    }
                    return untypedSetter;
                }
            }

            public Expression PropertyAccessExpression(Expression paramExpr) => Expression.Property(paramExpr, propertyInfo);

            public Impl(PropertyInfo pi, int implicitOrder, object[] attrs)
            {
                propertyInfo = pi;
                name = pi.Name;
                index = implicitOrder;
                getterMethod = pi.GetGetMethod();
                setterMethod = pi.GetSetMethod();

                var mpKoppelTabelAttribute = attrs.AttrH<MpKoppelTabelAttribute>();
                koppelTabelNaam = mpKoppelTabelAttribute == null ? null : (mpKoppelTabelAttribute.KoppelTabelNaam ?? propertyInfo.Name);
                var mpColumnCssAttribute = attrs.AttrH<MpColumnCssAttribute>();
                lijstCssClass = mpColumnCssAttribute == null ? default(ColumnCss) : mpColumnCssAttribute.CssClass;
                var mpHtmlEditModeAttribute = attrs.AttrH<MpHtmlEditModeAttribute>();
                htmlMode = mpHtmlEditModeAttribute == null ? default(HtmlEditMode) : mpHtmlEditModeAttribute.HtmlMode;
                required = attrs.AttrH<MpVerplichtAttribute>() != null;
                hide = attrs.AttrH<HideAttribute>() != null;
                allowNullInEditor = attrs.AttrH<MpAllowNullInEditorAttribute>() != null;
                isKey = attrs.AttrH<KeyAttribute>() != null;
                var mpShowDefaultOnNewAttribute = attrs.AttrH<MpShowDefaultOnNewAttribute>();
                showDefaultOnNew = mpShowDefaultOnNewAttribute != null;
                isReadonly = !pi.CanWrite || (attrs.AttrH<MpReadonlyAttribute>() != null);
                var mpLengteAttribute = attrs.AttrH<MpMaxLengthAttribute>();
                maxLength = mpLengteAttribute == null ? default(int?) : mpLengteAttribute.MaxLength;
                var mpDisplayLengthAttribute = attrs.AttrH<MpDisplayLengthAttribute>();
                displayLength = mpDisplayLengthAttribute == null ? maxLength : mpDisplayLengthAttribute.DisplayLength;
                var mpRegexAttribute = attrs.AttrH<MpRegexAttribute>();
                regex = mpRegexAttribute == null ? null : mpRegexAttribute.Regex;
                var mpDatumFormaatAttribute = attrs.AttrH<MpDatumFormaatAttribute>();
                datumtijd = mpDatumFormaatAttribute == null ? default(DatumFormaat?) : mpDatumFormaatAttribute.Formaat;

                var labelNoTt = LabelNoTt(attrs);
                var mpTooltipAttribute = attrs.AttrH<MpTooltipAttribute>();
                label = mpTooltipAttribute == null ? labelNoTt : labelNoTt.WithTooltip(mpTooltipAttribute.NL, mpTooltipAttribute.EN, mpTooltipAttribute.DE);

                if (KoppelTabelNaam != null && DataType.GetNonNullableUnderlyingType() != typeof(int)) {
                    throw new ProgressNetException(
                        typeof(TOwner) + " heeft Kolom " + Name + " heeft koppeltabel " +
                            KoppelTabelNaam + " maar is van type " + DataType + "!");
                }
            }

            public override string ToString() => ObjectToCode.GetCSharpFriendlyTypeName(typeof(TOwner)) + "." + name;

            LiteralTranslatable LabelNoTt(object[] attrs)
            {
                var mpLabelAttribute = attrs.AttrH<MpLabelAttribute>();
                var labelNoTt = mpLabelAttribute == null ? null : mpLabelAttribute.ToTranslatable();
                var mpLabelUntranslatedAttribute = attrs.AttrH<MpLabelUntranslatedAttribute>();
                var untranslatedLabelNoTt = mpLabelUntranslatedAttribute == null ? null : mpLabelUntranslatedAttribute.ToTranslatable();
                if (untranslatedLabelNoTt != null) {
                    if (labelNoTt != null) {
                        throw new Exception(
                            "Cannot define both an untranslated and a translated label on the same property " +
                                ObjectToCode.GetCSharpFriendlyTypeName(propertyInfo.DeclaringType) + "." + Name);
                    } else {
                        labelNoTt = untranslatedLabelNoTt;
                    }
                }

                if (labelNoTt == null) {
                    var prettyName = StringUtils.PrettyCapitalizedPrintCamelCased(propertyInfo.Name);
                    labelNoTt = Translatable.Literal(prettyName, prettyName, prettyName);
                }
                return labelNoTt;
            }

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

        static T AttrH<T>(this object[] attrs) where T : class
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
