using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;
using Xunit.Sdk;
using static ProgressOnderwijsUtils.Html.HtmlTagKinds;

namespace ProgressOnderwijsUtilsBenchmarks;
#nullable enable
[Config(typeof(Config))]
[MemoryDiagnoser]
public sealed class NullabilityBenchmark
{
    sealed class Config : ManualConfig
    {
        public Config()
            => _ = AddJob(
                Job.MediumRun
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithOutlierMode(OutlierMode.DontRemove)
                    .WithMaxIterationCount(5000)
                    .WithMaxRelativeError(0.002)
                    .WithToolchain(InProcessNoEmitToolchain.Instance)
                    .WithId("InProcess")
            );
    }

    readonly SomeClass someClass = new SomeClass();

    static string WarnAbout(string field)
        => $"{field} is a non nullable field with a null value.\n";

    readonly NullabilityInfoContext context = new();

    [Benchmark]
    public void WithReflection()
    {
        string CheckValidNonNullablitiy(Type type)
            => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(
                    f => f.GetValue(someClass) == null && context.Create(f).WriteState == NullabilityState.NotNull
                        ? WarnAbout(f.Name)
                        : null
                ).WhereNotNull().JoinStrings();

        _ = CheckValidNonNullablitiy(typeof(SomeClass));
    }

    [Benchmark]
    public void HardCoded()
    {
        _ = ""
            + (someClass.SomeNullString == null ? WarnAbout(nameof(SomeClass.SomeNullString)) : "")
            + (someClass.SomeObject == null ? WarnAbout(nameof(SomeClass.SomeObject)) : "")
            + (someClass.SomeObjectArray == null ? WarnAbout(nameof(SomeClass.SomeObjectArray)) : "")
            + (someClass.SomeFilledObjectArray == null ? WarnAbout(nameof(SomeClass.SomeFilledObjectArray)) : "")
            ;
    }

    static readonly Func<object, string> del = loopAllTypesForDelegate();

    [Benchmark]
    public void Compiled()
    {
        var bla = del(someClass);
        Console.WriteLine(bla);
    }

    public static Func<object, string> loopAllTypesForDelegate()
    {
        var statements = new List<Expression>();
        var switchstatements = new List<SwitchCase>();
        var switchValue = Expression.Variable(typeof(Type), "switchValue");
        var objectParam = Expression.Parameter(typeof(object), "Object");
        var exceptionVar = Expression.Variable(typeof(string), "exceptionVar");
        var obj = Expression.Convert(objectParam, typeof(object));
        statements.Add(Expression.Assign(exceptionVar, Expression.Constant("")));
        statements.Add(Expression.Assign(switchValue, Expression.Constant(obj.GetType())));

        var types = new[] { typeof(SomeClass) };
        switchstatements.AddRange(
            types.Select(
                t =>
                    Expression.SwitchCase(Expression.Assign(exceptionVar, Expression.Invoke(Expression.Constant(GetExprBlock(t)), obj)), Expression.Constant(t))
            )
        );
        var switchS = Expression.Switch(switchValue, Expression.Constant(""), switchstatements.ToArray());
        statements.Add(switchS);
        statements.Add(exceptionVar);
        var ToLambda = Expression.Lambda<Func<object, string>>(Expression.Block(new[] { exceptionVar, switchValue }, statements), objectParam);
        return ToLambda.Compile();
    }

    public static Delegate GetExprBlock(Type type)
    {
        var ObjectParam = Expression.Parameter(typeof(object), "Object");
        NullabilityInfoContext context = new();
        var statements = new List<Expression>();
        var varExcep = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(varExcep, Expression.Constant("")));
        var obj = Expression.Convert(ObjectParam, type);

        var fields = type.GetFields();

        foreach (var field in fields) {
            if (context.Create(field).WriteState == NullabilityState.NotNull) {
                var memberExpression = Expression.Field(obj, field.Name);
                var fieldValue = Expression.Convert(memberExpression, typeof(object));
                statements.Add(
                    Expression.IfThen(
                        Expression.Equal(Expression.Constant(fieldValue), Expression.Constant(null, typeof(object))),
                        Expression.Assign(
                            varExcep,
                            Expression.Call(
                                typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }),
                                varExcep,
                                Expression.Call(
                                    typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string), typeof(string), typeof(string) }),
                                    Expression.Constant("Found non nullable field in "),
                                    Expression.Constant(nameof(obj)),
                                    Expression.Constant("."),
                                    Expression.Constant(field.Name)
                                )
                            )
                        )
                    )
                );
            }
        }
        statements.Add(varExcep);
        var expBlock = Expression.Block(new[] { varExcep }, statements);
        var ToLambda = Expression.Lambda(expBlock, ObjectParam);
        return ToLambda.Compile();
    }

    class SomeClass
    {
        public string SomeNullString = null;
        public string? SomeNullableField = null;
        public object SomeObject = null;
        public object? SomeNullableObject = null;
        public object[] SomeObjectArray = null;
        public object[] SomeFilledObjectArray = { null };
    }
}
