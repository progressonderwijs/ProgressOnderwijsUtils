using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;


public static class NonNullableFieldVerifier5
{
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exception");

        NullabilityInfoContext context = new();
        var fields = typeof(T).GetFields(BindingFlags.NonPublic|BindingFlags.Public |BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();

        var conditionalExpressions = new List<ConditionalExpression>(
            fields.Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
                .Select(
                    f => {
                        var memberExpression = Expression.Field(objectParam, f);
                        var fieldValue = Expression.Convert(memberExpression, typeof(object));

                        var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
                        var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";

                        return Expression.Condition(
                            Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                            Expression.Constant(exceptionMessage),
                            Expression.Constant(null, typeof(string))
                        );
                    }
                )
        );

        var ToNewArrayCall = ((Func<string[], string[]?>)ToNewArrayWithoutNulls).Method;

        statements.Add(Expression.Assign(exception, Expression.Call(ToNewArrayCall, Expression.NewArrayInit(typeof(string), conditionalExpressions))));
        statements.Add(exception);

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] { exception, }, statements), objectParam);
        return ToLambda.Compile();
    }

    public static string[]? ToNewArrayWithoutNulls(string[] oldArray)
    {
        var newArray = oldArray.WhereNotNull().ToArray();
        return newArray.Length == 0 ? null : newArray;
    }
}
