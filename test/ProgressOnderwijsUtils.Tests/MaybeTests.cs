using System;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MaybeTests
    {
        [Fact]
        public void OkContainsTheValuePassedIn()
        {
            PAssert.That(() => Maybe.Ok("42").AsMaybeWithoutError<Unit>().Contains("42"));
            PAssert.That(() => Maybe.Ok("42").AsMaybeWithoutError<Unit>().Contains("hello world!") == false);
            // ReSharper disable once RedundantCast
            PAssert.That(() => Maybe.Ok((string?)"42").AsMaybeWithoutError<Unit>().Contains(default(string)) == false);
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable 8620 //R# bugje?
            PAssert.That(() => Maybe.Ok(default(string)).AsMaybeWithoutError<Unit>().Contains(default(string)));
#pragma warning restore 8620
#pragma warning restore IDE0079 // Remove unnecessary suppression
        }

        [Fact]
        public void IsOk_is_true_iif_maybe_is_ok()
        {
            PAssert.That(() => Maybe.Ok("42").AsMaybeWithoutError<Unit>().IsOk);
            PAssert.That(() => Maybe.Error("42").AsMaybeWithoutValue<int>().IsOk == false);
        }

        [Fact]
        public void Maybe_default_is_not_ok()
            => PAssert.That(() => default(Maybe<int, string>).IsOk == false);

        [Fact]
        public void Extract_calls_first_function_when_ok()
        {
            var ok = Maybe.Ok(3).AsMaybeWithoutError<Unit>();
            PAssert.That(() => ok.Extract(x => 42, x => 1) == 42);
        }

        [Fact]
        public void Extract_calls_second_function_when_not_ok()
        {
            var notOk = Maybe.Error().AsMaybeWithoutValue<Unit>();
            PAssert.That(() => notOk.Extract(x => 42, x => 1) == 1);
        }

        [Fact]
        public void A_maybe_can_either_contain_an_error_or_an_ok()
        {
            PAssert.That(() => Maybe.Either(true, "good", -1).Contains(val => val == "good"));
            PAssert.That(() => Maybe.Either(false, "good", -1).ContainsError(err => err == -1));
            PAssert.That(() => Maybe.Either(true, () => "good", () => -1).Contains("good"));
            PAssert.That(() => Maybe.Either(false, () => "good", () => -1).ContainsError(-1));
        }

        [Fact]
        public void Maybe_check_returns_Unit_or_error()
        {
            PAssert.That(() => Maybe.Verify(true, "an error").IsOk);
            PAssert.That(() => Maybe.Verify(false, "an error").ContainsError(err=> err== "an error"));
            PAssert.That(() => Maybe.Verify(true, () => "an error").IsOk);
            PAssert.That(() => Maybe.Verify(false, () => "an error").ContainsError(err=> err== "an error"));
        }

        [Fact]
        public void Maybe_try_is_ok_unless_exception_is_thrown()
        {
            PAssert.That(() => Maybe.Try(() => int.Parse("42")).Catch<Exception>().Contains(42));
            PAssert.That(() => Maybe.Try(() => int.Parse("42e")).Catch<Exception>().ContainsError(e => e is FormatException));
            Action<int> ignoreValue = _ => { };
            PAssert.That(() => Maybe.Try(() => ignoreValue(int.Parse("42e"))).Catch<Exception>().ContainsError(e => e is FormatException));
        }

        [Fact]
        public void Maybe_try_finally_cleansupOnce()
        {
            var cleanupCalled = 0;
            var maybeWithCleanup = Maybe.Try(() => int.Parse("42e")).Finally(() => cleanupCalled++);
            PAssert.That(() => cleanupCalled == 1 && maybeWithCleanup.ContainsError(e => e is FormatException));

            cleanupCalled = 0;
            maybeWithCleanup = Maybe.Try(() => int.Parse("42")).Finally(() => cleanupCalled++);
            PAssert.That(() => cleanupCalled == 1 && maybeWithCleanup.Contains(42));

            cleanupCalled = 0;
            maybeWithCleanup = Maybe.Try(() => int.Parse("42")).Finally(() => {
                cleanupCalled++;
                throw new InvalidOperationException();
            });
            PAssert.That(() => cleanupCalled == 1 && maybeWithCleanup.ContainsError(e => e is InvalidOperationException));

            cleanupCalled = 0;
            maybeWithCleanup = Maybe.Try(() => int.Parse("42e")).Finally(() => {
                cleanupCalled++;
                throw new();
            });
            PAssert.That(() => cleanupCalled == 1 && maybeWithCleanup.ContainsError(e => e is AggregateException));

            cleanupCalled = 0;
            var unitMaybeWithCleanup = Maybe.Try(() => throw new("bla")).Finally(() => {
                cleanupCalled++;
                throw new();
            });
            PAssert.That(() => cleanupCalled == 1 && unitMaybeWithCleanup.ContainsError(e => e is AggregateException));
        }

        [Fact]
        public void Maybe_try_does_not_catch_unrelated_exceptions()
            => Assert.ThrowsAny<Exception>(() => Maybe.Try(() => "123".Substring(4, 10)).Catch<NotSupportedException>());

        [Fact]
        public void ErrorWhenNotNull_is_error_for_nonnull()
        {
            PAssert.That(() => Maybe.ErrorWhenNotNull("asd").IsError);
            PAssert.That(() => Maybe.ErrorWhenNotNull(default(object)).IsOk);
            PAssert.That(() => Maybe.ErrorWhenNotNull(default(int?)).IsOk);
        }

        [Fact]
        public void OkWhenNotNull_is_ok_for_nonnull()
        {
            PAssert.That(() => Maybe.OkWhenNotNull("asd").IsOk);
            PAssert.That(() => Maybe.OkWhenNotNull(default(object)).IsError);
            PAssert.That(() => Maybe.OkWhenNotNull(default(int?)).IsError);
        }

        [Fact]
        public void ErrorOrNull_is_null_iif_error()
        {
            PAssert.That(() => Maybe.ErrorWhenNotNull("asd").ErrorOrNull() == "asd");
            PAssert.That(() => Maybe.Ok(3).AsMaybeWithoutError<string>().ErrorOrNull() == null);
            PAssert.That(() => Maybe.Error(1337).AsMaybeWithoutValue<string>().ErrorOrNullable() == 1337);
            PAssert.That(() => Maybe.Ok("42").AsMaybeWithoutError<string>().ErrorOrNull() == null);
            PAssert.That(() => Maybe.Error(1337).AsMaybeWithoutValue<int?>().ErrorOrNullable() == 1337);
            PAssert.That(() => Maybe.Ok((int?)42).AsMaybeWithoutError<string>().ErrorOrNull() == null);
        }

        [Fact]
        public void ValueOrNull_is_null_iif_ok()
        {
            PAssert.That(() => Maybe.ErrorWhenNotNull("asd").ValueOrNullable() == null);
            PAssert.That(() => Maybe.Ok(3).AsMaybeWithoutError<string>().ValueOrNullable() == 3);
            PAssert.That(() => Maybe.Error(1337).AsMaybeWithoutValue<string>().ValueOrNull() == null);
            PAssert.That(() => Maybe.Ok("42").AsMaybeWithoutError<string>().ValueOrNull() == "42");
            PAssert.That(() => Maybe.Error(1337).AsMaybeWithoutValue<int?>().ValueOrNull() == null);
            PAssert.That(() => Maybe.Ok((int?)42).AsMaybeWithoutError<string>().ValueOrNull() == 42);
        }

        [Fact]
        public void AssertOk_crashes_iif_error()
        {
            Assert.ThrowsAny<Exception>(() => Maybe.ErrorWhenNotNull("asd").AssertOk());
            PAssert.That(() => Maybe.Ok(3).AsMaybeWithoutError<string>().AssertOk() == 3);
        }

        [Fact]
        public void AssertError_crashes_iif_Ok()
        {
            Assert.ThrowsAny<Exception>(() => Maybe.OkWhenNotNull("asd").AssertError());
            PAssert.That(() => Maybe.Error(3).AsMaybeWithoutValue<string>().AssertError() == 3);
        }

        [Fact]
        public void WhenOk_calls_method_iif_ok()
        {
            var notOk_whenOk_called = false;
            var notOkExample = Maybe.ErrorWhenNotNull("asd").WhenOk(
                _ => {
                    notOk_whenOk_called = true;
                    return 42;
                });
            PAssert.That(() => notOk_whenOk_called == false);

            var okExample = Maybe.Ok(3).AsMaybeWithoutError<string>().WhenOk(i => i * 14);
            PAssert.That(() => okExample.Contains(42));
            PAssert.That(() => notOkExample.ContainsError("asd"));

            Action<int> action = _ => { };
            PAssert.That(() => okExample.WhenOk(action).Contains(Unit.Value));
            PAssert.That(() => notOkExample.WhenOk(action).Contains(Unit.Value) == false);
        }

        [Fact]
        public void WhenOk_for_Unit_calls_method_iif_ok()
        {
            var notOkExample = Maybe.ErrorWhenNotNull("asd");

            var okExample = Maybe.OkWhenNotNull((int?)3).WhenError(() => "asd").WhenOk(_ => { });

            PAssert.That(() => okExample.Contains(Unit.Value));
            PAssert.That(() => notOkExample.ContainsError("asd"));

            PAssert.That(() => okExample.WhenOk(() => DayOfWeek.Friday).Contains(DayOfWeek.Friday));
            PAssert.That(() => notOkExample.WhenOk(() => DayOfWeek.Friday).Contains(DayOfWeek.Friday) == false);
        }

        [Fact]
        public void IfOk_calls_delegate_iif_ok()
        {
            var notOkExample = Maybe.ErrorWhenNotNull("asd");
            var okExample = Maybe.Ok(3).AsMaybeWithoutError<string>();

            var notOkCalled = false;
            notOkExample.IfOk(_ => notOkCalled = true);
            PAssert.That(() => notOkCalled == false);

            var okCalled = false;
            okExample.IfOk(_ => okCalled = true);
            PAssert.That(() => okCalled);
        }

        [Fact]
        public void IfOk_Unit_calls_delegate_iif_ok()
        {
            var notOkExample = Maybe.ErrorWhenNotNull("asd");
            var okExample = Maybe.Ok().AsMaybeWithoutError<string>();

            var notOkCalled = false;
            notOkExample.IfOk(() => notOkCalled = true);
            PAssert.That(() => notOkCalled == false);

            var okCalled = false;
            okExample.IfOk(() => okCalled = true);
            PAssert.That(() => okCalled);
        }

        [Fact]
        public void If_calls_appropriate_method()
        {
            var notOkExample = Maybe.Error().AsMaybeWithoutValue<Unit>();
            var okExample = Maybe.Ok().AsMaybeWithoutError<Unit>();

            bool? notOkResult = null;
            notOkExample.If(_ => notOkResult = true, _ => notOkResult = false);
            PAssert.That(() => notOkResult == false);

            notOkResult = null;
            notOkExample.If(() => notOkResult = true, _ => notOkResult = false);
            PAssert.That(() => notOkResult == false);

            notOkResult = null;
            notOkExample.If(_ => notOkResult = true, () => notOkResult = false);
            PAssert.That(() => notOkResult == false);

            notOkResult = null;
            notOkExample.If(() => notOkResult = true, () => notOkResult = false);
            PAssert.That(() => notOkResult == false);

            bool? okResult = null;
            okExample.If(_ => okResult = true, _ => okResult = false);
            PAssert.That(() => okResult == true);

            okResult = null;
            okExample.If(() => okResult = true, _ => okResult = false);
            PAssert.That(() => okResult == true);

            okResult = null;
            okExample.If(_ => okResult = true, () => okResult = false);
            PAssert.That(() => okResult == true);

            okResult = null;
            okExample.If(() => okResult = true, () => okResult = false);
            PAssert.That(() => okResult == true);
        }

        [Fact]
        public void IfError_calls_delegate_iif_in_state_error()
        {
            var notOkExample = Maybe.Error().AsMaybeWithoutValue<Unit>();
            var okExample = Maybe.Ok().AsMaybeWithoutError<Unit>();

            var notOkCalled = false;
            notOkExample.IfError(_ => notOkCalled = true);
            PAssert.That(() => notOkCalled);

            var okCalled = false;
            okExample.IfError(_ => okCalled = true);
            PAssert.That(() => okCalled == false);

            var notOkCalled2 = false;
            notOkExample.IfError(() => notOkCalled2 = true);
            PAssert.That(() => notOkCalled2);

            var okCalled2 = false;
            okExample.IfError(() => okCalled2 = true);
            PAssert.That(() => okCalled2 == false);
        }

        [Fact]
        public void Deconstruct_deconstructs_Ok()
        {
            var (ok, okVal, errVal) = Maybe.Either(true, 13, "13");
            PAssert.That(() => ok && okVal == 13 && errVal == null);
        }

        [Fact]
        public void Deconstruct_deconstructs_errors()
        {
            var (ok, okVal, errVal) = Maybe.Either(false, 13, "13");
            PAssert.That(() => !ok && okVal == 0 && errVal == "13");
        }

        [Fact]
        public void WhenAllOk_simple_cases_work()
        {
            PAssert.That(() => Array.Empty<Maybe<Unit, Unit>>().WhenAllOk().IsOk);
            PAssert.That(() => TwoOkMaybes.WhenAllOk().Contains(ok => ok.SequenceEqual(new[] { 1, 2 })));
            PAssert.That(() => ThreeMixedMaybes.WhenAllOk().ContainsError(ok => ok.SequenceEqual(new[] { 1, 2 })));
        }

        [Fact]
        public void WhereOk_simple_cases_work()
        {
            PAssert.That(() => Array.Empty<Maybe<Unit, Unit>>().WhereOk().None());
            PAssert.That(() => TwoOkMaybes.WhereOk().SequenceEqual(new[] { 1, 2 }));
            PAssert.That(() => ThreeMixedMaybes.WhereOk().SequenceEqual(new[] { Unit.Value }));
        }

        [Fact]
        public void WhereError_simple_cases_work()
        {
            PAssert.That(() => Array.Empty<Maybe<Unit, Unit>>().WhereError().None());
            PAssert.That(() => TwoOkMaybes.WhereError().None());
            PAssert.That(() => ThreeMixedMaybes.WhereError().SequenceEqual(new[] { 1, 2, }));
        }

        [Fact]
        public void Partition_simple_cases_work()
        {
            var emptyPartitioned = Array.Empty<Maybe<Unit, Unit>>().Partition();
            PAssert.That(() => emptyPartitioned.errorValues.None() && emptyPartitioned.okValues.None());
            PAssert.That(() => emptyPartitioned.okValues.None());

            var twoOkPartitioned = TwoOkMaybes.Partition();
            PAssert.That(() => twoOkPartitioned.okValues.SequenceEqual(new[] { 1, 2 }));
            PAssert.That(() => twoOkPartitioned.errorValues.None());

            var threeMixedPartitioned = ThreeMixedMaybes.Partition();
            PAssert.That(() => threeMixedPartitioned.okValues.SequenceEqual(new[] { Unit.Value }));
            PAssert.That(() => threeMixedPartitioned.errorValues.SequenceEqual(new[] { 1, 2, }));
        }

        static Maybe<Unit, int>[] ThreeMixedMaybes
            => new[] { Maybe.Error(1).AsMaybeWithoutValue<Unit>(), Maybe.Ok(), Maybe.Error(2) };

        static Maybe<int, Unit>[] TwoOkMaybes
            => new[] { Maybe.Ok(1).AsMaybeWithoutError<Unit>(), Maybe.Ok(2) };

        [Fact]
        public void WhenOkTry_is_ok_iif_both_input_and_delegate_are_ok()
        {
            var okExample = Maybe.Ok().AsMaybeWithoutError<Unit>();
            var notOkExample = Maybe.Error().AsMaybeWithoutValue<Unit>();

            PAssert.That(() => okExample.WhenOkTry(_ => okExample).IsOk);
            PAssert.That(() => okExample.WhenOkTry(_ => notOkExample).IsError);
            PAssert.That(() => notOkExample.WhenOkTry(_ => okExample).IsError);
            PAssert.That(() => notOkExample.WhenOkTry(_ => notOkExample).IsError);

            PAssert.That(() => okExample.WhenOkTry(() => okExample).IsOk);
            PAssert.That(() => okExample.WhenOkTry(() => notOkExample).IsError);
            PAssert.That(() => notOkExample.WhenOkTry(() => okExample).IsError);
            PAssert.That(() => notOkExample.WhenOkTry(() => notOkExample).IsError);
        }

        [Fact]
        public void WhenErrorTry_is_ok_iif_either_input_or_delegate_are_ok()
        {
            var okExample = Maybe.Ok().AsMaybeWithoutError<Unit>();
            var notOkExample = Maybe.Error().AsMaybeWithoutValue<Unit>();

            PAssert.That(() => okExample.WhenErrorTry(_ => okExample).IsOk);
            PAssert.That(() => okExample.WhenErrorTry(_ => notOkExample).IsOk);
            PAssert.That(() => notOkExample.WhenErrorTry(_ => okExample).IsOk);
            PAssert.That(() => notOkExample.WhenErrorTry(_ => notOkExample).IsError);

            PAssert.That(() => okExample.WhenErrorTry(() => okExample).IsOk);
            PAssert.That(() => okExample.WhenErrorTry(() => notOkExample).IsOk);
            PAssert.That(() => notOkExample.WhenErrorTry(() => okExample).IsOk);
            PAssert.That(() => notOkExample.WhenErrorTry(() => notOkExample).IsError);
        }

        [Fact]
        public void Linq_integrated_SelectMany_is_ok_when_both_sources_are()
        {
            var okExample = Maybe.Ok().AsMaybeWithoutError<Unit>();
            var notOkExample = Maybe.Error().AsMaybeWithoutValue<Unit>();

            PAssert.That(
                () => (
                    from a in okExample
                    let v = 1
                    from b in okExample
                    select v
                ).Contains(1));
            PAssert.That(
                () => (
                    from a in okExample
                    let v = 1
                    from b in notOkExample
                    select v
                ).IsError);
            PAssert.That(
                () => (
                    from a in notOkExample
                    let v = 1
                    from b in okExample
                    select v
                ).IsError);
            PAssert.That(
                () => (
                    from a in notOkExample
                    let v = 1
                    from b in notOkExample
                    select v
                ).IsOk == false);
        }

        [Fact]
        public void Default_Maybe_throws()
        {
            Assert.ThrowsAny<Exception>(() => default(Maybe<object, object>).TryGet(out _, out _));
            Assert.ThrowsAny<Exception>(() => default(Maybe<object, object>).Extract(_ => 0, _ => 0));
        }

#pragma warning disable 618 //obsoletion is kind of the point.
        [Fact]
        public void WhenOk_with_nested_maybe_state_gives_compiler_error()
            => Maybe.Ok().AsMaybeWithoutError<string>()
                .WhenOk(_ => Maybe.Error("err").AsMaybeWithoutValue<Unit>())
                .AssertOk();
#pragma warning restore 618
    }
}
