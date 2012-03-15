using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	public abstract class ValueClass<T> : IEquatable<T>, IMetaObject where T : ValueClass<T>
	{
		//static readonly Func<T, T> copy;
		static readonly Func<T, T, bool> equalsFunc;
		static readonly Func<T, int> hashFunc;

#if DEBUG
		protected ValueClass() { if (!(this is T)) throw new InvalidOperationException("Only T can subclass ValueClass<T>."); }
#endif
		static ValueClass()
		{
			Type type = typeof(T);
			if (!type.IsSealed)
				throw new InvalidOperationException("Value Classes must be sealed.");

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			var parA = Expression.Parameter(type, "a");
			var parB = Expression.Parameter(type, "b");
			var areAllFieldsEqualExpr =
				fields.Select(fi => Expression.Equal(Expression.Field(parA, fi), Expression.Field(parB, fi)))
				.Aggregate((Expression)Expression.Constant(true), Expression.AndAlso);
			equalsFunc = Expression.Lambda<Func<T, T, bool>>(areAllFieldsEqualExpr, parA, parB).Compile();

			var accumulatorVar = Expression.Variable(typeof(ulong), "hashcodeAccumulator");
			var accumulateHashExpr =
				fields.Select((fi, n) => {
					MemberExpression fieldExpr = Expression.Field(parA, fi);
					UnaryExpression ulongHashCodeExpr = Expression.Convert(Expression.Convert(Expression.Call(fieldExpr, fi.FieldType.GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance)), typeof(uint)), typeof(ulong));
					var scaledHashExpr = Expression.Multiply(Expression.Constant((ulong)(2 * n + 1)), ulongHashCodeExpr);
					return fi.FieldType.IsValueType ? (Expression)scaledHashExpr : Expression.Condition(Expression.Equal(Expression.Default(typeof(object)), fieldExpr), Expression.Constant((ulong)n), scaledHashExpr);
				}).Aggregate((Expression)Expression.Constant(0UL), Expression.Add);
			var storeHashAcc = Expression.Assign(accumulatorVar, accumulateHashExpr);
			var finalHashExpr = Expression.ExclusiveOr(Expression.Convert(accumulatorVar, typeof(int)), Expression.Convert(Expression.RightShift(accumulatorVar, Expression.Constant(32)), typeof(int)));


			hashFunc = Expression.Lambda<Func<T, int>>(Expression.Block(new[] { accumulatorVar }, storeHashAcc, finalHashExpr), parA).Compile();
			//Expression.Call Expression.Equal(Expression.Field(parA, fi), Expression.Field(parB, fi)))

		}

		public bool Equals(T other) { return other != null && equalsFunc((T)this, other); }
		public override bool Equals(object obj) { return obj is T && Equals((T)obj); }
		public override int GetHashCode() { return hashFunc((T)this); }


		public T Copy() { return (T)MemberwiseClone(); }
		public T CopyWith(Action<T> action) { var copied = Copy(); action(copied); return copied; }
	}
}
