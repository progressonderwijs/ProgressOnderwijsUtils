using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Data;

public sealed class NonNullableFieldVerifier
{
    static readonly Func<object, string> Verifier = InitNonNullableVerifier();

    public static string Verify(object obj)
        => Verifier(obj);

    public static Func<object, string> InitNonNullableVerifier()
    {
        var statements = new List<Expression>();
        var switchstatements = new List<SwitchCase>();
        var switchValue = Expression.Variable(typeof(Type), "switchValue");
        var objectParam = Expression.Parameter(typeof(object), "obj");
        var exceptionVar = Expression.Variable(typeof(string), "exceptionVar");
        var obj = Expression.Convert(objectParam, typeof(object));
        statements.Add(Expression.Assign(exceptionVar, Expression.Constant("")));
        var type = Expression.Call(obj, typeof(object).GetMethod(nameof(GetType)));
        statements.Add(Expression.Assign(switchValue, type));

        var types = new[] { typeof(NullablityTestClass), typeof(OtherNullablityTestClass) };
        switchstatements.AddRange(
            types.Select(
                t => {
                    var invokeCall = Expression.Invoke(Expression.Constant(GetNonNullableVerifierForType(t)), Expression.Convert(obj, t));
                    return Expression.SwitchCase(Expression.Assign(exceptionVar, invokeCall), Expression.Constant(t));
                }
            )
        );
        var switchS = Expression.Switch(switchValue, Expression.Assign(exceptionVar, Expression.Constant("")), switchstatements.ToArray());
        statements.Add(switchS);
        statements.Add(exceptionVar);
        var ToLambda = Expression.Lambda<Func<object, string>>(Expression.Block(new[] { exceptionVar, switchValue }, statements), objectParam);
        return ToLambda.Compile();
    }

    public static Delegate GetNonNullableVerifierForType(Type type)
    {
        var ObjectParam = Expression.Parameter(typeof(object), "Obj");
        NullabilityInfoContext context = new();
        var statements = new List<Expression>();
        var varExcep = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(varExcep, Expression.Constant("")));
        var obj = Expression.Convert(ObjectParam, type);

        var fields = type.GetFields();

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
                                Expression.Constant("Found null value in non nullable field in " + type.Name + "." + f.Name + Environment.NewLine)
                            )
                        )
                    );
                }
            )
        );

        statements.Add(varExcep);
        var expBlock = Expression.Block(new[] { varExcep }, statements);
        var ToLambda = Expression.Lambda<Func<object, string>>(expBlock, ObjectParam);
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

public sealed class OtherNullablityTestClass
{
    public string SomeNullString1 = null;
    public string? SomeNullableField1 = null;
    public object SomeObject1 = null;
    public object? SomeNullableObject1 = null;
    public object[] SomeObjectArray1 = null;
    public object[] SomeFilledObjectArray1 = { null };
}
