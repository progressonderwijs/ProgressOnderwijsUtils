#pragma warning disable CS8625

namespace ProgressOnderwijsUtils.Tests;

public sealed class NonNullableNullabilityCheck
{
    static readonly NullabilityInfoContext context = new();
    static readonly Func<NullablityTestClass, string[]?> Verifier = NonNullableFieldVerifier.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string?> Verifier0 = NonNullableFieldVerifier0.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string[]?> Verifier1 = NonNullableFieldVerifier1.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string[]?> Verifier2 = NonNullableFieldVerifier2.MissingRequiredProperties_FuncFactory<NullablityTestClass>();
    static readonly Func<NullablityTestClass, string[]?> Verifier3 = NonNullableFieldVerifier3.MissingRequiredProperties_FuncFactory<NullablityTestClass>();

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

    static string getVerifierMessage(string field, string className)
        => "Found null value in non nullable field in ProgressOnderwijsUtils.Tests." + className + "." + field;

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
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(
            () => CheckValidNonNullablitiy(oneContainingNull).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                }
            )
        );
    }

    [Fact]
    public void AssertWithReflectionOfAllFields()
        => PAssert.That(
            () => CheckValidNonNullablitiy(new NullablityTestClass()).SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)),
                }
            )
        );

    [Fact]
    public void AssertOneNullFieldCompiled()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null!, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(() => Verifier(oneContainingNull).AssertNotNull().SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier(allContainingNull).AssertNotNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)),
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
                SomeFilledObjectArray = new object[] { },
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
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(() => Verifier1(oneContainingNull).AssertNotNull().SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled1()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier1(allContainingNull).EmptyIfNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)),
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
                SomeFilledObjectArray = new object[] { },
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
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(() => Verifier2(oneContainingNull).EmptyIfNull().SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled2()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier2(allContainingNull).EmptyIfNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)),
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
                SomeFilledObjectArray = new object[] { },
            };
        PAssert.That(
            () => Verifier2(notContainingNull) ==
                null
        );
    }

    [Fact]
    public void AssertOneNullFieldCompile0()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(() => Verifier0(oneContainingNull) == getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) + "\n");
    }

    [Fact]
    public void AssertAllNullFieldsCompiled0()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier0(allContainingNull) ==
                getVerifierMessage(nameof(NullablityTestClass.SomeNullString)) + "\n"
                + getVerifierMessage(nameof(NullablityTestClass.SomeObject)) + "\n"
                + getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)) + "\n"
        );
    }

    [Fact]
    public void AssertNoNullFieldsCompiled0()
    {
        var notContainingNull
            = new NullablityTestClass {
                SomeNullString = "",
                SomeNullableField = null,
                SomeObject = new(),
                SomeNullableObject = null,
                SomeObjectArray = new object[] { },
                SomeFilledObjectArray = new object[] { },
            };
        PAssert.That(() => Verifier0(notContainingNull) == "");
    }

    [Fact]
    public void AssertOneNullFieldCompile3()
    {
        var oneContainingNull = new NullablityTestClass {
            SomeNullString = null, //non nullable
            SomeNullableField = null,
            SomeObject = new(),
            SomeNullableObject = null,
            SomeObjectArray = new object[] { },
            SomeFilledObjectArray = new object[] { },
        };
        PAssert.That(() => Verifier3(oneContainingNull).EmptyIfNull().SequenceEqual(new[] { getVerifierMessage(nameof(NullablityTestClass.SomeNullString)), }));
    }

    [Fact]
    public void AssertAllNullFieldsCompiled3()
    {
        var allContainingNull = new NullablityTestClass();
        PAssert.That(
            () => Verifier3(allContainingNull).EmptyIfNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestClass.SomeNullString)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObject)),
                    getVerifierMessage(nameof(NullablityTestClass.SomeObjectArray)),
                }
            )
        );
    }

    [Fact]
    public void AssertNoNullFieldsCompiled3()
    {
        var notContainingNull
            = new NullablityTestClass {
                SomeNullString = "",
                SomeNullableField = null,
                SomeObject = new(),
                SomeNullableObject = null,
                SomeObjectArray = new object[] { },
                SomeFilledObjectArray = new object[] { },
            };
        PAssert.That(() => Verifier3(notContainingNull) == null);
    }

    [Fact]
    public void AssertAllNullFieldsInheridedCompiled3()
    {
        var VerifierInheritence = NonNullableFieldVerifier3.MissingRequiredProperties_FuncFactory<NullabilityTestSubClass>();

        var containingAllNullSubClass
            = new NullabilityTestSubClass();
        PAssert.That(
            () => VerifierInheritence(containingAllNullSubClass).EmptyIfNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullabilityTestSubClass.SomeNullString),nameof(NullabilityTestSubClass)),
                    getVerifierMessage(nameof(NullabilityTestSubClass.SomeObject),nameof(NullabilityTestSubClass)),
                    getVerifierMessage(nameof(NullabilityTestSubClass.SomeObjectArray),nameof(NullabilityTestSubClass)),
                }
            )
        );
    }

    [Fact]
    public void AssertAllNullFieldsPropertiesCompiled3()
    {
        var VerifierInheritence = NonNullableFieldVerifier3.MissingRequiredProperties_FuncFactory<NullablityTestPropertyClass>();

        var containingAllNullSubClass
            = new NullablityTestPropertyClass(null!,null,null!,null!,null,new object[]{null});
        PAssert.That(
            () => VerifierInheritence(containingAllNullSubClass).EmptyIfNull().SequenceEqual(
                new[] {
                    getVerifierMessage(nameof(NullablityTestPropertyClass.SomeNullString),nameof(NullablityTestPropertyClass)),
                    getVerifierMessage(nameof(NullablityTestPropertyClass.SomeObject),nameof(NullablityTestPropertyClass)),
                    getVerifierMessage(nameof(NullablityTestPropertyClass.SomeObjectArray),nameof(NullablityTestPropertyClass)),
                }
            )
        );
    }
}
