using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Field), MeansImplicitUse]
    public sealed class SmartEnumMemberAttribute : Attribute { }

    public interface ISmartEnum
    {
        int Id { get; }
        ITranslatable Text { get; }
    }

    public static class SmartEnum
    {
        static class Values<T>
            where T : ISmartEnum
        {
            static readonly T[] values = typeof(T)
                .GetFields()
                .Where(f => f.GetCustomAttribute<SmartEnumMemberAttribute>() != null)
                .Select(f => f.GetValue(null))
                .Cast<T>()
                .ToArray();

            static readonly Dictionary<int, T> lookup = values.ToDictionary(val => val.Id);

            public static T GetById(int id)
            {
                return lookup[id];
            }

            public static IEnumerable<T> GetValues()
            {
                return values;
            }
        }

        public static T GetById<T>(int id)
            where T : ISmartEnum
        {
            return Values<T>.GetById(id);
        }

        public static IEnumerable<T> GetValues<T>()
            where T : ISmartEnum
        {
            return Values<T>.GetValues();
        }

        public static bool IsSmartEnum(Type type)
        {
            return typeof(ISmartEnum).IsAssignableFrom(type);
        }
    }
}
