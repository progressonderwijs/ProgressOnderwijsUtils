using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;

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
				var nonAbstractBaseTypes = typeof(T).BaseTypes().Where(bt => !bt.IsAbstract && typeof(IMetaObject).IsAssignableFrom(bt));
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


		IReadOnlyList<IMetaProperty> IMetaPropCache.Properties { get { return MetaProperties; } }

		IMetaProperty IMetaPropCache.this[string name] { get { return ByName[name]; } }
		public IMetaProperty<T> this[string name] { get { return ByName[name]; } }
		public bool HasColumn(string name) { return ByName.ContainsKey(name); }

		public int Count { get { return MetaProperties.Length; } }
		IReadOnlyList<IMetaProperty<T>> IMetaPropCache<T>.Properties { get { return ReadOnlyView; } }

		static IMetaProperty<T>[] GetMetaPropertiesImpl()
		{
			return typeof(T).GetProperties()
				.Where(pi => !pi.GetCustomAttributes(typeof(MpNotMappedAttribute), true).Any())
				.OrderBy(pi => pi.MetadataToken)
				.Select((info, i) => (IMetaProperty<T>)new MetaProperty.Impl<T>(info, i))
				.ToArray();
		}

		public IEnumerator<IMetaProperty<T>> GetEnumerator() { return ReadOnlyView.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}