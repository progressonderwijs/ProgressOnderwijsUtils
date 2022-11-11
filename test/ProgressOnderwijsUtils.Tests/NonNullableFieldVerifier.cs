using System.Linq.Expressions;

namespace ProgressOnderwijsUtils.Tests;

public static class NonNullableFieldVerifier
{
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exception");

        NullabilityInfoContext context = new();
        var fields = typeof(T).GetFields();

        var conditionalExpressions = new List<ConditionalExpression>(
            fields.Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
                .Select(
                    f => {
                        var memberExpression = Expression.Field(objectParam, f);
                        var fieldValue = Expression.Convert(memberExpression, typeof(object));
                        return Expression.Condition(
                            Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                            Expression.Constant("Found null value in non nullable field in " + typeof(T) + "." + f.Name + Environment.NewLine),
                            Expression.Constant(null, typeof(string))
                        );
                    }
                )
        );

        var ToNewArrayCall = ((Func<string[], string[]?>)ToNewArrayWithoutNulls).Method;

        statements.Add(Expression.Assign(exception, Expression.Call(ToNewArrayCall, Expression.NewArrayInit(typeof(string), conditionalExpressions))));
        statements.Add(exception);

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] { exception }, statements), objectParam);
        return ToLambda.Compile();
    }

    public static string[]? ToNewArrayWithoutNulls(string[] oldArray)
    {
        var newArray = oldArray.WhereNotNull().ToArray();
        return newArray.Length == 0 ? null : newArray;
    }
}

public sealed class NullablityTestClass
{
    public string SomeNullString = null;
    public string? SomeNullableField = null;
    public object SomeObject = null;
    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null;
    public object[] SomeFilledObjectArray = { null };
}
