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
        for (var i = 0; i < 300; i++) {
            _ = CheckValidNonNullablitiy(typeof(SomeClass));
        }
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

    public static Delegate getDelegateOneClass()
    {
        var Object = Expression.Parameter(typeof(SomeClass), "Object");

        var expBlock = GetExpr(typeof(SomeClass), Object);
        var ToLambda = Expression.Lambda(expBlock, new[] {Object});
        return ToLambda.Compile();
    }

    public static BlockExpression GetExpr(Type ObjectType, ParameterExpression ObjectParam)
    {
        NullabilityInfoContext context = new();
        var statements = new List<Expression>();
        var varExcep = Expression.Variable(typeof(string), "exceptionVar");
        statements.Add(Expression.Assign(varExcep, Expression.Constant("")));

        var fields = ObjectType.GetFields();

        foreach (var field in fields) {
            if (context.Create(field).WriteState == NullabilityState.NotNull) {
                var memberExpression = Expression.Field(ObjectParam, field.Name);
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
                                    Expression.Constant(ObjectType.Name),
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
        return Expression.Block(new[] { varExcep }, statements);
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
