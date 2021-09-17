using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using FastExpressionCompiler;

namespace ProgressOnderwijsUtils
{
    public static class ToStringByMembers
    {
        public static string ToStringByPublicMembers<T>(T obj)
            => ToStringByMembers<T>.Func(obj);
    }

    static class StringifierMethod
    {
        internal static readonly MethodInfo? MethodInfo = ((Func<object, string>)ObjectToCode.ComplexObjectToPseudoCode).Method;
    }

    public static class ToStringByMembers<T>
    {
        public static readonly Func<T, string> Func = byPublicMembers();

        static MemberExpression MemberAccessExpression(Expression expr, MemberInfo mi)
            => mi is FieldInfo info ? Expression.Field(expr, info) : Expression.Property(expr, (PropertyInfo)mi);

        static Func<T, string> byPublicMembers()
        {
            var concatMethod = ((Func<string[], string>)string.Concat).Method;

            Expression concatStringExpressions(params Expression[] stringExpressions)
                => Expression.Call(concatMethod, Expression.NewArrayInit(typeof(string), stringExpressions));

            var type = typeof(T);
            var refEqMethod = ((Func<object, object, bool>)ReferenceEquals).Method;
            var toStringMethod = StringifierMethod.MethodInfo ?? throw new InvalidOperationException($"missing {nameof(StringifierMethod)}?");

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
                        )
                    ),
                    Expression.Constant(",\n")
                );

            var toStringExpr =
                concatStringExpressions(
                    new[] { Expression.Constant("new " + type.Name + " {\n"), }
                        .Concat(nonCompilerGeneratedMembers.Select(MemberToStringExpression))
                        .Concat(new[] { Expression.Constant("}") })
                        .ToArray()
                );

            return Expression.Lambda<Func<T, string>>(toStringExpr, parA).CompileFast();
        }

        static string FriendlyMemberName(MemberInfo fi)
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
