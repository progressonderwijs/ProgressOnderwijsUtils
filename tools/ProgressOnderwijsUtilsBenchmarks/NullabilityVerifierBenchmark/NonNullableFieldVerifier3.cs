using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.BackingFieldDetector;

namespace ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;

public static class NonNullableFieldVerifier3
{
    //Without any calls but with counters etc based off hardcoded2
    public static Func<T, string[]?> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exception = Expression.Variable(typeof(string[]), "exceptionVar");
        var ErrorCounter = Expression.Variable(typeof(int), "counter");
        statements.Add(Expression.Assign(ErrorCounter, Expression.Constant(0)));

        NullabilityInfoContext context = new();

        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => context.Create(f).WriteState == NullabilityState.NotNull)
            .ToArray();

        var variables = new List<ParameterExpression>();
        var i = 0;
        foreach (var f in fields) {
            var memberExpression = Expression.Field(objectParam, f);
            var fieldValue = Expression.Convert(memberExpression, typeof(object));
            var variable = Expression.Variable(typeof(string), "v" + i);
            var propName = AutoPropertyOfFieldOrNull(f) is { } prop ? prop.Name : f.Name;
            var exceptionMessage = typeof(T).ToCSharpFriendlyTypeName() + "." + propName + " contains NULL despite being non-nullable";

            statements.Add(variable);
            statements.Add(
                Expression.Assign(
                    variable,
                    Expression.Condition(
                        Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                        Expression.Block(
                            Expression.AddAssign(ErrorCounter, Expression.Constant(1, typeof(int))),
                            Expression.Constant(exceptionMessage)
                        ),
                        Expression.Constant(null, typeof(string))
                    )
                )
            );
            variables.Add(variable);
            i++;
        }
        var setArray = variables.Select(
            var => Expression.IfThen(
                Expression.NotEqual(var, Expression.Constant(null, typeof(string))),
                Expression.Block(
                    Expression.Assign(Expression.ArrayAccess(exception, ErrorCounter), var),
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
