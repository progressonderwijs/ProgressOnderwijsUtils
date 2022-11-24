using System.Linq.Expressions;

namespace ProgressOnderwijsUtils.Tests;

public static class NonNullableFieldVerifier1
{

    static readonly Func<string, StringSplitOptions, string[]> x = "".Split;
    //string split
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(exception, Expression.Constant("")));

        NullabilityInfoContext context = new();
        var concatCall = ((Func<string, string, string>)string.Concat).Method;

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
                                exception,
                                Expression.Call(
                                    concatCall,
                                    exception,
                                    Expression.Constant("Found null value in non nullable field in " + typeof(T) + "." + f.Name + Environment.NewLine)
                                )
                            )
                        );
                    }
                )
        );
        
        var splitStringCall = x.Method;

        var exceptionList = Expression.Variable(typeof(string[]), "exceptionList");
        var result = Expression.Call(exception, splitStringCall, Expression.Constant(Environment.NewLine), Expression.Constant(StringSplitOptions.RemoveEmptyEntries));
        statements.Add(Expression.Assign(exceptionList,result));
        statements.Add(Expression.Condition(
            Expression.GreaterThan(
                Expression.ArrayLength(exceptionList),
                Expression.Constant(0)
                ),
            exceptionList,
            Expression.Constant(null,typeof(string[]))
            )
        );

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] {exception, exceptionList, }, statements), objectParam);
        return ToLambda.Compile();
    }
}
