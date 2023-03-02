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
        var context = new NullabilityInfoContext();
        var statements = new List<Expression>();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var nullExpression = Expression.Constant(null, typeof(object));
        foreach (var f in fields) {
            var memberExpression = Expression.Field(obj, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));
            if (context.Create(f).WriteState == NullabilityState.NotNull) {
                if (f.FieldType.IsClass && !f.FieldType.FullName.AssertNotNull().StartsWith("System.", StringComparison.Ordinal)) {
                    statements.Add(
                        Expression.IfThenElse(
                            Expression.Equal(fieldValue, nullExpression),
                            Expression.AddAssign(nullCounter, Expression.Constant(1, typeof(int))),
                            Expression.Block(CountInvalidNullOccurrences(f.FieldType, memberExpression, nullCounter))
                        )
                    );
                } else {
                    statements.Add(
                        Expression.IfThen(
                            Expression.Equal(fieldValue, nullExpression),
                            Expression.AddAssign(nullCounter, Expression.Constant(1, typeof(int)))
                        )
                    );
                }
            } else {
                statements.Add(
                    Expression.IfThen(
                        Expression.NotEqual(fieldValue, nullExpression),
                        Expression.Block(CountInvalidNullOccurrences(f.FieldType, memberExpression, nullCounter))
                    )
                );
            }
        }
        return statements;
    }

    static List<Expression> SetArrayValues(Type type, Expression obj, Expression count, Expression exception)
    {
        var context = new NullabilityInfoContext();
        var statements = new List<Expression>();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var nullExpression = Expression.Constant(null, typeof(object));
        foreach (var f in fields) {
            var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
            var exceptionMessage = type.ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";
            var fieldAccessExpression = Expression.Convert(Expression.Field(obj, f), typeof(object));
            if(context.Create(f).WriteState == NullabilityState.NotNull) {
                if (f.FieldType.IsClass && !f.FieldType.FullName.AssertNotNull().StartsWith("System.", StringComparison.Ordinal)) {
                    statements.Add(
                        Expression.IfThenElse(
                            Expression.Equal(fieldAccessExpression, nullExpression),
                            Expression.Block(
                                Expression.Assign(Expression.ArrayAccess(exception, count), Expression.Constant(exceptionMessage)),
                                Expression.AddAssign(count, Expression.Constant(1, typeof(int)))),
                            Expression.Block(SetArrayValues(f.FieldType, Expression.Field(obj, f), count, exception))
                        )
                    );
                } else {
                    statements.Add(
                        Expression.IfThen(
                            Expression.Equal(fieldAccessExpression, nullExpression),
                            Expression.Block(
                                Expression.Assign(Expression.ArrayAccess(exception, count), Expression.Constant(exceptionMessage)),
                                Expression.AddAssign(count, Expression.Constant(1, typeof(int)))
                            )
                        )
                    );
                }
            } else {
                if (f.FieldType.IsClass && !f.FieldType.FullName.AssertNotNull().StartsWith("System.", StringComparison.Ordinal)) {
                    statements.Add(
                        Expression.IfThen(
                            Expression.NotEqual(fieldAccessExpression, nullExpression),
                            Expression.Block(SetArrayValues(f.FieldType, Expression.Field(obj, f), count, exception))
                        )
                    );
                }
            }
        }
        return statements;
    }
}
