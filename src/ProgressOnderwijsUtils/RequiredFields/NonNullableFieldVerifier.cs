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
            var nullConstantExpression = Expression.Constant(null, typeof(object));

            var context = new NullabilityInfoContext();

            var fields = typeof(T)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
                .ToArray();

            IEnumerable<Expression> ForEachInvalidNull(Func<FieldInfo, Expression> func)
                => fields.Select(field => Expression.IfThen(Expression.Equal(Expression.Convert(Expression.Field(objectParam, field), typeof(object)), nullConstantExpression), func(field)));

            var variables = new List<ParameterExpression>();
            var incrementErrorCounter = Expression.AddAssign(errorCounterVar, Expression.Constant(1, typeof(int)));
            statements.AddRange(ForEachInvalidNull(_ => incrementErrorCounter));

            var setArray = ForEachInvalidNull(
                field => {
                    var memberName = AutoPropertyOfFieldOrNull(field) is { } autoProp ? autoProp.Name : field.Name;
                    var exceptionMessage1 = $"{typeof(T).ToCSharpFriendlyTypeName()}.{memberName} contains NULL despite being non-nullable";
                    return Expression.Block(
                        Expression.Assign(Expression.ArrayAccess(exceptionVar, errorCounterVar), Expression.Constant(exceptionMessage1)),
                        incrementErrorCounter
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
            BlockExpression bla(FieldInfo field)
            {
                var propName = AutoPropertyOfFieldOrNull(field) is { } prop ? prop.Name : field.Name;
                var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";
                var onNullDetected = Expression.Block(
                    Expression.Assign(Expression.ArrayAccess(exceptionVar, errorCounterVar), Expression.Constant(exceptionMessage)),
                    incrementErrorCounter
                );
                return onNullDetected;
            }
        }
    }

    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
        => Cache<T>.checker ?? throw new("Failed to construct nullability verifier", Cache<T>.exception);
}
