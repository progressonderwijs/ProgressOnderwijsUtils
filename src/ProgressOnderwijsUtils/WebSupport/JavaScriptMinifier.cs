using System;
using System.IO;

/* Original source: http://crockford.com/javascript/jsmin
 * Originally written in 'C', this code has been converted to the C# language.
 * The author's copyright message is reproduced below.
 * All modifications from the original to C# are placed in the public domain.
 * Additional modifications by eamon 2009-06-22: removed command line interface, added static method to help conversion.
 */

/* jsmin.c
   2007-05-22

Copyright (c) 2002 Douglas Crockford  (www.crockford.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

The Software shall be used for Good, not Evil.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace ProgressOnderwijsUtils.WebSupport
{
	public class JavaScriptMinifier
	{
		const int EOF = -1;

		TextReader input;
		TextWriter output;
		int theA;
		int theB;
		int theLookahead = EOF;

		JavaScriptMinifier(TextReader inputParam, TextWriter outputParam)
		{
			this.input = inputParam;
			this.output = outputParam;
		}

		public static void Minify(TextReader input, TextWriter output) { new JavaScriptMinifier(input, output).jsmin(); }

		/// <summary>
		/// Copy the input to the output, deleting the characters which are
		/// insignificant to JavaScript. Comments will be removed. Tabs will be
		/// replaced with spaces. Carriage returns will be replaced with linefeeds.
		/// Most spaces and linefeeds will be removed.
		/// </summary>
		void jsmin()
		{
			theA = '\n';
			action(3);
			while (theA != EOF)
			{
				switch (theA)
				{
					case ' ':
						{
							if (isAlphanum(theB))
							{
								action(1);
							}
							else
							{
								action(2);
							}
							break;
						}
					case '\n':
						{
							switch (theB)
							{
								case '{':
								case '[':
								case '(':
								case '+':
								case '-':
									{
										action(1);
										break;
									}
								case ' ':
									{
										action(3);
										break;
									}
								default:
									{
										if (isAlphanum(theB))
										{
											action(1);
										}
										else
										{
											action(2);
										}
										break;
									}
							}
							break;
						}
					default:
						{
							switch (theB)
							{
								case ' ':
									{
										if (isAlphanum(theA))
										{
											action(1);
											break;
										}
										action(3);
										break;
									}
								case '\n':
									{
										switch (theA)
										{
											case '}':
											case ']':
											case ')':
											case '+':
											case '-':
											case '"':
											case '\'':
												{
													action(1);
													break;
												}
											default:
												{
													if (isAlphanum(theA))
													{
														action(1);
													}
													else
													{
														action(3);
													}
													break;
												}
										}
										break;
									}
								default:
									{
										action(1);
										break;
									}
							}
							break;
						}
				}
			}
		}


		/// <summary>
		/// action -- do something! What you do is determined by the argument:
		///     1   Output A. Copy B to A. Get the next B.
		///     2   Copy B to A. Get the next B. (Delete A).
		///     3   Get the next B. (Delete B).
		/// action treats a string as a single character. Wow!
		/// action recognizes a regular expression if it is preceded by ( or , or =. 
		/// </summary>
		/// <param name="d">What to do.</param>
		void action(int d)
		{
			if (d <= 1)
			{
				put(theA);
			}
			if (d <= 2)
			{
				theA = theB;
				if (theA == '\'' || theA == '"')
				{
					for (; ; )
					{
						put(theA);
						theA = get();
						if (theA == theB)
						{
							break;
						}
						if (theA <= '\n')
						{
							throw new Exception(string.Format("Error: JSMIN unterminated string literal: {0}\n", theA));
						}
						if (theA == '\\')
						{
							put(theA);
							theA = get();
						}
					}
				}
			}
			if (d <= 3)
			{
				theB = next();
				if (theB == '/' && (theA == '(' || theA == ',' || theA == '=' ||
									theA == '[' || theA == '!' || theA == ':' ||
									theA == '&' || theA == '|' || theA == '?' ||
									theA == '{' || theA == '}' || theA == ';' ||
									theA == '\n'))
				{
					put(theA);
					put(theB);
					for (; ; )
					{
						theA = get();
						if (theA == '/')
						{
							break;
						}
						else if (theA == '\\')
						{
							put(theA);
							theA = get();
						}
						else if (theA <= '\n')
						{
							throw new Exception(string.Format("Error: JSMIN unterminated Regular Expression literal : {0}.\n", theA));
						}
						put(theA);
					}
					theB = next();
				}
			}
		}

		/// <summary>
		/// next -- get the next character, excluding comments. peek() is used to see
		///    if a '/' is followed by a '/' or '*'. 
		/// </summary>
		/// <returns>the next character</returns>
		int next()
		{
			int c = get();
			if (c == '/')
			{
				switch (peek())
				{
					case '/':
						{
							for (; ; )
							{
								c = get();
								if (c <= '\n')
								{
									return c;
								}
							}
						}
					case '*':
						{
							get();
							for (; ; )
							{
								switch (get())
								{
									case '*':
										{
											if (peek() == '/')
											{
												get();
												return ' ';
											}
											break;
										}
									case EOF:
										{
											throw new Exception("Error: JSMIN Unterminated comment.\n");
										}
								}
							}
						}
					default:
						{
							return c;
						}
				}
			}
			return c;
		}

		/// <summary>
		/// peek -- get the next character without getting it.
		/// </summary>
		/// <returns>the value of the next character</returns>
		int peek()
		{
			theLookahead = get();
			return theLookahead;
		}
		/// <summary>
		/// get -- return the next character from stdin. Watch out for lookahead. If
		///        the character is a control character, translate it to a space or
		///        linefeed.
		/// </summary>
		/// <returns>the translated next character</returns>
		int get()
		{
			int c = theLookahead;
			theLookahead = EOF;
			if (c == EOF)
			{
				c = input.Read();
			}
			if (c >= ' ' || c == '\n' || c == EOF)
			{
				return c;
			}
			if (c == '\r')
			{
				return '\n';
			}
			return ' ';
		}
		void put(int c)
		{
			output.Write((char)c);
		}

		/// <summary>
		/// isAlphanum -- return true if the character is a letter, digit, underscore,
		///	dollar sign, or non-ASCII character. 
		/// </summary>
		/// <param name="c">the character to test</param>
		/// <returns>whether it's an alphanumeric character.</returns>
		bool isAlphanum(int c)
		{
			return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
				c > 126);
		}
	}
}
