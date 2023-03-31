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

            var incrementErrorCounter = Expression.AddAssign(errorCounterVar, Expression.Constant(1, typeof(int)));

            var storeNullabilityErrorCountInVariable = Expression.Block(ForEachInvalidNull(_ => incrementErrorCounter));

            var whenNullabilityErrorDetected = Expression.Block(
                    new[] { exceptionVar, },
                    Expression.Assign(exceptionVar, Expression.NewArrayBounds(typeof(string), errorCounterVar)),
                    Expression.Assign(errorCounterVar, Expression.Constant(0, typeof(int))),
                    Expression.Block(
                        ForEachInvalidNull(
                            field => Expression.Block(
                                Expression.Assign(Expression.ArrayAccess(exceptionVar, errorCounterVar), Expression.Constant(ErrorMessageForField(field))),
                                incrementErrorCounter
                            )
                        )
                    ),
                    exceptionVar
                );

            var computeErrorMessageGivenCount = Expression.Condition(
                Expression.Equal(errorCounterVar, Expression.Constant(0, typeof(int))),
                Expression.Constant(null, typeof(string[])),
                whenNullabilityErrorDetected
            );

            var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] { errorCounterVar, }, storeNullabilityErrorCountInVariable, computeErrorMessageGivenCount), objectParam);
            return ToLambda.Compile();
        }

        static string ErrorMessageForField(FieldInfo field)
            => $"{typeof(T).ToCSharpFriendlyTypeName()}.{HumanReadableMemberName(field)} contains NULL despite being non-nullable";

        static string HumanReadableMemberName(FieldInfo field)
            => AutoPropertyOfFieldOrNull(field) is { } autoProp ? autoProp.Name : field.Name;
    }

    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
        => Cache<T>.checker ?? throw new("Failed to construct nullability verifier", Cache<T>.exception);
}
