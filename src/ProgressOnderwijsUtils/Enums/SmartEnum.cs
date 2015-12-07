using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Field), MeansImplicitUse]
    public sealed class SmartEnumMemberAttribute : Attribute { }

    public abstract class SmartEnum
    {
        static class Values<T>
            where T : SmartEnum
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
            where T : SmartEnum
        {
            return Values<T>.GetById(id);
        }

        public static IEnumerable<T> GetValues<T>()
            where T : SmartEnum
        {
            return Values<T>.GetValues();
        }

        public static bool IsSmartEnum(Type type)
        {
            return typeof(SmartEnum).IsAssignableFrom(type);
        }

        public int Id { get; }
        public ITranslatable Text { get; }

        protected SmartEnum(int id, ITranslatable text)
        {
            Id = id;
            Text = text;
        }

        public override string ToString()
        {
            return Text.Translate(Taal.NL).Text;
        }
    }
}
