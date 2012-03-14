using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	public interface IPropertyAccessors
	{
		Func<object, object> Getter { get; }
		Action<object, object> Setter { get; }
		Type OwnerType { get; }
		Type PropertyType { get; }
	}


	public sealed class MetaPropertyAccessors<TOwner, TProperty> : IPropertyAccessors
	{
		// ReSharper disable MemberCanBePrivate.Global
		public readonly Func<TOwner, TProperty> TypedGetter;
		public readonly Action<TOwner, TProperty> TypedSetter;
		public readonly Func<object, object> DynGetter;
		public readonly Action<object, object> DynSetter;
		//public readonly PropertyInfo propInfo;
		public MetaPropertyAccessors(PropertyInfo pi)
		// ReSharper restore MemberCanBePrivate.Global
		{
			TypedGetter = (Func<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Func<TOwner, TProperty>), pi.GetGetMethod());
			bool isReadonly = !pi.CanWrite || pi.GetSetMethod() == null;
			TypedSetter =
				isReadonly ? default(Action<TOwner, TProperty>) :
				(Action<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Action<TOwner, TProperty>), pi.GetSetMethod());
			var objParamExpr = Expression.Parameter(typeof(object), "propertyOwner");
			var propExpr = Expression.Property(Expression.Convert(objParamExpr, typeof(TOwner)), pi);
			var dynGetExpr = Expression.Lambda<Func<object, object>>(Expression.Convert(propExpr, typeof(object)), objParamExpr);
			DynGetter = dynGetExpr.Compile();
			var valParamExpr = Expression.Parameter(typeof(object), "newValue");

			DynSetter = isReadonly ? default(Action<object, object>) :
					Expression.Lambda<Action<object, object>>(
						Expression.Assign(propExpr,
							Expression.Convert(valParamExpr, typeof(TProperty))
						), objParamExpr, valParamExpr
					).Compile();
			//Expression.Assign will throw an exception if constructed with a property without pi.CanWrite!
			//propInfo = pi;
		}

		Func<object, object> IPropertyAccessors.Getter { get { return DynGetter; } }
		Action<object, object> IPropertyAccessors.Setter { get { return DynSetter; } }

		public Type OwnerType { get { return typeof(TOwner); } }
		public Type PropertyType { get { return typeof(TProperty); } }
	}
}