using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ValueUtils;

namespace ProgressOnderwijsUtils
{
    /// <summary> 
    /// Helper base class to automatically implement Equals, GetHashCode, ToString()
    /// uses all public+private fields of the object for comparisons
    /// uses public fields and properties for ToString()
    /// Instantiated types must be sealed and pass themselves as the type parameter T.
    /// </summary>
    /// <typeparam name="T">The derived type; must be sealed</typeparam>
    [Serializable]
    public abstract class ValueBase<T> : IEquatable<T>
        where T : ValueBase<T>
    {
        protected ValueBase()
        {
            if (!(this is T)) {
                throw new InvalidOperationException("Only T can subclass ValueBase<T>.");
            }
        }

        static ValueBase()
        {
            try {
                if (!typeof(T).IsSealed) {
                    throw new InvalidOperationException("Value Classes must be sealed.");
                }
            } catch (Exception e) {
                throw new Exception("Failed to create ValueBase for " + ObjectToCode.ToCSharpFriendlyTypeName(typeof(T)), e);
            }
        }

        public bool Equals(T other) => other != null && FieldwiseEquality<T>.Instance((T)this, other);
        public override bool Equals(object obj) => obj is T && Equals((T)obj);
        public override int GetHashCode() => FieldwiseHasher<T>.Instance((T)this);
        public T Copy() => (T)MemberwiseClone();

        public T CopyWith(Action<T> action)
        {
            var copied = Copy();
            action(copied);
            return copied;
        }

        public override string ToString() => ToStringByMembers<T>.Func((T)this);
    }

    public static class ToStringByMembers
    {
        public static string ToStringByPublicMembers<T>(T obj) => ToStringByMembers<T>.Func(obj);
    }

    public static class ToStringByMembers<T>
    {
        public static readonly Func<T, string> Func = byPublicMembers();

        static MemberExpression MemberAccessExpression(Expression expr, MemberInfo mi)
        {
            return mi is FieldInfo ? Expression.Field(expr, (FieldInfo)mi) : Expression.Property(expr, (PropertyInfo)mi);
        }

        [UsedImplicitly]
        static string ToString(object o) => ObjectToCode.ComplexObjectToPseudoCode(o);

        static Func<T, string> byPublicMembers()
        {
            Type type = typeof(T);
            var refEqMethod = ((Func<object, object, bool>)ReferenceEquals).Method;
            var toStringMethod = typeof(ToStringByMembers<T>).GetMethod("ToString", BindingFlags.Static | BindingFlags.NonPublic);
            var concatMethod = ((Func<string, string, string>)string.Concat).Method;
            var parA = Expression.Parameter(type, "a");

            var toStringExpr =
                Expression.Call(
                    concatMethod,
                    type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Concat(
                        type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(pi => pi.CanRead && pi.GetGetMethod() != null)
                            .Cast<MemberInfo>()
                        )
                        .Where(fi => !fi.Name.StartsWith("<"))
                        .Select(
                            fi =>
                                Expression.Call(
                                    concatMethod,
                                    Expression.Call(
                                        concatMethod,
                                        Expression.Constant(FriendlyMemberName(fi) + " = "),
                                        Expression.Condition(
                                            Expression.Call(
                                                null,
                                                refEqMethod,
                                                Expression.Convert(MemberAccessExpression(parA, fi), typeof(object)),
                                                Expression.Default(typeof(object))),
                                            Expression.Constant("null"),
                                            Expression.Call(toStringMethod, Expression.Convert(MemberAccessExpression(parA, fi), typeof(object)))
                                            )
                                        ),
                                    Expression.Constant(", ")
                                    )
                        ).Aggregate((Expression)Expression.Constant("new " + type.Name + " { "), (a, b) => Expression.Call(concatMethod, a, b)),
                    Expression.Constant("}")
                    );

            return Expression.Lambda<Func<T, string>>(toStringExpr, parA).Compile();
        }

        static string FriendlyMemberName(MemberInfo fi)
        {
            bool isPublic;
            if (fi is FieldInfo) {
                var fieldinfo = (FieldInfo)fi;
                isPublic = fieldinfo.Attributes.HasFlag(FieldAttributes.Public);
            } else {
                var propertyinfo = (PropertyInfo)fi;
                isPublic = propertyinfo.GetGetMethod(false) != null;
            }

            return isPublic ? fi.Name : "*" + fi.Name;
        }
    }
}
