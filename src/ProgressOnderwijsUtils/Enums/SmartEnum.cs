using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public abstract class SmartEnum
    {
        static class Values<T>
            where T : SmartEnum
        {
            static readonly IReadOnlyDictionary<int, T> lookup = typeof(T)
                .GetFields()
                .Where(f => f.IsStatic && f.IsInitOnly && f.FieldType == typeof(T))
                .Select(f => f.GetValue(null))
                .Cast<T>()
                .ToDictionary(val => val.Id);

            public static T GetById(int id)
            {
                return lookup[id];
            }

            public static IEnumerable<T> GetValues()
            {
                return lookup.Values;
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
