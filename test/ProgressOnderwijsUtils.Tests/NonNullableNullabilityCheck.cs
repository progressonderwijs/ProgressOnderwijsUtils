using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests;

#nullable enable
public sealed class NullablityTestClass 
{
    public string SomeNullString = null;
    public string? SomeNullableField = null;
    public object SomeObject = null;

    public object? SomeNullableObject = null;
    public object[] SomeObjectArray = null;
    public object[] SomeFilledObjectArray = {null};
}
public sealed class NonNullableNullabilityCheck
{
    static string WarnAbout(string field) => $"{field} is a non nullable field with a null value.\n";
    static readonly NullabilityInfoContext context = new();

    static string CheckValidNonNullablitiy(object obj)
        => obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(
                f => AssertNullWhileNotNullable(obj,f)
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
}
