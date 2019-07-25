#nullable disable
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
    public abstract class ValueBase<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
        T> : IEquatable<T>
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
                throw new Exception("Failed to create ValueBase for " + typeof(T).ToCSharpFriendlyTypeName(), e);
            }
        }

        public bool Equals(T other)
            => other != null && FieldwiseEquality<T>.Instance((T)this, other);

        public override bool Equals(object obj)
            => obj is T typed && Equals(typed);

        public override int GetHashCode()
            => FieldwiseHasher<T>.Instance((T)this);

        [NotNull]
        public T Copy()
            => (T)MemberwiseClone();

        [NotNull]
        public T CopyWith([NotNull] Action<T> action)
        {
            var copied = Copy();
            action(copied);
            return copied;
        }

        public override string ToString()
            => ToStringByMembers<T>.Func((T)this);
    }

    public static class ToStringByMembers
    {
        public static string ToStringByPublicMembers<T>(T obj)
            => ToStringByMembers<T>.Func(obj);
    }

    public static class ToStringByMembers<T>
    {
        public static readonly Func<T, string> Func = byPublicMembers();

        [NotNull]
        static MemberExpression MemberAccessExpression(Expression expr, [NotNull] MemberInfo mi)
            => mi is FieldInfo info ? Expression.Field(expr, info) : Expression.Property(expr, (PropertyInfo)mi);

        [UsedImplicitly]
        static string ToString(object o)
            => ObjectToCode.ComplexObjectToPseudoCode(o);

        [NotNull]
        static Func<T, string> byPublicMembers()
        {
            var concatMethod = ((Func<string[], string>)string.Concat).Method;

            Expression concatStringExpressions(params Expression[] stringExpressions)
                => Expression.Call(concatMethod, Expression.NewArrayInit(typeof(string), stringExpressions));

            var type = typeof(T);
            var refEqMethod = ((Func<object, object, bool>)ReferenceEquals).Method;
            var toStringMethod = typeof(ToStringByMembers<T>).GetMethod("ToString", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException("missing ToString?");

            var parA = Expression.Parameter(type, "a");
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var readableProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(pi => pi.CanRead && pi.GetGetMethod() != null);
            var fieldsAndProperties = fields.Cast<MemberInfo>().Concat(readableProperties);
            var nonCompilerGeneratedMembers = fieldsAndProperties.Where(fi => !fi.Name.StartsWith("<", StringComparison.Ordinal));

            var replaceMethod = ((Func<string, string, string>)"".Replace).Method;

            Expression MemberToStringExpression(MemberInfo fi)
                => concatStringExpressions(
                    Expression.Constant("    " + FriendlyMemberName(fi) + " = "),
                    Expression.Condition(
                        Expression.Call(refEqMethod, Expression.Convert(MemberAccessExpression(parA, fi), typeof(object)), Expression.Default(typeof(object))),
                        Expression.Constant("null"),
                        Expression.Call(
                            Expression.Coalesce(
                                Expression.Call(toStringMethod, Expression.Convert(MemberAccessExpression(parA, fi), typeof(object))),
                                Expression.Constant("")
                            ),
                            replaceMethod,
                            Expression.Constant("\n"),
                            Expression.Constant("\n    ")
                        )),
                    Expression.Constant(",\n")
                );

            var toStringExpr =
                concatStringExpressions(
                    new[] { Expression.Constant("new " + type.Name + " {\n"), }
                        .Concat(nonCompilerGeneratedMembers.Select(MemberToStringExpression))
                        .Concat(new[] { Expression.Constant("}") })
                        .ToArray()
                );

            return Expression.Lambda<Func<T, string>>(toStringExpr, parA).Compile();
        }

        [NotNull]
        static string FriendlyMemberName([NotNull] MemberInfo fi)
        {
            bool isPublic;
            if (fi is FieldInfo fieldinfo) {
                isPublic = fieldinfo.Attributes.HasFlag(FieldAttributes.Public);
            } else {
                var propertyinfo = (PropertyInfo)fi;
                isPublic = propertyinfo.GetGetMethod(false) != null;
            }

            return isPublic ? fi.Name : "*" + fi.Name;
        }
    }
}
