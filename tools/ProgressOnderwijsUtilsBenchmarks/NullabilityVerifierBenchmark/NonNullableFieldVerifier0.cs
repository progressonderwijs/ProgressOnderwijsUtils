using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;

public static class NonNullableFieldVerifier0
{
    public static Func<T, string> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exceptionVar = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(exceptionVar, Expression.Constant("")));

        NullabilityInfoContext context = new();
        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();
        var concatCall = ((Func<string, string, string>)string.Concat).Method;
        statements.AddRange(
            fields.Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
                .Select(
                    f => {
                        var memberExpression = Expression.Field(objectParam, f);
                        var fieldValue = Expression.Convert(memberExpression, typeof(object));

                        var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
                        var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable\n";

                        return Expression.IfThen(
                            Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                            Expression.Assign(
                                exceptionVar,
                                Expression.Call(
                                    concatCall,
                                    exceptionVar,
                                    Expression.Constant(exceptionMessage)
                                )
                            )
                        );
                    }
                )
        );

        statements.Add(exceptionVar);
        var ToLambda = Expression.Lambda<Func<T, string>>(Expression.Block(new[] { exceptionVar, }, statements), objectParam);
        return ToLambda.Compile();
    }
}
