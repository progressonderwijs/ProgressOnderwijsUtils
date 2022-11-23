using System.Linq.Expressions;

namespace ProgressOnderwijsUtils.Tests;

public static class NonNullableFieldVerifier0
{
    public static Func<T, string> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exceptionVar = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(exceptionVar, Expression.Constant("")));

        NullabilityInfoContext context = new();
        var fields = typeof(T).GetFields();
        statements.AddRange(
            fields.Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
                .Select(
                    f => {
                        var memberExpression = Expression.Field(objectParam, f);
                        var fieldValue = Expression.Convert(memberExpression, typeof(object));
                        return Expression.IfThen(
                            Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                            Expression.Assign(
                                exceptionVar,
                                Expression.Call(
                                    typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }),
                                    exceptionVar,
                                    Expression.Constant("Found null value in non nullable field in " + typeof(T) + "." + f.Name + "\n")
                                )
                            )
                        );
                    }
                )
        );

        statements.Add(exceptionVar);
        var ToLambda = Expression.Lambda<Func<T, string>>(Expression.Block(new[] { exceptionVar }, statements), objectParam);
        return ToLambda.Compile();
    }
}
