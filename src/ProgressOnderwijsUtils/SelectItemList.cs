using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class SelectItemList
	{
		public static IReadOnlyList<SelectItem<T>> Create<T>(IEnumerable<SelectItem<T>> collection)
		{
			return collection.ToArray();
		}

		public static IReadOnlyList<SelectItem<T>> CreateWithLeeg<T>(SelectItem<T> addnullitem, IEnumerable<SelectItem<T>> collection)
		{
			return Create(new[] { addnullitem }.Concat(collection));
		}

		public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(DataTable dt)
		{
			return Create(DbToEnumerable<T>(dt));
		}

		public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(DataTable dt, DataColumn idColumn, DataColumn textColumn)
		{
			return Create(DbToEnumerable<T>(dt, idColumn, textColumn));
		}

		public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(SelectItem<T> addnullitem, DataTable dt)
		{
			return CreateWithLeeg(addnullitem, DbToEnumerable<T>(dt));
		}

		public static IReadOnlyList<SelectItem<T>> CreateFromDb<T>(SelectItem<T> addnullitem, DataTable dt, DataColumn idColumn, DataColumn textColumn)
		{
			return CreateWithLeeg(addnullitem, DbToEnumerable<T>(dt, idColumn, textColumn));
		}

		static IEnumerable<SelectItem<T>> DbToEnumerable<T>(DataTable dt)
		{
			return DbToEnumerable<T>(dt, dt.Columns[0], dt.Columns[1]);
		}

		static IEnumerable<SelectItem<T>> DbToEnumerable<T>(DataTable dt, DataColumn idColumn, DataColumn textColumn)
		{
			return dt == null ? Enumerable.Empty<SelectItem<T>>() : dt.Rows.Cast<DataRow>().Select(dr => SelectItem.Create((T)dr[idColumn], new TextDefSimple(dr[textColumn].ToString(), "")));
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

	public struct SelectItem<T> : ISelectItem<T>
	{

		readonly T v;
		readonly ITranslatable label;

		public T Value { get { return v; } }
		public ITranslatable Label { get { return label; } }

		internal SelectItem(T v, ITranslatable label)
		{
			if (label == null)
				throw new ArgumentNullException("label");
			this.v = v;
			this.label = label;
		}

		public override string ToString() { return "{" + v + ": " + label.Translate(Taal.NL).Text + "}"; }
	}

	public static class SelectItem
	{
		public static SelectItem<T> Create<T>(T val, ITranslatable text)
		{
			return new SelectItem<T>(val, text);
		}

	}
}