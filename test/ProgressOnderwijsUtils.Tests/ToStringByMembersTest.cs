using System;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)] // to test valuebase.ToString
    public sealed class ExampleValue
    {
        public int MyInt;
        public string? MyString { get; set; }
        public DateTime SomeValueType { get; set; }
        public ExampleValue? Nested;
        public double? NullableField;
        public ConsoleKey? AnEnum;

        public override string ToString()
            => ToStringByMembers.ToStringByPublicMembers(this);
    }

    public sealed class ToStringByMembersTest
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
