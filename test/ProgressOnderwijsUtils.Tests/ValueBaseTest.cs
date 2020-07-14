using System;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
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

    public sealed class ValueBaseTest
    {
        [Fact]
        public void ToString_ReturnsCleanLookingOutput()
        {
            ApprovalTest.Verify(
                new ExampleValue {
                    NullableField = null,
                    AnEnum = ConsoleKey.BrowserBack,
                    MyString = "Hello World!",
                    Nested = new ExampleValue { AnEnum = ConsoleKey.BrowserRefresh },
                    SomeValueType = default(DateTime),
                    MyInt = 42,
                }.ToString());
        }
    }
}
