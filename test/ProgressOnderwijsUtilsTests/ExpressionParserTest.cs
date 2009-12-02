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
			return ExpressionParser.Parse(expressie, null);
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

				yield return new TestCaseData("2 ^ 8").Returns(256);
				yield return new TestCaseData("((((2 ^ 8))))").Returns(256);
				yield return new TestCaseData("((12 - 10) + (2 ^ 8))").Returns(258);
			}
		}

	}
}
