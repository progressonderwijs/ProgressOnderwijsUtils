using System;
using System.Linq;
using BenchmarkDotNet.Running;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public static class BenchmarkProgram
    {
        static void Main()
        {
            BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
            RunArrayBuilderBenchmarks();
        }

        [UsedImplicitly]
        static void RunArrayBuilderBenchmarks()
        {
            new BenchmarkSwitcher(new[] {
                typeof(IntArrayBuilderBenchmark),
                typeof(BigStructArrayBuilderBenchmark),
                typeof(ByteArrayBuilderBenchmark),
                typeof(ReferenceTypeArrayBuilderBenchmark),
                typeof(SmallStructArrayBuilderBenchmark),
            }).RunAllJoined();
        }
    }
}
