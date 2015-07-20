using NUnit.Framework;

using System;
using System.Text.RegularExpressions;

using Oracular.Spec;

namespace Oracular.Tests
{
	[TestFixture]
	public class LexerTests
	{
		[Test]
		public void ReturnEOF ()
		{
			var lexer = new StringLexer ("");

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);

			token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);

			token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		[Test]
		public void IgnoreComments ()
		{
			var lexer = new StringLexer ("# this is a comment");

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		[Test]
		[TestCase("    ")]
		[TestCase(" \r ")]
		[TestCase(" \t ")]
		[TestCase(" \n ")]
		public void IgnoreWhitespace (string input)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		[Test]
		[TestCase("<", "<")]
		[TestCase(">", ">")]
		[TestCase("<=", "<=")]
		[TestCase(">=", ">=")]
		[TestCase("=", "=")]
		[TestCase("!=", "!=")]
		[TestCase("+", "+")]
		[TestCase("-", "-")]
		[TestCase("*", "*")]
		[TestCase("/", "/")]
		[TestCase("and", "and")]
		[TestCase("or", "or")]
		[TestCase("   <   ", "<")]
		[TestCase("   >   ", ">")]
		[TestCase("   <=   ", "<=")]
		[TestCase("   >=   ", ">=")]
		[TestCase("   =   ", "=")]
		[TestCase("   !=   ", "!=")]
		[TestCase("   +   ", "+")]
		[TestCase("   -   ", "-")]
		[TestCase("   *   ", "*")]
		[TestCase("   /   ", "/")]
		[TestCase("   and   ", "and")]
		[TestCase("   or   ", "or")]
		[TestCase("AND", "and")]
		[TestCase("OR", "or")]
		public void LexOperators (string input, string expected)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.Operator, token.Type);
			Assert.AreEqual (expected, token.GetRawValue ());

			token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		[Test]
		[TestCase("42", 42)]
		[TestCase("4.2", 4.2)]
		[TestCase("-42", -42)]
		[TestCase("-4.2", -4.2)]
		public void LexNumbers (string input, double expected)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.Number, token.Type);
			Assert.AreEqual (expected, token.GetNumberValue ());

			token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		[Test]
		[TestCase("'foobar'", "foobar")]
		[TestCase("\"foobar\"", "foobar")]
		[TestCase("'\\'\\''", "''")]
		[TestCase("\"\\\"\\\"\"", "\"\"")]
		[TestCase("'\\\\'", "\\")]
		public void LexStrings (string input, string expected)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.String, token.Type);
			Assert.AreEqual (expected, token.GetStringValue ());

			token = lexer.GetToken ();
			Assert.AreEqual (TokenType.EOF, token.Type);
		}

		private static Regex NOT_CLOSED_RE = new Regex("not closed", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ErrorOnUnfinishedSingleQuote()
		{
			var lexer = new StringLexer ("'foobar");

			var ex = Assert.Throws<LexerException> (() => lexer.GetToken ());
			Assert.That (NOT_CLOSED_RE.IsMatch (ex.Message));
		}

		[Test]
		public void ErrorOnUnfinishedDoubleQuote()
		{
			var lexer = new StringLexer ("\"foobar");

			var ex = Assert.Throws<LexerException> (() => lexer.GetToken ());
			Assert.That (NOT_CLOSED_RE.IsMatch (ex.Message));
		}

		[Test]
		[TestCase("foo", new string[]{ "foo" })]
		[TestCase("foo.bar", new string[]{ "foo", "bar" })]
		[TestCase("foo.bar.baz", new string[]{ "foo", "bar", "baz" })]
		public void LexReferences (string input, string[] expected)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();
			Assert.AreEqual (TokenType.Reference, token.Type);

			var segments = token.GetReferenceValue ();
			Assert.AreEqual (expected.Length, segments.Length);

			for (var i = 0; i < expected.Length; i += 1)
			{
				Assert.AreEqual (expected [i], segments [i]);
			}
		}

		private static Regex INVALID_RE = new Regex("invalid", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ErrorOnInvalidInput ()
		{
			var lexer = new StringLexer ("{foobar}");

			var ex = Assert.Throws<LexerException> (() => lexer.GetToken ());

			Assert.That (INVALID_RE.IsMatch (ex.Message));
		}

		[Test]
		[TestCase("(", TokenType.OpenParen)]
		[TestCase(",", TokenType.Comma)]
		[TestCase(")", TokenType.CloseParen)]
		public void LexFunctionCallDelimiters(string input, TokenType expected)
		{
			var lexer = new StringLexer (input);

			var token = lexer.GetToken ();

			Assert.AreEqual (expected, token.Type);
		}
	}
}

