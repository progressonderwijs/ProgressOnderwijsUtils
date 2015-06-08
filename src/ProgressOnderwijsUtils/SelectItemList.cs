using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class SelectItemList
    {
        public static IReadOnlyList<SelectItem<T>> Create<T>(IEnumerable<IToSelectItem<T>> collection)
        {
            return Create(collection.Select(i => i.ToSelectItem()));
        }

        public static IReadOnlyList<SelectItem<T>> Create<T>(IEnumerable<SelectItem<T>> collection) { return collection.ToArray(); }

        public static IReadOnlyList<SelectItem<T>> CreateWithLeeg<T>(SelectItem<T> addnullitem, IEnumerable<IToSelectItem<T>> collection)
        {
            return CreateWithLeeg(addnullitem, Create(collection));
        }

        public static IReadOnlyList<SelectItem<T>> CreateWithLeeg<T>(SelectItem<T> addnullitem, IEnumerable<SelectItem<T>> collection)
        {
            return Create(new[] { addnullitem }.Concat(collection));
        }

        public static IReadOnlyList<SelectItem<T?>> CreateWithLeeg<T>(SelectItem<T?> addnullitem, IEnumerable<SelectItem<T>> collection) where T : struct
        {
            return Create(new[] { addnullitem }.Concat(collection.Select(item => SelectItem.Create((T?)item.Value, item.Label))));
        }

        public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(DataTable dt) { return Create(DbToEnumerable<T>(dt)); }

        public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(DataTable dt, DataColumn idColumn, DataColumn textColumn)
        {
            return Create(DbToEnumerable<T>(dt, idColumn, textColumn));
        }

        public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(SelectItem<T> addnullitem, DataTable dt) { return CreateWithLeeg(addnullitem, DbToEnumerable<T>(dt)); }

        public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(SelectItem<T> addnullitem, DataTable dt, DataColumn idColumn, DataColumn textColumn)
        {
            return CreateWithLeeg(addnullitem, DbToEnumerable<T>(dt, idColumn, textColumn));
        }

        static IEnumerable<SelectItem<T>> DbToEnumerable<T>(DataTable dt)
        {
            return dt == null ? DbToEnumerable<T>(null, null, null) : DbToEnumerable<T>(dt, dt.Columns[0], dt.Columns[1]);
        }

        static IEnumerable<SelectItem<T>> DbToEnumerable<T>(DataTable dt, DataColumn idColumn, DataColumn textColumn)
        {
            return dt == null
                ? Enumerable.Empty<SelectItem<T>>()
                : dt.Rows.Cast<DataRow>().Select(dr => SelectItem.Create((T)dr[idColumn], Translatable.Raw(dr[textColumn].ToString(), "")));
        }

        public static SelectItem<T> GetItem<T>(this IReadOnlyList<SelectItem<T>> list, T value)
        {
            return (from item in list
                where item.Value.Equals(value)
                select item).SingleOrDefault();
        }

        public static SelectItem<T> GetItem<T>(this IReadOnlyList<SelectItem<T>> list, Taal language, string text)
        {
            return (from item in list
                where item.Label.Translate(language).Text == text
                select item).SingleOrDefault();
        }
    }

    public interface ISelectItem<out T>
    {
        T Value { get; }
        ITranslatable Label { get; }
    }

    public interface IToSelectItem<T>
    {
        SelectItem<T> ToSelectItem();
    }

    public struct SelectItem<T> : ISelectItem<T>
    {
        readonly T v;
        readonly ITranslatable label;
        public T Value => v;
        public ITranslatable Label => label;

        internal SelectItem(T v, ITranslatable label)
        {
            if (label == null) {
                throw new ArgumentNullException(nameof(label));
            }
            this.v = v;
            this.label = label;
        }

        public override string ToString() => "{" + v + ": " + label.Translate(Taal.NL).Text + "}";
    }

    public static class SelectItem
    {
        public static SelectItem<T> Create<T>(T val, ITranslatable text) { return new SelectItem<T>(val, text); }
    }
}
