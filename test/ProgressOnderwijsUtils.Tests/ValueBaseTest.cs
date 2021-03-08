using System;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)] // to test valuebase.ToString
    public sealed record ExampleValue
    {
        public int MyInt;
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string MyString { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public DateTime SomeValueType { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public ExampleValue Nested;
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public double? NullableField;
        public ConsoleKey? AnEnum;
    }

    public sealed class ValueBaseTest
    {
        [Fact]
        public void ToString_ReturnsCleanLookingOutput()
            => ApprovalTest.CreateHere().AssertUnchangedAndSave(
                new ExampleValue {
                    NullableField = null,
                    AnEnum = ConsoleKey.BrowserBack,
                    MyString = "Hello World!",
                    Nested = new() { AnEnum = ConsoleKey.BrowserRefresh },
                    SomeValueType = new(),
                    MyInt = 42,
                }.ToString()
            );
    }
}
