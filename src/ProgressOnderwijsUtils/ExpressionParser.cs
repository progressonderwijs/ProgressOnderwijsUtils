using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
	public static class ExpressionParser
	{
		public delegate double ParserDelegate(string variable);
		private static event ParserDelegate parserdelegate;

		static Stack<double> Variable;
		static int current;
		static List<Token> tokens;

		public static double Parse(string expression, ParserDelegate pd)
		{
			parserdelegate = pd;
			Variable = new Stack<double>();
			tokens = new Scanner().Scan(expression);
			current = 0;

			Expression();

			return Variable.Count > 0 ? Variable.Pop() : (double)0;
		}

		private static void Expression()
		{
			if (!Eof())
				ExpressionAddSubtract();
		}

		private static void ExpressionAddSubtract()
		{
			ExpressionMultiplyDivide();

			string value;
			while (!Eof() && tokens[current].Type == TokenType.Delimiter && (tokens[current].Value == "+" || tokens[current].Value == "-"))
			{
				value = tokens[current].Value;
				++current;
				ExpressionMultiplyDivide();
				Arithmetic(value);
			}
		}

		private static void ExpressionMultiplyDivide()
		{
			ExpressionExponent();

			string value;
			while (!Eof() && tokens[current].Type == TokenType.Delimiter && (tokens[current].Value == "*" || tokens[current].Value == "/" || tokens[current].Value == "%"))
			{
				value = tokens[current].Value;
				++current;
				ExpressionExponent();
				Arithmetic(value);
			}
		}

		private static void ExpressionExponent()
		{
			ExpressionSign();

			if (!Eof() && tokens[current].Type == TokenType.Delimiter && tokens[current].Value == "^")
			{
				++current;
				ExpressionSign();
				Arithmetic("^");
			}
		}

		private static void ExpressionSign()
		{
			string value = null;
			if (!Eof() && tokens[current].Type == TokenType.Delimiter && (tokens[current].Value == "+" || tokens[current].Value == "-"))
			{
				value = tokens[current].Value;
				++current;
			}
			ExpressionParentheses();
			if (value != null)
				ArithmeticSign(value);
		}

		private static void ExpressionParentheses()
		{
			if (!Eof() && tokens[current].Type == TokenType.Delimiter && tokens[current].Value == "(")
			{
				++current;
				ExpressionAddSubtract();
				if (!Eof() && tokens[current].Type == TokenType.Delimiter && tokens[current].Value != ")")
					throw new NietZoErgeException(") expected");
			}
			else
			{
				ExpressionPrimitive();
			}
			++current;
		}

		private static void ExpressionPrimitive()
		{
			switch (tokens[current].Type)
			{
				case TokenType.NumericConstant:
					Variable.Push(Double.Parse(tokens[current].Value, CultureInfo.InvariantCulture.NumberFormat));
					break;
				case TokenType.Variable:
					if (parserdelegate == null)
						throw new NietZoErgeException("Parser needs delegate when using variables");
					Variable.Push(parserdelegate(tokens[current].Value));
					break;
			}
		}

		private static void ArithmeticSign(string op)
		{
			if (Variable.Count == 0)
				throw new NietZoErgeException(op + " misplaced");
			double left = Variable.Pop();
			Variable.Push(op == "-" ? -1 * left : left);
		}

		private static void Arithmetic(string op)
		{
			if (Variable.Count < 2)
				throw new NietZoErgeException(op + " misplaced");

			double right = Variable.Pop();
			double left = Variable.Pop();

			switch (op)
			{
				case "+":
					Variable.Push(left + right);
					break;
				case "-":
					Variable.Push(left - right);
					break;
				case "*":
					Variable.Push(left * right);
					break;
				case "/":
					Variable.Push(left / right);
					break;
				case "%":
					Variable.Push(left % right);
					break;
				case "^":
					Variable.Push(Math.Pow(left, right));
					break;
			}
		}

		private static bool Eof()
		{
			return current == tokens.Count;
		}
	}
	
	public enum TokenType
	{
		Delimiter,
		NumericConstant,
		Variable
	}

	public class Token
	{
		public TokenType Type { get; set; }
		public string Value { get; set; }
	}

	public class Scanner
	{
		int current = 0;
		string expression;

		public List<Token> Scan(string s)
		{
			Token token;
			List<Token> tokens = new List<Token>();
			expression = s;

			while (current < expression.Length)
			{
				while (current < expression.Length && IsWhite(expression[current]))
					++current;

				// Delimiter?
				if (IsDelimiter(expression[current]))
				{
					token = new Token();
					token.Type = TokenType.Delimiter;
					token.Value = Convert.ToString(expression[current++]);
					tokens.Add(token);
					continue;
				}

				// Numeric literal?
				if(IsNumeric(expression[current]))
				{
					token = new Token();
					token.Type = TokenType.NumericConstant;
					while (current < expression.Length && IsNumeric(expression[current]))
					{
						token.Value = token.Value + expression[current++];
					}
					tokens.Add(token);
					continue;
				}
				
				// variable
				token = new Token();
				token.Type = TokenType.Variable;
				while (current < expression.Length && !IsWhite(expression[current]) && !IsDelimiter(expression[current]))
				{
					token.Value = token.Value + expression[current++];
				}
				tokens.Add(token);
			}
			return tokens;
		}

		private void SkipWhite()
		{
			while (current < expression.Length)
			{
				if (!IsWhite(expression[current]))
				{
					if (current > 0)
						--current;
					break;
				}
				++current;
			}
		}
	
		private bool IsNumeric(char c)
		{
			return "0123456789.".IndexOf(c) != -1;
		}

		private bool IsDelimiter(char c)
		{
			return "/*-+^%()".IndexOf(c) != -1;
		}

		private bool IsWhite(char c)
		{
			return " \t\r\n".IndexOf(c) != -1;
		}
	}	
}
