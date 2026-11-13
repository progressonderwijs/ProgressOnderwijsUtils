namespace ProgressOnderwijsUtils.Tests;

public sealed class ReflectionLoadableClassHelperTest
{
    [Fact]
    public void A_public_closed_base_is_enumerable()
    {
        var constructors = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(ClosedBaseForReflectionLoading)).AssertOk();
        PAssert.That(() => constructors.Select(c => c.DeclaringType).SequenceEqual(new[] { typeof(SubclassOfClosedBase), }));
    }

    [Fact]
    public void A_public_base_with_a_protected_constructor_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(OpenBaseForReflectionLoading)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{nameof(OpenBaseForReflectionLoading)} must be closed", StringComparison.Ordinal));
    }

    [Fact]
    public void An_internal_base_with_a_protected_constructor_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(InternalBaseWithProtectedConstructor)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{nameof(InternalBaseWithProtectedConstructor)} must be closed", StringComparison.Ordinal));
    }

    [Fact]
    public void A_public_base_with_only_private_protected_constructors_is_enumerable()
    {
        var constructors = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(PrivateProtectedBaseForReflectionLoading)).AssertOk();
        PAssert.That(() => constructors.Select(c => c.DeclaringType).SequenceEqual(new[] { typeof(SubclassOfPrivateProtectedBase), }));
    }

    [Fact]
    public void A_public_non_closed_abstract_intermediate_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(ClosedBaseWithOpenIntermediate)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{nameof(OpenIntermediate)} is an abstract subclass of {nameof(ClosedBaseWithOpenIntermediate)}", StringComparison.Ordinal));
    }

    [Fact]
    public void A_public_non_closed_generic_intermediate_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(PrivateProtectedBaseWithGenericIntermediate)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{typeof(OpenGenericIntermediate<>).ToCSharpFriendlyTypeName()} is an abstract subclass", StringComparison.Ordinal));
    }

    [Fact]
    public void A_closed_generic_intermediate_is_enumerable()
    {
        var constructors = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(PrivateProtectedBaseWithClosedGenericIntermediate)).AssertOk();
        PAssert.That(() => constructors.Select(c => c.DeclaringType).SequenceEqual(new[] { typeof(SubclassOfClosedGenericIntermediate), }));
    }

    [Fact]
    public void A_constructed_generic_base_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(ClosedGenericBase<int>)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{typeof(ClosedGenericBase<int>).ToCSharpFriendlyTypeName()} must be non-generic", StringComparison.Ordinal));
    }

    [Fact]
    public void An_interface_base_is_rejected()
    {
        var problems = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(IInterfaceBaseForReflectionLoading)).AssertError();
        PAssert.That(() => problems.Single().StartsWith($"{nameof(IInterfaceBaseForReflectionLoading)} must be a class", StringComparison.Ordinal));
    }

    [Fact]
    public void A_closed_intermediate_is_enumerable()
    {
        var constructors = ReflectionLoadableClassHelper.AllSubclassConstructorsForType(typeof(ClosedBaseWithClosedIntermediate)).AssertOk();
        PAssert.That(() => constructors.Select(c => c.DeclaringType).SequenceEqual(new[] { typeof(SubclassOfClosedIntermediate), }));
    }
}

public closed class ClosedBaseForReflectionLoading;

public sealed class SubclassOfClosedBase : ClosedBaseForReflectionLoading;

public abstract class OpenBaseForReflectionLoading;

abstract class InternalBaseWithProtectedConstructor;

public abstract class PrivateProtectedBaseForReflectionLoading
{
    private protected PrivateProtectedBaseForReflectionLoading() { }
}

public sealed class SubclassOfPrivateProtectedBase : PrivateProtectedBaseForReflectionLoading;

public closed class ClosedBaseWithOpenIntermediate;

public abstract class OpenIntermediate : ClosedBaseWithOpenIntermediate;

// A generic intermediate cannot derive from a closed base without using its type parameter in that base (CS9383).
public abstract class PrivateProtectedBaseWithGenericIntermediate
{
    private protected PrivateProtectedBaseWithGenericIntermediate() { }
}

public abstract class OpenGenericIntermediate<T> : PrivateProtectedBaseWithGenericIntermediate
{
    protected abstract void Method(T arg);
}

public abstract class PrivateProtectedBaseWithClosedGenericIntermediate
{
    private protected PrivateProtectedBaseWithClosedGenericIntermediate() { }
}

public closed class ClosedGenericIntermediate<T> : PrivateProtectedBaseWithClosedGenericIntermediate
{
    protected abstract void Method(T arg);
}

public sealed class SubclassOfClosedGenericIntermediate : ClosedGenericIntermediate<int>
{
    protected override void Method(int arg) { }
}

public closed class ClosedBaseWithClosedIntermediate;

public closed class ClosedIntermediate : ClosedBaseWithClosedIntermediate;

public sealed class SubclassOfClosedIntermediate : ClosedIntermediate;

public closed class ClosedGenericBase<T>;

public interface IInterfaceBaseForReflectionLoading;
