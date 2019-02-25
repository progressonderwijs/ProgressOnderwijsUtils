using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public interface IMetaPropCache<out T> : IReadOnlyList<T>
        where T : IMetaProperty { }

    public sealed class MetaInfo<T> : IMetaPropCache<IMetaProperty<T>>
        where T : IMetaObject
    {
        readonly IMetaProperty<T>[] MetaProperties;
        readonly IReadOnlyDictionary<string, int> indexByName;

        public IReadOnlyDictionary<string, int> IndexByName
            => indexByName;

        public static readonly MetaInfo<T> Instance = new MetaInfo<T>();

        MetaInfo()
        {
            if (typeof(T) == typeof(IMetaObject)) {
                throw new ArgumentException("Cannot determine metaproperties on IMetaObject itself");
            } else if (typeof(T).IsInterface) {
                throw new ArgumentException("Cannot determine metaproperties on interface type " + typeof(T));
            } else if (typeof(T).IsAbstract) {
                throw new ArgumentException("Cannot determine metaproperties on abstract type " + typeof(T));
            }

            MetaProperties = GetMetaPropertiesImpl();
            var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in MetaProperties) { //perf:avoid LINQ.
                dictionary.Add(property.Name, property.Index);
            }
            indexByName = dictionary;
        }

        public IMetaProperty<T> GetByName(string name)
            => MetaProperties[indexByName[name]];

        public int Count
            => MetaProperties.Length;

        [NotNull]
        static IMetaProperty<T>[] GetMetaPropertiesImpl()
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var metaProperties = new IMetaProperty<T>[propertyInfos.Length];
            for (var index = 0; index < propertyInfos.Length; index++) {
                var propertyInfo = propertyInfos[index];
                var customAttributes = propertyInfo.GetCustomAttributes(true);
                metaProperties[index] = new MetaProperty.Impl<T>(propertyInfo, index, customAttributes);
            }
            return metaProperties;
        }

        [NotNull]
        public IMetaProperty<T> GetByExpression<TProp>([NotNull] Expression<Func<T, TProp>> propertyExpression)
        {
            var memberInfo = MetaObject.GetMemberInfo(propertyExpression);
            var retval = MetaProperties.SingleOrDefault(mp => mp.PropertyInfo == memberInfo); //TODO:get by name.
            if (retval == null) {
                throw new ArgumentException(
                    "To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
                    "The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
            }
            return retval;
        }

        public IEnumerator<IMetaProperty<T>> GetEnumerator()
        {
            foreach (var mp in MetaProperties) {
                yield return mp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IMetaProperty<T> this[int index]
            => MetaProperties[index];
    }
}
