using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class NonNullableNullabilityCheck
{
    static string WarnAbout(string field)
        => $"{field} is a non nullable field with a null value.\n";

    static readonly NullabilityInfoContext context = new();
    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    static string CheckValidNonNullablitiy(object obj)
        => obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => NullFoundInNotNullField(obj, f)
                    ? WarnAbout(f.Name)
                    : null
            ).WhereNotNull().JoinStrings();

    static bool NullFoundInNotNullField(object obj, FieldInfo? field)
        => field?.GetValue(obj) == null && context.Create(field.AssertNotNull()).WriteState == NullabilityState.NotNull;

    static string getVerifierMessage(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + field + Environment.NewLine;

    [Fact]
    public void AssertWithReflectionOfField()
        => PAssert.That(() => NullFoundInNotNullField(new NullablityTestClass(), typeof(NullablityTestClass).GetField("SomeNullString")) == true);

    [Fact]
    public void AssertWithReflectionOfAllFields()
        => PAssert.That(() => CheckValidNonNullablitiy(new NullablityTestClass()) != "");

    [Fact]
    public void AssertOneNullFieldCompiled()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { }
        };
        PAssert.That(() => Verifier(oneContainingNull).SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier(allContainingNull).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray))
                }
            )
        );
    }

    [Fact]
    public void AssertNoNullFieldsCompiled()
    {
        var notContainingNull
            = new NullablityTestClass {
                SomeNullString = "",
                SomeNullableField = null,
                SomeObject = new(),
                SomeNullableObject = null,
                SomeObjectArray = new object[] { },
                SomeFilledObjectArray = new object[] { }
            };
        PAssert.That(
            () => Verifier(notContainingNull) ==
                null
        );
    }
}
