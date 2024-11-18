using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;

public static class NonNullableFieldVerifier2
{
    //Without any calls but with counters etc
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exceptionVar");
        var NullFoundCounter = Expression.Variable(typeof(int), "counter");
        statements.Add(Expression.Assign(NullFoundCounter, Expression.Constant(0)));

        NullabilityInfoContext context = new();

        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();
        var count = fields.Length;
        var messages = Expression.Variable(typeof(string[]), "AllExceptions");
        statements.Add(Expression.Assign(messages, Expression.NewArrayBounds(typeof(string), Expression.Constant(count))));

        foreach (var f in fields) {
            var memberExpression = Expression.Field(objectParam, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));

            var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
            var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";

            statements.Add(
                Expression.IfThen(
                    Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                    Expression.Block(
                        Expression.Assign(Expression.ArrayAccess(messages, NullFoundCounter), Expression.Constant(exceptionMessage)),
                        Expression.AddAssign(NullFoundCounter, Expression.Constant(1))
                    )
                )
            );
        }
        var IteratorCounter = Expression.Variable(typeof(int), "loopCounter");
        var label = Expression.Label();
        statements.Add(
            Expression.IfThenElse(
                Expression.GreaterThan(NullFoundCounter, Expression.Constant(0)),
                Expression.Block(
                    Expression.Assign(
                        exception,
                        Expression.NewArrayBounds(typeof(string), NullFoundCounter)
                    ),
                    Expression.Assign(IteratorCounter, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThenElse(
                                Expression.LessThan(IteratorCounter, NullFoundCounter),
                                Expression.Assign(Expression.ArrayAccess(exception, IteratorCounter), Expression.ArrayAccess(messages, IteratorCounter)),
                                Expression.Break(label)
                            ),
                            Expression.AddAssign(IteratorCounter, Expression.Constant(1))
                        ),
                        label
                    )
                ),
                Expression.Assign(exception, Expression.Constant(null, typeof(string[])))
            )
        );
        statements.Add(exception);

        var ToLambda = Expression.Lambda<Func<T, string[]?>>(Expression.Block(new[] { IteratorCounter, NullFoundCounter, messages, exception, }, statements), objectParam);
        return ToLambda.Compile();
    }
}
