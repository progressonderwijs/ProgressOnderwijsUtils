using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtils.RequiredFields;

public static class NonNullableFieldVerifier5
{
    //Copy of NonNullableFieldVerifier4 except also verifies nested objects within T
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exceptionVar");
        var ErrorCounter = Expression.Variable(typeof(int), "counter");
        statements.Add(Expression.Assign(ErrorCounter, Expression.Constant(0)));
        statements.AddRange(CountInvalidNullOccurrences(typeof(T), objectParam, ErrorCounter));

        var falseState = Expression.Block(
            Expression.Assign(exception, Expression.NewArrayBounds(typeof(string), ErrorCounter)),
            Expression.Assign(ErrorCounter, Expression.Constant(0, typeof(int))),
            Expression.Block(SetArrayValues(typeof(T), objectParam, ErrorCounter, exception))
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

        var variables = new List<ParameterExpression>();
        variables.AddRange(new[] { exception, ErrorCounter, });

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(variables, statements), objectParam);
        return ToLambda.Compile();
    }

    static List<Expression> CountInvalidNullOccurrences(Type type, Expression obj, Expression nullCounter)
    {
        NullabilityInfoContext context = new();

        var statements = new List<Expression>();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(f => context.Create(f).WriteState == NullabilityState.NotNull);

        foreach (var f in fields) {
            var memberExpression = Expression.Field(obj, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));

            statements.Add(
                Expression.IfThen(
                    Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                    Expression.AddAssign(nullCounter, Expression.Constant(1, typeof(int)))
                )
            );
            if (f.FieldType.IsClass && !f.FieldType.FullName.AssertNotNull().StartsWith("System.", StringComparison.Ordinal)) {
                statements.AddRange(CountInvalidNullOccurrences(f.FieldType, memberExpression, nullCounter));
            }
        }

        return statements;
    }

    static List<Expression> SetArrayValues(Type type, Expression obj, Expression count, Expression exception)
    {
        NullabilityInfoContext context = new();

        var statements = new List<Expression>();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(f => context.Create(f).WriteState == NullabilityState.NotNull);
        foreach (var f in fields) {
            statements.Add(
                Expression.IfThen(
                    Expression.Equal(Expression.Convert(Expression.Field(obj, f), typeof(object)), Expression.Constant(null, typeof(object))),
                    Expression.Block(
                        Expression.Assign(Expression.ArrayAccess(exception, count), Expression.Constant("Found null value in non nullable field in " + type + "." + (AutoPropertyOfFieldOrNull(f) == null ? f.Name : AutoPropertyOfFieldOrNull(f)?.Name))),
                        Expression.AddAssign(count, Expression.Constant(1, typeof(int)))
                    )
                )
            );
            if (f.FieldType.IsClass && !f.FieldType.FullName.AssertNotNull().StartsWith("System.", StringComparison.Ordinal)) {
                statements.AddRange(SetArrayValues(f.FieldType, Expression.Field(obj, f), count, exception));
            }
        }
        return statements;
    }
}
