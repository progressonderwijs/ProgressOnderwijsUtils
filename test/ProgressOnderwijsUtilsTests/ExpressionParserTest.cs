using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class ExpressionParserTest
	{
		[Test]

		[TestCaseSource("Expressions")]
		public double TestExpression(string expressie)
		{
			return ExpressionParser.Parse(expressie, Variabelen);
		}

		public double Variabelen(string variabele)
		{
			switch (variabele)
			{
				default:
					return (double)0;
				case "A":
					return (double)1;
				case "B":
					return (double)2;
				case "C":
					return (double)8;
				case "D":
					return (double)10;
				case "E":
					return (double)12;
			}
		}

		public IEnumerable Expressions
		{
			get
			{
				yield return new TestCaseData("1 + 2").Returns(3);
				yield return new TestCaseData("1 - 2").Returns(-1);
				yield return new TestCaseData("1 * 2").Returns(2);
				yield return new TestCaseData("1 / 2").Returns(0.5);
				yield return new TestCaseData("1 % 2").Returns(1);
				yield return new TestCaseData("1 ^ 2").Returns(1);

				yield return new TestCaseData("A + B").Returns(3);
				yield return new TestCaseData("A - B").Returns(-1);
				yield return new TestCaseData("A * B").Returns(2);
				yield return new TestCaseData("A / B").Returns(0.5);
				yield return new TestCaseData("A % B").Returns(1);
				yield return new TestCaseData("A ^ B").Returns(1);

				yield return new TestCaseData("1 + B").Returns(3);
				yield return new TestCaseData("A - 2").Returns(-1);
				yield return new TestCaseData("1 * B").Returns(2);
				yield return new TestCaseData("A / 2").Returns(0.5);
				yield return new TestCaseData("1 % B").Returns(1);
				yield return new TestCaseData("A ^ 2").Returns(1);

				yield return new TestCaseData("2 ^ 8").Returns(256);
				yield return new TestCaseData("((((2 ^ 8))))").Returns(256);
				yield return new TestCaseData("((12 - 10) + (2 ^ 8))").Returns(258);

				yield return new TestCaseData("B ^ C").Returns(256);
				yield return new TestCaseData("((((B ^ C))))").Returns(256);
				yield return new TestCaseData("((E - D) + (B ^ C))").Returns(258);

				yield return new TestCaseData("B ^ 8").Returns(256);
				yield return new TestCaseData("((((2 ^ C))))").Returns(256);
				yield return new TestCaseData("((12 - D) + (2 ^ C))").Returns(258);
			}
		}

	}
}
