using System;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)] // to test valuebase.ToString
    public sealed class ExampleValue : ValueBase<ExampleValue>
    {
        public int MyInt;
        public string MyString { get; set; }
        public DateTime SomeValueType { get; set; }
        public ExampleValue Nested;
        public double? NullableField;
        public ConsoleKey? AnEnum;
    }

    class ValueBaseTest
    {
        [Test]
        public void ToString_IsCompilableWherePossible()
        {
            PAssert.That(
                () =>
                    new ExampleValue {
                        NullableField = null,
                        AnEnum = ConsoleKey.BrowserBack,
                        MyString = "Hello World!",
                        Nested = new ExampleValue { AnEnum = ConsoleKey.BrowserRefresh },
                        SomeValueType = default(DateTime),
                        MyInt = 42,
                    }.ToString()
                        == @"new ExampleValue { MyInt = 42, Nested = new ExampleValue { MyInt = 0, Nested = null, NullableField = null, AnEnum = ConsoleKey.BrowserRefresh, MyString = null, SomeValueType = default(DateTime), }, NullableField = null, AnEnum = ConsoleKey.BrowserBack, MyString = ""Hello World!"", SomeValueType = default(DateTime), }");
        }
    }
}
