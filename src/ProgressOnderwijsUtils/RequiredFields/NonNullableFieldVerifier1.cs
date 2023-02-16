using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtils.RequiredFields;

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

        var fields = typeof(T).GetFields(BindingFlags.NonPublic|BindingFlags.Public |BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();
        statements.AddRange(
            fields.Select(
                    f => {
                        var memberExpression = Expression.Field(objectParam, f);
                        var fieldValue = Expression.Convert(memberExpression, typeof(object));

                        var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
                        var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable\n";

                        return Expression.IfThen(
                            Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                            Expression.Assign(
                                exception,
                                Expression.Call(
                                    concatCall,
                                    exception,
                                    Expression.Constant(exceptionMessage)
                                )
                            )
                        );
                    }
                )
        );

        var splitStringCall = x.Method;

        var exceptionList = Expression.Variable(typeof(string[]), "exceptionList");
        var result = Expression.Call(exception, splitStringCall, Expression.Constant("\n"), Expression.Constant(StringSplitOptions.RemoveEmptyEntries));
        statements.Add(Expression.Condition(
            Expression.NotEqual(
                exception,
                Expression.Constant("")
                ),
            Expression.Assign(exceptionList,result),
            Expression.Assign(exceptionList,Expression.Constant(null,typeof(string[]))
            ))
        );
        statements.Add(exceptionList);
        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] {exception, exceptionList, }, statements), objectParam);
        return ToLambda.Compile();
    }
}
