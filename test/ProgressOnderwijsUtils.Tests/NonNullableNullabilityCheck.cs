using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils.Tests;

#nullable enable

public sealed class NonNullableNullabilityCheck
{
    static string WarnAbout(string field)
        => $"{field} is a non nullable field with a null value.\n";

    static readonly NullabilityInfoContext context = new();

    static string CheckValidNonNullablitiy(object obj)
        => obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => AssertNullWhileNotNullable(obj, f)
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().JoinStrings();

    static bool AssertNullWhileNotNullable(object obj, FieldInfo? field)
        => field?.GetValue(obj) == null && context.Create(field.AssertNotNull()).WriteState == NullabilityState.NotNull;

    [Fact]
    public void AssertWithReflectionOfField()
        => PAssert.That(() => AssertNullWhileNotNullable(new NullablityTestClass(), typeof(NullablityTestClass).GetField("SomeNullString")) == true);

    [Fact]
    public void AssertWithReflectionOfAllFields()
        => PAssert.That(() => CheckValidNonNullablitiy(new NullablityTestClass()) != "");

    [Fact]
    public void AssertOneNullFieldCompiled()
    {
        var someClassChanged = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { }
        };
        PAssert.That(
            () => NonNullableFieldVerifier.Verify(someClassChanged) ==
                "Found null value in non nullable field in NullablityTestClass.SomeNullString" + Environment.NewLine
        );
    }

    [Fact]
    public void AssertAllNullFieldsCompiled()
    {
        var someClassChanged = new NullablityTestClass();
        PAssert.That(
            () => NonNullableFieldVerifier.Verify(someClassChanged) ==
                "Found null value in non nullable field in NullablityTestClass.SomeNullString" + Environment.NewLine
                + "Found null value in non nullable field in NullablityTestClass.SomeObject" + Environment.NewLine
                + "Found null value in non nullable field in NullablityTestClass.SomeObjectArray" + Environment.NewLine
        );
    }
}
