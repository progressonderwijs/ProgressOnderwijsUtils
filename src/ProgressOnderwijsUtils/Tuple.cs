using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class TupleF
	{
		public static Tuple<A, B, C, D, E, F> Create<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f) { return new Tuple<A, B, C, D, E, F>(a, b, c, d, e, f); }
		public static Tuple<A, B, C, D, E> Create<A, B, C, D, E>(A a, B b, C c, D d, E e) { return new Tuple<A, B, C, D, E>(a, b, c, d, e); }
		public static Tuple<A, B, C, D> Create<A, B, C, D>(A a, B b, C c, D d) { return new Tuple<A, B, C, D>(a, b, c, d); }
		public static Tuple<A, B, C> Create<A, B, C>(A a, B b, C c) { return new Tuple<A, B, C>(a, b, c); }
		public static Tuple<A, B> Create<A, B>(A a, B b) { return new Tuple<A, B>(a, b); }
	}

	public struct Tuple<A, B>
	{
		public A a;
		public B b;
		public Tuple(A a, B b)
		{
			this.a = a;
			this.b = b;
		}
	}

	public struct Tuple<A, B, C>
	{
		public A a;
		public B b;
		public C c;
		public Tuple(A a, B b, C c)
		{
			this.a = a;
			this.b = b;
			this.c = c;
		}
	}

	public struct Tuple<A, B, C, D>
	{
		public A a;
		public B b;
		public C c;
		public D d;
		public Tuple(A a, B b, C c, D d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}
	}

	public struct Tuple<A, B, C, D, E>
	{
		public A a;
		public B b;
		public C c;
		public D d;
		public E e;
		public Tuple(A a, B b, C c, D d, E e)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
			this.e = e;
		}
	}
	public struct Tuple<A, B, C, D, E, F>
	{
		public A a;
		public B b;
		public C c;
		public D d;
		public E e;
		public F f;
		public Tuple(A a, B b, C c, D d, E e, F f)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
			this.e = e;
			this.f = f;
		}
	}
}
