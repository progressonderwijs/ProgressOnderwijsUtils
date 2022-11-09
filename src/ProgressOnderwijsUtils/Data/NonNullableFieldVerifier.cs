using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Data;

public static class NonNullableFieldVerifier
{
    public static Func<T, string> MissingRequiredProperties_FuncFactory<T>()
    {
        var statements = new List<Expression>();
        var objectParam = Expression.Parameter(typeof(T), "obj");
        var exceptionVar = Expression.Variable(typeof(string), "exceptionVar");
        var obj = Expression.Convert(objectParam, typeof(object));
        statements.Add(Expression.Assign(exceptionVar, Expression.Constant("")));
        var invokeCall = Expression.Invoke(Expression.Constant(GetNonNullableVerifierForType<T>()), Expression.Convert(obj, typeof(T)));
        statements.Add(Expression.Assign(exceptionVar, invokeCall));
        statements.Add(exceptionVar);
        var ToLambda = Expression.Lambda<Func<T, string>>(Expression.Block(new[] { exceptionVar }, statements), objectParam);
        return ToLambda.Compile();
    }

    public static Delegate GetNonNullableVerifierForType<T>()
    {
        var ObjectParam = Expression.Parameter(typeof(T), "Obj");
        NullabilityInfoContext context = new();
        var statements = new List<Expression>();
        var varExcep = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(varExcep, Expression.Constant("")));
        var obj = Expression.Convert(ObjectParam, typeof(T));
        var fields = typeof(T).GetFields();
        statements.AddRange(
            fields.Where(f => context.Create(f).WriteState == NullabilityState.NotNull).Select(
                f => {
                    var memberExpression = Expression.Field(obj, f.Name);
                    var fieldValue = Expression.Convert(memberExpression, typeof(object));
                    return Expression.IfThen(
                        Expression.Equal(fieldValue, Expression.Constant(null, typeof(object))),
                        Expression.Assign(
                            varExcep,
                            Expression.Call(
                                typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }),
                                varExcep,
                                Expression.Constant("Found null value in non nullable field in " + typeof(T) + "." + f.Name + Environment.NewLine)
                            )
                        )
                    );
                }
            )
        );

        statements.Add(varExcep);
        var expBlock = Expression.Block(new[] { varExcep }, statements);
        var ToLambda = Expression.Lambda<Func<T, string>>(expBlock, ObjectParam);
        return ToLambda.Compile();
    }
}

public sealed class NullablityTestClass
{
    public string SomeNullString = null;
    public string? SomeNullableField = null;
    public object SomeObject = null;
    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null;
    public object[] SomeFilledObjectArray = { null };
}
