using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ApprovalTests;
using NUnit.Framework;
using ProgressOnderwijsUtils.ErrorHandling;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    public class ExampleTestClass
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CausesError()
        {
            throw new Exception("This is an exception");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // ReSharper disable UnusedParameter.Global
        public void IndirectErrorViaInterface<T>(T param, string arg2 = "bla")
        {
            ((IExampleTestInterface)new NestedClass()).SomeMethod();
        }

        // ReSharper restore UnusedParameter.Global
        interface IExampleTestInterface
        {
            void SomeMethod();
        }

        class NestedClass : IExampleTestInterface
        {
            void IExampleTestInterface.SomeMethod()
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ExceptionPrettifierTest
    {
        static string ElideLineNumbers(string text) => Regex.Replace(text, @":line \d+", ":line ??");

        [Test, MethodImpl(MethodImplOptions.NoInlining), Continuous]
        public void TrivialStackTraceWorks()
        {
            try {
                ExampleTestClass.CausesError();
            } catch (Exception e) {
                Approvals.Verify(ElideLineNumbers(ExceptionPrettifier.PrettyPrintException(e)));
            }
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining), Continuous]
        public void ExplicitInterfaceImplementationInNestedClass()
        {
            try {
                new ExampleTestClass().IndirectErrorViaInterface(42);
            } catch (Exception e) {
                Approvals.Verify(ElideLineNumbers(ExceptionPrettifier.PrettyPrintException(e)));
            }
        }
    }
}
