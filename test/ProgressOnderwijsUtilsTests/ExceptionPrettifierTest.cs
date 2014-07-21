﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
//using ApprovalTests;//using ApprovalTests.Reporters;
using ExpressionToCodeLib;
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
		public void IndirectErrorViaInterface<T>(T param, string arg2 = "bla")
		{
			((IExampleTestInterface)new NestedClass()).SomeMethod();
		}


		interface IExampleTestInterface
		{
			void SomeMethod();
		}


		class NestedClass : IExampleTestInterface
		{
			void IExampleTestInterface.SomeMethod() { throw new NotImplementedException(); }
		}
	}


	//[UseReporter(typeof(DiffReporter))]
	public class ExceptionPrettifierTest
	{
		[Test, MethodImpl(MethodImplOptions.NoInlining), Continuous]
		public void TrivialStackTraceWorks()
		{
			try
			{
				ExampleTestClass.CausesError();
			}
			catch (Exception e)
			{
				PAssert.That(()=>
					ExceptionPrettifier.PrettyPrintException(e) == "This is an exception\n   at ProgressOnderwijsUtilsTests.ExampleTestClass.CausesError() in test\\Tools\\ExceptionPrettifierTest.cs:line 18\n   at ProgressOnderwijsUtilsTests.ExceptionPrettifierTest.TrivialStackTraceWorks() in test\\Tools\\ExceptionPrettifierTest.cs:line 49\n");
			}
		}

		[Test, MethodImpl(MethodImplOptions.NoInlining), Continuous]
		public void ExplicitInterfaceImplementationInNestedClass()
		{
			try
			{
				new ExampleTestClass().IndirectErrorViaInterface(42);
			}
			catch (Exception e)
			{
				PAssert.That(()=>
					ExceptionPrettifier.PrettyPrintException(e) == "The method or operation is not implemented.\n   at ProgressOnderwijsUtilsTests.ExampleTestClass.NestedClass.SomeMethod() in test\\Tools\\ExceptionPrettifierTest.cs:line 36\n   at ProgressOnderwijsUtilsTests.ExampleTestClass.IndirectErrorViaInterface<T>(T param, string arg2) in test\\Tools\\ExceptionPrettifierTest.cs:line 24\n   at ProgressOnderwijsUtilsTests.ExceptionPrettifierTest.ExplicitInterfaceImplementationInNestedClass() in test\\Tools\\ExceptionPrettifierTest.cs:line 63\n");
			}
		}
	}
}