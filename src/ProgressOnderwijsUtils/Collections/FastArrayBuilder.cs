using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Collections
{
	public interface IArrayBuilder<T>
	{
		void Add(T item);
		T[] ToArray();
	}

	public struct FastArrayBuilder<T> : IArrayBuilder<T>
	{
		const int InitSize2Pow = 4;
		const int InitSize = (1 << InitSize2Pow) - 1;
		int idx, sI;
		T[] current;
		T[][] segments;

		public static FastArrayBuilder<T> Create() { return new FastArrayBuilder<T> { current = new T[InitSize] }; }

		public void Add(T item)
		{
			if (idx < current.Length)
				current[idx++] = item;
			else
			{
				if (segments == null)
					segments = new T[31 - InitSize2Pow][];
				segments[sI++] = current;
				current = new T[(current.Length << 1) & ~current.Length];
				current[0] = item;
				idx = 1;
			}
		}

		public T[] ToArray()
		{
			if (segments == null)
			{
				T[] retval = current;
				Array.Resize(ref retval, idx);
				return retval;
			}
			else
			{
				int sumlength = (1 << (sI + InitSize2Pow - 1)) + idx - 1;
				var retval = new T[sumlength];
				int j = 0;
				for (int sJ = 0; sJ < sI; sJ++)
				{
					var subarr = segments[sJ];
					subarr.CopyTo(retval, j);
					j += subarr.Length;
				}
				Array.Copy(current, 0, retval, j, idx);
				return retval;
			}
		}
	}

#if false
	public static class FasterToArray
	{
		const int InitSize2Pow = 4;
		const int InitSize = 1 << InitSize2Pow;
		public static T[] FastToArray2<T>(this IEnumerable<T> seq)
		{
			int idx = 0, sumlength = 0;
			var arr = new T[InitSize];
			SList<T[]> list = SList<T[]>.Empty;
			foreach (var x in seq)
			{
				if (idx >= arr.Length)
				{
					list = list.Prepend(arr);
					arr = new T[arr.Length * 2];
					sumlength += idx;
					idx = 0;
				}

				arr[idx++] = x;
			}

			if (!list.IsEmpty)
			{
				sumlength += idx;
				var retval = new T[sumlength];
				int j = 0;
				for (list = list.Reverse(); !list.IsEmpty; list = list.Tail)
				{
					var subarr = list.Head;
					subarr.CopyTo(retval, j);
					j += subarr.Length;
				}
				for (int k = 0; j < sumlength; k++)
					retval[j++] = arr[k];
				return retval;
			}
			else
			{
				var retval = new T[idx];
				for (int k = 0; k < idx; k++)
					retval[k] = arr[k];
				return retval;
			}
		}

		static T[] FastToArrayHelper<T, TEnum>(TEnum enumerator, T[] arr) where TEnum : IEnumerator<T>
		{
			T[][] segments = new T[32][];
			segments[0] = arr;
			int sI = 1;
			arr = new T[arr.Length << 1];
			arr[0] = enumerator.Current;
			int idx = 1;
			while (enumerator.MoveNext())
			{
				if (idx >= arr.Length)
				{
					segments[sI++] = arr;
					arr = new T[arr.Length << 1];
					idx = 0;
				}
				arr[idx++] = enumerator.Current;
			}
			int sumlength = (1 << (sI + InitSize2Pow)) - InitSize + idx;
			var retval = new T[sumlength];
			int j = 0;
			for (int sJ = 0; sJ < sI; sJ++)
			{
				var subarr = segments[sJ];
				//			for(int k=0;k<subarr.Length;k++)
				//				retval[j++]=subarr[k];
				subarr.CopyTo(retval, j);
				j += subarr.Length;
			}
			Array.Copy(arr, 0, retval, j, idx);
			//		for(int k=0;j<sumlength;k++)
			//			retval[j++]=arr[k];
			return retval;
		}

		public static T[] FastToArray<T>(this IEnumerable<T> seq)
		{
			using (var enumerator = seq.GetEnumerator())
				return FastToArray<T, IEnumerator<T>>(enumerator);
		}

		public static T[] FastToArray<T, TEnum>(TEnum enumerator) where TEnum : IEnumerator<T>
		{
			if (!enumerator.MoveNext())
				return new T[0];
			else
			{
				var arr = new T[InitSize];
				int idx = 0;
				do
				{
					if (idx >= arr.Length)
						return FastToArrayHelper(enumerator, arr);
					arr[idx++] = enumerator.Current;
				} while (enumerator.MoveNext());
				Array.Resize(ref arr, idx);
				return arr;
				//				var retval = new T[idx];
				//				for(int k=0;k<idx;k++)
				//					retval[k]=arr[k];
				//				return retval;
			}
		}
	}
#endif
}
