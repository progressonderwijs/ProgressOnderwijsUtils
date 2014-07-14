using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Reporters;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.ErrorHandling;

namespace ProgressOnderwijsUtilsTests
{
	public class ExampleTestClass
	{
		interface IExampleTestInterface
		{
			void SomeMethod();
		}

		public static void CausesError()
		{
			throw new Exception("This is an exception");
		}
		class NestedClass : IExampleTestInterface
		{
			void IExampleTestInterface.SomeMethod()
			{
				throw new NotImplementedException();
			}
		}
		public void IndirectErrorViaInterface<T>(T param, string arg2 = "bla")
		{
			((IExampleTestInterface)new NestedClass()).SomeMethod();
		}
	}

	[UseReporter(typeof(DiffReporter))]
	public class ExceptionPrettifierTest
	{

		[Test, MethodImpl(MethodImplOptions.NoInlining)]
		public void TrivialStackTraceWorks()
		{
			try
			{
				ExampleTestClass.CausesError();
			}
			catch (Exception e)
			{
				Approvals.Verify(
					ExceptionPrettifier.PrettyPrintException(e));
			}
		}

		[Test, MethodImpl(MethodImplOptions.NoInlining)]
		public void ExplicitInterfaceImplementationInNestedClass()
		{
			try
			{
				new ExampleTestClass().IndirectErrorViaInterface(42);
			}
			catch (Exception e)
			{
				Approvals.Verify(
					ExceptionPrettifier.PrettyPrintException(e));
			}
		}

	}
}
