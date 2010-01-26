using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class Tuple
	{
		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 Item1, T2 Item2, T3 Item3, T4 Item4, T5 Item5, T6 Item6) { return new Tuple<T1, T2, T3, T4, T5, T6>(Item1, Item2, Item3, Item4, Item5, Item6); }
		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 Item1, T2 Item2, T3 Item3, T4 Item4, T5 Item5) { return new Tuple<T1, T2, T3, T4, T5>(Item1, Item2, Item3, Item4, Item5); }
		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 Item1, T2 Item2, T3 Item3, T4 Item4) { return new Tuple<T1, T2, T3, T4>(Item1, Item2, Item3, Item4); }
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 Item1, T2 Item2, T3 Item3) { return new Tuple<T1, T2, T3>(Item1, Item2, Item3); }
		public static Tuple<T1, T2> Create<T1, T2>(T1 Item1, T2 Item2) { return new Tuple<T1, T2>(Item1, Item2); }
	}

	public struct Tuple<T1, T2>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public Tuple(T1 Item1, T2 Item2)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
		}
		public static implicit operator Tuple<T1, object>(Tuple<T1, T2> t) { return new Tuple<T1, object>(t.Item1, t.Item2); } //in C# 4.0, this won't be necessary, we'll just mark T1+T2 as covariant, i.Item5. as "out" types.
		public static implicit operator Tuple<object, T2>(Tuple<T1, T2> t) { return new Tuple<object, T2>(t.Item1, t.Item2); }
	}

	public struct Tuple<T1, T2, T3>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public readonly T3 Item3;
		public Tuple(T1 Item1, T2 Item2, T3 Item3)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
			this.Item3 = Item3;
		}
	}

	public struct Tuple<T1, T2, T3, T4>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public readonly T3 Item3;
		public readonly T4 Item4;
		public Tuple(T1 Item1, T2 Item2, T3 Item3, T4 Item4)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
			this.Item3 = Item3;
			this.Item4 = Item4;
		}
	}

	public struct Tuple<T1, T2, T3, T4, T5>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public readonly T3 Item3;
		public readonly T4 Item4;
		public readonly T5 Item5;
		public Tuple(T1 Item1, T2 Item2, T3 Item3, T4 Item4, T5 Item5)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
			this.Item3 = Item3;
			this.Item4 = Item4;
			this.Item5 = Item5;
		}
	}
	public struct Tuple<T1, T2, T3, T4, T5, T6>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public readonly T3 Item3;
		public readonly T4 Item4;
		public readonly T5 Item5;
		public readonly T6 Item6;
		public Tuple(T1 Item1, T2 Item2, T3 Item3, T4 Item4, T5 Item5, T6 Item6)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
			this.Item3 = Item3;
			this.Item4 = Item4;
			this.Item5 = Item5;
			this.Item6 = Item6;
		}
	}
}
