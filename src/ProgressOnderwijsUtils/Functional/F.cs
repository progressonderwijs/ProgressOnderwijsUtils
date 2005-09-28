using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils.Functional {
	public delegate TAcc Combiner<TInput,TAcc>(TInput val, TAcc acc);

	/// <summary>
	/// This code is an implementation of the common (in functional languages) functions Map, Filter and FoldL.  It also contains some other functional experiments.  It's was used
	/// as a possible way to simplify DataTable filtering, but currently I'm (Eamon) not using it because it seems complex and unusual in C# code.  Since the implementation is
	/// so simple though and I'm not sure if it'll be useful I haven't trashed it yet.
	/// </summary>
	public class F {

		public static IEnumerable<TOutputItem> Map<TInputItem, TOutputItem>(Converter<TInputItem, TOutputItem> converter, IEnumerable<TInputItem> list) {
			foreach (TInputItem inp in list) yield return converter(inp);
		}

		public static IEnumerable<TListItem> Concat<TListItem>(IEnumerable<TListItem> firstList, IEnumerable<TListItem> secondList) {
			foreach (TListItem item in firstList) yield return item;
			foreach (TListItem item in secondList) yield return item;
		}

		public static IEnumerable<int> Fibonacci() {	
			int a=0, b=1;
			while (true) {
				yield return b;
				int tmp = a;
				a = b;
				b = tmp + b;
			}
		}
		
		public static CombType FoldL<ListItem, CombType>(Combiner<ListItem, CombType> combiner, CombType acc, IEnumerable<ListItem> list) {
			foreach (ListItem inp in list) acc = combiner(inp, acc);
			return acc;
		}

		public static IEnumerable<TListItem> Filter<TListItem>(Predicate<TListItem> match, IEnumerable<TListItem> list) {
			foreach (TListItem inp in list) if (match(inp)) yield return inp;
		}


	}
}
