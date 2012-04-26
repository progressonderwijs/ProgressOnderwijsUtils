using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	public abstract class ValueClass<T> : IEquatable<T>, IMetaObject where T : ValueClass<T>, new()
	{
		//static readonly Func<T, T> copy;
		static readonly Func<T, T, bool> equalsFunc;
		static readonly Func<T, int> hashFunc;
		static readonly Func<T, string> toStringFunc;

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

			var toStringMethod = typeof(object).GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
			var refEqMethod = ((Func<object, object, bool>)ReferenceEquals).Method;
			var concatMethod = ((Func<string, string, string>)string.Concat).Method;

			Func<Expression, MemberInfo, Expression> membAccess = (expr, mi) => mi is FieldInfo ? Expression.Field(expr, (FieldInfo)mi) : Expression.Property(expr, (PropertyInfo)mi);

			var toStringExpr =
				Expression.Call(concatMethod,
					fields.Where(fi=>fi.IsPublic).Concat(
						type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(pi=>pi.CanRead && pi.GetGetMethod() !=null)
						.Cast<MemberInfo>()
					)
					.Select(fi =>
						Expression.Call(concatMethod,
							Expression.Call(concatMethod,
								Expression.Constant(fi.Name + " = "),

								Expression.Condition(
									Expression.Call(null, refEqMethod, Expression.Convert(membAccess(parA, fi), typeof(object)), Expression.Default(typeof(object))),
									Expression.Constant("<NULL>"),
									Expression.Call(membAccess(parA, fi), toStringMethod)
								)
							),
							Expression.Constant(", ")
						)
					).Aggregate((Expression)Expression.Constant(type.Name + "{ "), (a, b) => Expression.Call(concatMethod, a, b)),
					Expression.Constant("}")
				);

			toStringFunc = Expression.Lambda<Func<T, string>>(toStringExpr, parA).Compile();
		}

		public bool Equals(T other) { return other != null && equalsFunc((T)this, other); }
		public override bool Equals(object obj) { return obj is T && Equals((T)obj); }
		public override int GetHashCode() { return hashFunc((T)this); }


		public T Copy() { return (T)MemberwiseClone(); }
		public T CopyWith(Action<T> action) { var copied = Copy(); action(copied); return copied; }
		public override string ToString() { return toStringFunc((T)this); }
	}
}
