using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// SelectItemList can be used for the SelectInput control. It is just a List of SelectItems with 
	/// two extra constructors to facilitate the use of DataTables
	/// </summary>
	/// <typeparam name="T">Used only Types that SelectInput support, typically int, int? and string</typeparam>
	public sealed class SelectItemList<T> : List<SelectItem<T>>
	{

		public SelectItemList() { }

		public SelectItemList(IEnumerable<SelectItem<T>> collection)
			: base(collection) { }

		public SelectItemList<T> Clone()
		{
			return new SelectItemList<T>(this);
		}

		public override string ToString() { return string.Join("\n", this.Select(item => item.ToString())); }
	}

	public static class SelectItemList
	{

		/// <summary>
		/// Voegt een leeg item bovenaan de lijst toe
		/// </summary>
		public static void ToevoegenLeegItem<T>(this List<SelectItem<T>> selectitemlist, SelectItem<T> addnullitem)
		{
			selectitemlist.Insert(0, addnullitem);
		}

		public static SelectItemList<T> Create<T>(IEnumerable<SelectItem<T>> collection)
		{
			return new SelectItemList<T>(collection);
		}

		public static SelectItemList<T> CreateWithLeeg<T>(SelectItem<T> addnullitem, IEnumerable<SelectItem<T>> collection)
		{
			return new SelectItemList<T>(new[] { addnullitem }.Concat(collection));
		}
		public static SelectItemList<T> CreateWithLeeg<T>(SelectItem<T> addnullitem, SelectItem<T> addnullitem2, IEnumerable<SelectItem<T>> collection)
		{
			return new SelectItemList<T>(new[] { addnullitem,addnullitem2 }.Concat(collection));
		}

		public static SelectItemList<T> CreateFromDb<T>(DataTable dt)
		{
			return new SelectItemList<T>(DbToEnumerable<T>(dt));
		}

		public static SelectItemList<T> CreateFromDb<T>(SelectItem<T> addnullitem, DataTable dt)
		{
			return new SelectItemList<T>(new[] { addnullitem }.Concat(DbToEnumerable<T>(dt)));
		}

		public static IEnumerable<SelectItem<T>> DbToEnumerable<T>(DataTable dt)
		{
			return dt == null ? Enumerable.Empty<SelectItem<T>>() : dt.Rows.Cast<DataRow>().Select(dr => SelectItem.Create((T)(T)dr[0], new TextDefSimple(dr[1].ToString(), "")));
		}

		public static SelectItem<T> GetItem<T>(this List<SelectItem<T>> list, T value)
		{
			return (from item in list
					where item.Value.Equals(value)
					select item).SingleOrDefault();
		}

		public static SelectItem<T> GetItem<T>(this List<SelectItem<T>> list, Taal language, string text)
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