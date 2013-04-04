using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{

	internal interface IMetaPropCache
	{
		IReadOnlyList<IMetaProperty> Properties { get; }
	}

	internal sealed class MetaPropCache<T> : IMetaPropCache
	{
		public readonly static IMetaProperty<T>[] MetaProperties;

		static MetaPropCache()
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

			MetaProperties = GetMetaPropertiesImpl().ToArray();
		}


		public IReadOnlyList<IMetaProperty> Properties { get { return MetaProperties; } }

		static IEnumerable<IMetaProperty<T>> GetMetaPropertiesImpl()
		{
			return typeof(T).GetProperties().Select(LoadIfMetaProperty)
				.Where(mp => mp != null)
				.OrderBy(mp => mp.PropertyInfo.MetadataToken);
		}
		static IMetaProperty<T> LoadIfMetaProperty(PropertyInfo pi, int implicitOrder)
		{
			return pi.GetCustomAttributes(typeof(MpNotMappedAttribute), true).Any() ? null
				: new MetaProperty.Impl<T>(pi, implicitOrder);
		}
	}
}