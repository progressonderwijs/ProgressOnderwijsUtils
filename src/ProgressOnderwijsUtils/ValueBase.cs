using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	/// <summary> 
	/// Helper base class to automatically implement Equals, GetHashCode, ToString()
	/// uses all public+private fields of the object for comparisons
	/// uses public fields and properties for ToString()
	/// Instantiated types must be sealed and pass themselves as the type parameter T.
	/// </summary>
	/// <typeparam name="T">The derived type; must be sealed</typeparam>
	public abstract class ValueBase<T> : IEquatable<T> where T : ValueBase<T>
	{
		//static readonly Func<T, T> copy;
		static readonly Func<T, T, bool> equalsFunc;
		static readonly Func<T, int> hashFunc;
		static readonly Func<T, string> toStringFunc;

		protected ValueBase() { if (!(this is T)) throw new InvalidOperationException("Only T can subclass ValueBase<T>."); }
		static ValueBase()
		{
			try
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
						UnaryExpression ulongHashCodeExpr = Expression.Convert(Expression.Convert(Expression.Call(fieldExpr, GetHashcodeMethod(fi.FieldType)), typeof(uint)), typeof(ulong));
						var scaledHashExpr = Expression.Multiply(Expression.Constant((ulong)(2 * n + 1)), ulongHashCodeExpr);
						return fi.FieldType.IsValueType ? (Expression)scaledHashExpr : Expression.Condition(Expression.Equal(Expression.Default(typeof(object)), fieldExpr), Expression.Constant((ulong)n), scaledHashExpr);
					}).Aggregate((Expression)Expression.Constant(0UL), Expression.Add);
				var storeHashAcc = Expression.Assign(accumulatorVar, accumulateHashExpr);
				var finalHashExpr = Expression.ExclusiveOr(Expression.Convert(accumulatorVar, typeof(int)), Expression.Convert(Expression.RightShift(accumulatorVar, Expression.Constant(32)), typeof(int)));


				hashFunc = Expression.Lambda<Func<T, int>>(Expression.Block(new[] { accumulatorVar }, storeHashAcc, finalHashExpr), parA).Compile();



				toStringFunc = ToStringByMembers<T>.Func;
			}
			catch (Exception e)
			{
				throw new Exception("Failed to create ValueBase for " + ExpressionToCodeLib.ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)), e);
			}
		}


		static MethodInfo GetHashcodeMethod(Type type)
		{
			var objectHashcodeMethod = ((Func<int>)(new object().GetHashCode)).Method;
			var method = type.GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance) ?? objectHashcodeMethod;
			return method.GetBaseDefinition() != objectHashcodeMethod ? objectHashcodeMethod : method;

		}

		public bool Equals(T other) { return other != null && equalsFunc((T)this, other); }
		public override bool Equals(object obj) { return obj is T && Equals((T)obj); }
		public override int GetHashCode() { return hashFunc((T)this); }
		public T Copy() { return (T)MemberwiseClone(); }
		public T CopyWith(Action<T> action) { var copied = Copy(); action(copied); return copied; }


		public override string ToString() { return toStringFunc((T)this); }
	}


	public static class ToStringByMembers<T>
	{
		public static readonly Func<T, string> Func = byPublicMembers();
		static MemberExpression MemberAccessExpression(Expression expr, MemberInfo mi) { return mi is FieldInfo ? Expression.Field(expr, (FieldInfo)mi) : Expression.Property(expr, (PropertyInfo)mi); }

		static Func<T, string> byPublicMembers()
		{
			Type type = typeof(T);
			var refEqMethod = ((Func<object, object, bool>)ReferenceEquals).Method;
			var toStringMethod = typeof(object).GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
			var concatMethod = ((Func<string, string, string>)string.Concat).Method;
			var parA = Expression.Parameter(type, "a");

			var toStringExpr =
				Expression.Call(concatMethod,
					type.GetFields(BindingFlags.Instance | BindingFlags.Public).Concat(
						type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(pi => pi.CanRead && pi.GetGetMethod() != null)
							.Cast<MemberInfo>()
						)
						.Select(fi =>
							Expression.Call(concatMethod,
								Expression.Call(concatMethod,
									Expression.Constant(fi.Name + " = "),
									Expression.Condition(
										Expression.Call(null, refEqMethod, Expression.Convert(MemberAccessExpression(parA, fi), typeof(object)), Expression.Default(typeof(object))),
										Expression.Constant("<NULL>"),
										Expression.Call(MemberAccessExpression(parA, fi), toStringMethod)
										)
									),
								Expression.Constant(", ")
								)
						).Aggregate((Expression)Expression.Constant(type.Name + "{ "), (a, b) => Expression.Call(concatMethod, a, b)),
					Expression.Constant("}")
					);

			return Expression.Lambda<Func<T, string>>(toStringExpr, parA).Compile();
		}

	}
}
