using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtils.RequiredFields;

public static class NonNullableFieldVerifier4
{
    //Without any calls but with counters etc based of hardcoded2
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exceptionVar");
        var ErrorCounter = Expression.Variable(typeof(int), "counter");
        statements.Add(Expression.Assign(ErrorCounter, Expression.Constant(0)));

        var context = new NullabilityInfoContext();

        var fields = typeof(T)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();

        var variables = new List<ParameterExpression>();
        var nullConstantExpression = Expression.Constant(null, typeof(object));
        foreach (var f in fields) {
            var memberExpression = Expression.Field(objectParam, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));

            statements.Add(
                Expression.IfThen(
                    Expression.Equal(fieldValue, nullConstantExpression),
                    Expression.AddAssign(ErrorCounter, Expression.Constant(1, typeof(int)))
                )
            );
        }
        var setArray = fields.Select(
            f => {
                var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
                var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";
                var fieldAccessExpression = Expression.Convert(Expression.Field(objectParam, f), typeof(object));
                return Expression.IfThen(
                    Expression.Equal(fieldAccessExpression, nullConstantExpression),
                    Expression.Block(
                        Expression.Assign(Expression.ArrayAccess(exception, ErrorCounter), Expression.Constant(exceptionMessage)),
                        Expression.AddAssign(ErrorCounter, Expression.Constant(1, typeof(int)))
                    )
                );
            }
        );
        var falseState = Expression.Block(
            Expression.Assign(exception, Expression.NewArrayBounds(typeof(string), ErrorCounter)),
            Expression.Assign(ErrorCounter, Expression.Constant(0, typeof(int))),
            Expression.Block(setArray)
        );
        statements.Add(
            Expression.Block(
                Expression.IfThenElse(
                    Expression.Equal(ErrorCounter, Expression.Constant(0, typeof(int))),
                    Expression.Assign(exception, Expression.Constant(null, typeof(string[]))),
                    falseState
                )
            )
        );
        statements.Add(exception);

        variables.AddRange(new[] { exception, ErrorCounter, });

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(variables, statements), objectParam);
        return ToLambda.Compile();
    }
}
