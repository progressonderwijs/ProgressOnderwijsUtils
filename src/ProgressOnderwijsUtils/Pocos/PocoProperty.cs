using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public interface IPocoProperty
    {
        Func<object, object> UntypedGetter { get; }
        object UnsafeSetPropertyAndReturnObject(object obj, object newValue);
        Expression PropertyAccessExpression(Expression paramExpr);
        bool IsKey { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        PropertyInfo PropertyInfo { get; }
        IReadOnlyList<object> CustomAttributes { get; }
        Type DataType { get; }
        string Name { get; }
        ParameterizedSql SqlColumnName { get; }
        int Index { get; }
    }

    public interface IReadonlyPocoProperty<in TOwner> : IPocoProperty
    {
        Func<TOwner, object> Getter { get; }
    }

    public interface IPocoProperty<TOwner> : IReadonlyPocoProperty<TOwner>
    {
        Setter<TOwner> Setter { get; }
    }

    public static class PocoProperty
    {
        public sealed class Impl<TOwner> : IPocoProperty<TOwner>
        {
            public bool IsKey { get; }
            public string Name { get; }
            ParameterizedSql sqlColumnName;

            public ParameterizedSql SqlColumnName
                => sqlColumnName ? sqlColumnName : sqlColumnName = ParameterizedSql.CreateDynamic(Name);

            public IReadOnlyList<object> CustomAttributes { get; }
            public int Index { get; }

                public Type DataType
                => PropertyInfo.PropertyType;

            public PropertyInfo PropertyInfo { get; }

            public bool CanRead
                => getterMethod != null;

            public bool CanWrite
                => setterMethod != null;

            Func<TOwner, object> getter;

            public Func<TOwner, object> Getter
                => getter ?? (getter = MkGetter(getterMethod, PropertyInfo.PropertyType));

            Setter<TOwner> setter;

            public Setter<TOwner> Setter
                => setter ?? (setter = MkSetter(setterMethod, PropertyInfo.PropertyType));

            Func<object, object>? untypedGetter;

            public Func<object, object>? UntypedGetter
            {
                get {
                    if (untypedGetter == null) {
                        var localGetter = Getter;
                        untypedGetter = localGetter is null ? default(Func<object, object>?) : o => localGetter!((TOwner)o);
                    }
                    return untypedGetter;
                }
            }

            public object UnsafeSetPropertyAndReturnObject(object o, object newValue)
            {
                var typedObj = (TOwner)o;
                Setter(ref typedObj, newValue);
                return typedObj;
            }

                public Expression PropertyAccessExpression(Expression paramExpr)
                => Expression.Property(paramExpr, PropertyInfo);

            public Impl(PropertyInfo pi, int implicitOrder, object[] attrs)
            {
                PropertyInfo = pi;
                Name = pi.Name;
                Index = implicitOrder;
                CustomAttributes = attrs;
                getterMethod = pi.GetGetMethod();
                setterMethod = pi.GetSetMethod();
                foreach (var attr in attrs) {
                    if (attr is KeyAttribute) {
                        IsKey = true;
                        break;
                    }
                }
            }

            public override string ToString()
                => typeof(TOwner).ToCSharpFriendlyTypeName() + "." + Name;

            static Setter<TOwner> MkSetter(MethodInfo? setterMethod, Type propertyType)
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
                //  Expression.Assign(typedPropExpr, Expression.Convert(valParamExpr, pi.PropertyType)),
                //  typedParamExpr, valParamExpr
                //  ).Compile();
            }

            static Func<TOwner, object> MkGetter(MethodInfo? getterMethod, Type propertyType)
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
            => (T)(object)Delegate.CreateDelegate(typeof(T), mi);

        interface IOutCaster
        {
            Func<TObj, object> GetterBoxed<TObj>(MethodInfo method);
            Func<TObj, object> StructGetterBoxed<TObj>(MethodInfo method);
            Setter<TObj> SetterChecked<TObj>(MethodInfo method);
            Setter<TObj> StructSetterChecked<TObj>(MethodInfo method);
        }

        delegate TVal StructGetterDel<TOwner, out TVal>(ref TOwner obj);

        delegate void StructSetterDel<TOwner, in TVal>(ref TOwner obj, TVal val);

        sealed class OutCaster<TOut> : IOutCaster
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
            => CasterFactoryCache.GetOrAdd(propType, type => (IOutCaster)Activator.CreateInstance(typeof(OutCaster<>).MakeGenericType(type)));
    }

    public delegate void Setter<T>(ref T obj, object value);
}
