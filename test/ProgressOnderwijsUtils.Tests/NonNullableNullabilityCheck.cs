using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

public sealed class NonNullableNullabilityCheck
{
    static readonly NullabilityInfoContext context = new();
    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

    static string[] CheckValidNonNullablitiy(object obj)
        => obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => NullFoundInNotNullField(obj, f)
                    ? getVerifierMessage(f.Name)
                    : null
            ).WhereNotNull().ToArray();

    static bool NullFoundInNotNullField(object obj, FieldInfo? field)
        => field?.GetValue(obj) == null && context.Create(field.AssertNotNull()).WriteState == NullabilityState.NotNull;

    static string getVerifierMessage(string field)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests.NullablityTestClass." + field;

    [Fact]
    public void AssertWithReflectionOfField()
        => PAssert.That(() => NullFoundInNotNullField(new NullablityTestClass(), typeof(NullablityTestClass).GetField("SomeNullString")) == true);

    [Fact]
    public void AssertWithReflectionOfOneField()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { }
        };
        PAssert.That(
            () => CheckValidNonNullablitiy(oneContainingNull).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString))
                }
            )
        );
    }

    [Fact]
    public void AssertWithReflectionOfAllFields()
        => PAssert.That(() => CheckValidNonNullablitiy(new NullablityTestClass()).SequenceEqual(new[] {
            getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
            getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
            getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray))
        }));

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

    [Fact]
    public void AssertOneNullFieldCompiled1()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { }
        };
        PAssert.That(() => Verifier1(oneContainingNull).SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled1()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier1(allContainingNull).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray))
                }
            )
        );
    }

    [Fact]
    public void AssertNoNullFieldsCompiled1()
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
            () => Verifier1(notContainingNull) ==
                null
        );
    }

    [Fact]
    public void AssertOneNullFieldCompile2()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { }
        };
        PAssert.That(() => Verifier2(oneContainingNull).SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled2()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier2(allContainingNull).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray))
                }
            )
        );
    }

    [Fact]
    public void AssertNoNullFieldsCompiled2()
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
            () => Verifier2(notContainingNull) ==
                null
        );
    }
}
