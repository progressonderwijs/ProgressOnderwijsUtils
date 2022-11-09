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
    static readonly Func<NullablityTestClass, string> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    static string CheckValidNonNullablitiy(object obj)
        => obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => AssertNullWhileNotNullable(obj, f)
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().JoinStrings();

    static bool AssertNullWhileNotNullable(object obj, FieldInfo? field)
        => field?.GetValue(obj) == null && context.Create(field.AssertNotNull()).WriteState == NullabilityState.NotNull;

    static string getVerifierMessage(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Data.NullablityTestClass." + field + Environment.NewLine;

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
            () => Verifier(someClassChanged) ==
                getVerifierMessage(nameof(NullablityTestClass.SomeNullString))
        );
    }

    [Fact]
    public void AssertAllNullFieldsCompiled()
    {
        var someClassChanged = new NullablityTestClass();
        PAssert.That(
            () => Verifier(someClassChanged) ==
                getVerifierMessage(nameof(NullablityTestClass.SomeNullString))
                + getVerifierMessage(nameof(NullablityTestClass.SomeObject))
                + getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray))
        );
    }
}
