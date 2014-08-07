using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{

	public interface IMetaPropCache
	{
		IReadOnlyList<IMetaProperty> Properties { get; }
		IMetaProperty this[string name] { get; }
		int Count { get; }

	}
	public interface IMetaPropCache<in T> : IMetaPropCache
	{
		new IReadOnlyList<IMetaProperty<T>> Properties { get; }
		new IMetaProperty<T> this[string name] { get; }
	}

	public sealed class MetaInfo<T> : IMetaPropCache<T>, IEnumerable<IMetaProperty<T>>
		where T : IMetaObject
	{
		readonly IMetaProperty<T>[] MetaProperties;
		readonly IReadOnlyList<IMetaProperty<T>> ReadOnlyView;
		readonly Dictionary<string, IMetaProperty<T>> ByName;

		public readonly static MetaInfo<T> Instance = new MetaInfo<T>();

		MetaInfo()
		{
			if (typeof(T) == typeof(IMetaObject))
				throw new ArgumentException("Cannot determine metaproperties on IMetaObject itself");
			else if (typeof(T).IsInterface)
				throw new ArgumentException("Cannot determine metaproperties on interface type " + typeof(T));
			else if (typeof(T).IsAbstract)
				throw new ArgumentException("Cannot determine metaproperties on abstract type " + typeof(T));
			else
			{
				var nonAbstractBaseTypes = NonAbstractMetaObjectBaseTypes();
				if (nonAbstractBaseTypes.Any())
					throw new ArgumentException("Cannot determine metaproperties on type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)) + " with non-abstract base type(s) : " + String.Join(", ", nonAbstractBaseTypes.Select(ObjectToCode.GetCSharpFriendlyTypeName)));
				else if (!typeof(T).GetProperties().Any())
					Console.WriteLine("Warning: attempting to load metaproperties on type " + typeof(T) + " without properties.");
				//throw new ArgumentException("Cannot determine metaproperties on type " + typeof(T) + " without properties");
			}

			MetaProperties = GetMetaPropertiesImpl();
			ReadOnlyView = MetaProperties.AsReadView();
			ByName = MetaProperties.ToDictionary(mp => mp.Name, StringComparer.OrdinalIgnoreCase);
		}

		static IEnumerable<Type> NonAbstractMetaObjectBaseTypes()
		{
			foreach (Type bt in typeof(T).BaseTypes())
				if (!bt.IsAbstract && typeof(IMetaObject).IsAssignableFrom(bt))
					yield return bt;
		}

		IReadOnlyList<IMetaProperty> IMetaPropCache.Properties { get { return MetaProperties; } }

		IMetaProperty IMetaPropCache.this[string name] { get { return ByName[name]; } }
		public IMetaProperty<T> this[string name] { get { return ByName[name]; } }
		public bool HasColumn(string name) { return ByName.ContainsKey(name); }

		public int Count { get { return MetaProperties.Length; } }
		IReadOnlyList<IMetaProperty<T>> IMetaPropCache<T>.Properties { get { return ReadOnlyView; } }

		static IMetaProperty<T>[] GetMetaPropertiesImpl()
		{
			var list = FastArrayBuilder<IMetaProperty<T>>.Create();
			int i = 0;
			foreach (PropertyInfo info in typeof(T).GetProperties())
			{
				if (info.GetCustomAttributes(typeof(MpNotMappedAttribute), true).Length == 0)
					list.Add(new MetaProperty.Impl<T>(info, i++));
			}
			var array = list.ToArray();
			Array.Sort(array, (a, b) => a.PropertyInfo.MetadataToken.CompareTo(b.PropertyInfo.MetadataToken));
			return array;
		}

		public IEnumerator<IMetaProperty<T>> GetEnumerator() { return ReadOnlyView.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public IMetaProperty<T> GetByExpression<TProp>(Expression<Func<T, TProp>> propertyExpression)
		{
			var memberInfo = MetaObject.GetMemberInfo(propertyExpression);
			var retval = MetaProperties.SingleOrDefault(mp => mp.PropertyInfo == memberInfo); //TODO:get by name.
			if (retval == null)
				throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
						"The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
			return retval;
		}

		public bool TryGetValue(string colName, out IMetaProperty<T> metaProperty) { return ByName.TryGetValue(colName, out metaProperty); }
	}
}