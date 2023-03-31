using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtils.RequiredFields;

public static class NonNullableFieldVerifier
{
    static class Cache<T>
    {
        public static readonly Func<T, string[]?>? checker;
        public static readonly Exception? exception;

        static Cache()
        {
            try {
                checker = CreateDelegate();
            } catch (Exception e) {
                exception = e;
            }
        }

        //Without any calls but with counters etc based of hardcoded2
        static Func<T, string[]?> CreateDelegate()
        {
            var statements = new List<Expression>();
            var objectParam = Expression.Parameter(typeof(T), "obj");
            var exceptionVar = Expression.Variable(typeof(string[]), "exceptionVar");
            var errorCounterVar = Expression.Variable(typeof(int), "errorCounterVar");
            statements.Add(Expression.Assign(errorCounterVar, Expression.Constant(0)));

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
                        Expression.AddAssign(errorCounterVar, Expression.Constant(1, typeof(int)))
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
                            Expression.Assign(Expression.ArrayAccess(exceptionVar, errorCounterVar), Expression.Constant(exceptionMessage)),
                            Expression.AddAssign(errorCounterVar, Expression.Constant(1, typeof(int)))
                        )
                    );
                }
            );
            var falseState = Expression.Block(
                typeof(string[]),
                new[] { exceptionVar, },
                new Expression[] {
                        Expression.Assign(exceptionVar, Expression.NewArrayBounds(typeof(string), errorCounterVar)),
                        Expression.Assign(errorCounterVar, Expression.Constant(0, typeof(int))),
                    }.Concat(setArray)
                    .Concat(new[] { exceptionVar, })
            );
            statements.Add(
                Expression.Condition(
                    Expression.Equal(errorCounterVar, Expression.Constant(0, typeof(int))),
                    Expression.Constant(null, typeof(string[])),
                    falseState
                )
            );

            variables.AddRange(new[] { exceptionVar, errorCounterVar, });

            var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(variables, statements), objectParam);
            return ToLambda.Compile();
        }
    }

    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
        => Cache<T>.checker ?? throw new("Failed to construct nullability verifier", Cache<T>.exception);
}
