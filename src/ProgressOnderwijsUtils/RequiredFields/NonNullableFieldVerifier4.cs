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

        NullabilityInfoContext context = new();

        var fields = typeof(T).GetFields(BindingFlags.NonPublic|BindingFlags.Public |BindingFlags.Instance).Where(f => context.Create(f).WriteState == NullabilityState.NotNull);

        var variables = new List<ParameterExpression>();
        foreach (var f in fields) {
            var memberExpression = Expression.Field(objectParam, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));

            statements.Add(
                Expression.IfThen(
                        Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                        Expression.AddAssign(ErrorCounter, Expression.Constant(1, typeof(int)))
                )
            );
        }
        var setArray = fields.Select(
            f => Expression.IfThen(
                Expression.Equal(Expression.Convert(Expression.Field(objectParam, f), typeof(object)), Expression.Constant(null, typeof(object))),
                Expression.Block(
                    Expression.Assign(Expression.ArrayAccess(exception, ErrorCounter), Expression.Constant("Found null value in non nullable field in " + typeof(T) + "." + (AutoPropertyOfFieldOrNull(f) == null ? f.Name : AutoPropertyOfFieldOrNull(f)?.Name))),
                    Expression.AddAssign(ErrorCounter, Expression.Constant(1, typeof(int)))
                )
            )
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