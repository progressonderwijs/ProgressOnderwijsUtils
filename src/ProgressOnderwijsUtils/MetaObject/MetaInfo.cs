using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
	public interface IMetaPropCache<out T> : IReadOnlyList<T> where T : IMetaProperty
	{
		IReadOnlyDictionary<string, int> IndexByName { get; }
	}

	public sealed class MetaInfo<T> : IMetaPropCache<IMetaProperty<T>> where T : IMetaObject
	{
		readonly IMetaProperty<T>[] MetaProperties;

		readonly IReadOnlyDictionary<string, int> indexByName;
		public IReadOnlyDictionary<string, int> IndexByName { get { return indexByName; } }

		public readonly static MetaInfo<T> Instance = new MetaInfo<T>();

		MetaInfo()
		{
			if (typeof(T) == typeof(IMetaObject))
				throw new ArgumentException("Cannot determine metaproperties on IMetaObject itself");
			else if (typeof(T).IsInterface)
				throw new ArgumentException("Cannot determine metaproperties on interface type " + typeof(T));
			else if (typeof(T).IsAbstract)
				throw new ArgumentException("Cannot determine metaproperties on abstract type " + typeof(T));

			MetaProperties = GetMetaPropertiesImpl();
			var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			foreach (var property in MetaProperties)//perf:avoid LINQ.
				dictionary.Add(property.Name, property.Index);
			indexByName = dictionary;
		}

		public IMetaProperty<T> GetByName(string name) { return MetaProperties[indexByName[name]]; }
		public int Count { get { return MetaProperties.Length; } }

		static IMetaProperty<T>[] GetMetaPropertiesImpl()
		{
			int index = 0;
			var propertyInfos = typeof(T).GetProperties();
			var metaProperties = new IMetaProperty<T>[propertyInfos.Length];
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				var customAttributes = propertyInfo.GetCustomAttributes(true);
				bool isMapped = true;
				foreach (var attr in customAttributes)
					if (attr is MpNotMappedAttribute)
					{
						isMapped = false;
						break;
					}
				if (isMapped)
				{
					metaProperties[index] = new MetaProperty.Impl<T>(propertyInfo, index, customAttributes);
					index++;
				}
			}
			Array.Resize(ref metaProperties, index);
			return metaProperties;
		}


		public IMetaProperty<T> GetByExpression<TProp>(Expression<Func<T, TProp>> propertyExpression)
		{
			var memberInfo = MetaObject.GetMemberInfo(propertyExpression);
			var retval = MetaProperties.SingleOrDefault(mp => mp.PropertyInfo == memberInfo); //TODO:get by name.
			if (retval == null)
				throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
						"The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
			return retval;
		}

		public IMetaProperty<T> GetByNameOrNull(string colName)
		{
			int index;
			return indexByName.TryGetValue(colName, out index) ? MetaProperties[index] : null;
		}

		public IEnumerator<IMetaProperty<T>> GetEnumerator()
		{
			foreach (var mp in MetaProperties)
				yield return mp;
		}
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public IMetaProperty<T> this[int index] { get { return MetaProperties[index]; } }
	}
}