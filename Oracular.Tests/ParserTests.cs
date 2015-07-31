using NUnit.Framework;

using System;
using System.Text.RegularExpressions;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular.Tests
{
	[TestFixture]
	public class ParserTests
	{
		private Token numberToken (double n)
		{
			return new Token (TokenType.Number, n.ToString());
		}

		private Token stringToken (string s)
		{
			return new Token (TokenType.String, s);
		}

		private Token referenceToken (params string[] segments)
		{
			return new Token (TokenType.Reference, String.Join (".", segments));
		}

		private Token operatorToken (string op)
		{
			return new Token (TokenType.Operator, op);
		}

		private static readonly Token openParenToken = new Token(TokenType.OpenParen);
		private static readonly Token closeParenToken = new Token(TokenType.CloseParen);
		private static readonly Token commaToken = new Token(TokenType.Comma);

		private Parser makeParser (params Token[] input)
		{
			return new Parser(new ArrayLexer(input));
		}

		[Test]
		[TestCase(42)]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(4.2)]
		public void ParseNumbers (double value)
		{
			var parser = makeParser (
				numberToken(value)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<NumberLiteral> (tree);

			var asNumber = tree as NumberLiteral;
			Assert.AreEqual (value, asNumber.Value);
		}

		[Test]
		[TestCase("'foobar'", "foobar")]
		[TestCase("\"foobar\"", "foobar")]
		public void ParseStrings (string value, string expected)
		{
			var parser = makeParser (
				stringToken(value)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<StringLiteral> (tree);

			var asString = tree as StringLiteral;
			Assert.AreEqual (expected, asString.Value);
		}

		[Test]
		[TestCase("null")]
		[TestCase("NULL")]
		[TestCase("NuLl")]
		[TestCase("nUlL")]
		public void ParseNull (string source)
		{
			var parser = makeParser (
				referenceToken(source)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<NullLiteral> (tree);
		}

		[Test]
		[TestCase("true", true)]
		[TestCase("false", false)]
		[TestCase("TRUE", true)]
		[TestCase("FALSE", false)]
		public void ParseBooleans (string source, bool expected)
		{
			var parser = makeParser (
				referenceToken(source)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<BooleanLiteral> (tree);

			var asBool = tree as BooleanLiteral;
			Assert.AreEqual (expected, asBool.Value);
		}

		[Test]
		[TestCase("foo", new string[]{"foo"})]
		[TestCase("foo.bar", new string[]{"foo", "bar"})]
		[TestCase("foo.bar.baz", new string[]{"foo", "bar", "baz"})]
		public void ParseReference (string ignore, string[] segments)
		{
			var parser = makeParser (
				referenceToken(segments: segments)
            );

			var tree = parser.Parse ();
			Assert.IsInstanceOf<Reference> (tree);

			var asRef = tree as Reference;
			Assert.AreEqual (segments.Length, asRef.Value.Length);

			for (var i = 0; i < segments.Length; i += 1)
			{
				Assert.AreEqual (segments [i], asRef.Value [i]);
			}
		}

		[Test]
		[TestCase("truer")]
		[TestCase("atrue")]
		[TestCase("falsely")]
		[TestCase("isfalse")]
		[TestCase("nulletta")]
		[TestCase("notnull")]
		public void ParseKeywordLikeReferences (string value)
		{
			var parser = makeParser (
				referenceToken (value)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<Reference> (tree);

			var asRef = tree as Reference;
			Assert.AreEqual (1, asRef.Value.Length);
			Assert.AreEqual (value, asRef.Value [0]);
		}

		private static readonly Regex TOO_MUCH_RE = new Regex("too much", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ErrorOnTooMuchInput()
		{
			var parser = makeParser (
				numberToken(42),
				numberToken(43)
			);

			var ex = Assert.Throws<ParserException> (() => parser.Parse ());
			Assert.That (TOO_MUCH_RE.IsMatch (ex.Message));
		}

		private static readonly Regex NOT_ENOUGH_RE = new Regex("not enough", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ErrorOnNotEnoughInput()
		{
			var parser = makeParser (
				numberToken(42),
				operatorToken("+")
			);

			var ex = Assert.Throws<ParserException> (() => parser.Parse ());
			Assert.That (NOT_ENOUGH_RE.IsMatch (ex.Message));
		}

		[Test]
		[TestCase("+")]
		[TestCase("-")]
		[TestCase("*")]
		[TestCase("/")]
		[TestCase("=")]
		[TestCase("!=")]
		[TestCase("<")]
		[TestCase("<=")]
		[TestCase(">")]
		[TestCase(">=")]
		public void ParseSimpleBinary(string op)
		{
			var parser = makeParser (
				numberToken(1),
				operatorToken(op),
				numberToken(2)
			);

			var tree = parser.Parse ();
			Assert.IsInstanceOf<BinaryOperation> (tree);

			var asBinary = tree as BinaryOperation;
			Assert.AreEqual (op, asBinary.Operator);

			Assert.IsInstanceOf<NumberLiteral> (asBinary.Left);
			Assert.IsInstanceOf<NumberLiteral> (asBinary.Right);
		}

		[Test]
		public void ParseOperatorsWithPrecedence()
		{
			var one = numberToken (1);
			var two = numberToken (2);
			var three = numberToken (3);
			var four = numberToken (4);

			var equals = operatorToken ("=");
			var plus = operatorToken ("+");
			var times = operatorToken ("*");

			var left = makeParser (
				one, equals, two, plus, three, times, four
			);

			var right = makeParser (
				one, times, two, plus, three, equals, four
			);

			var leftTree = left.Parse ();
			var rightTree = right.Parse ();

			Assert.IsInstanceOf<BinaryOperation> (leftTree);
			Assert.IsInstanceOf<BinaryOperation> (rightTree);

			var leftBinary = leftTree as BinaryOperation;
			var rightBinary = rightTree as BinaryOperation;

			Assert.AreEqual ("=", leftBinary.Operator);
			Assert.AreEqual ("=", rightBinary.Operator);

			Assert.IsInstanceOf<NumberLiteral> (leftBinary.Left);
			Assert.IsInstanceOf<BinaryOperation> (leftBinary.Right);
			var leftRight = leftBinary.Right as BinaryOperation;

			Assert.IsInstanceOf<NumberLiteral> (rightBinary.Right);
			Assert.IsInstanceOf<BinaryOperation> (rightBinary.Left);
			var rightLeft = rightBinary.Left as BinaryOperation;

			Assert.AreEqual ("+", leftRight.Operator);
			Assert.AreEqual ("+", rightLeft.Operator);

			Assert.IsInstanceOf<NumberLiteral> (leftRight.Left);
			Assert.IsInstanceOf<BinaryOperation> (leftRight.Right);
			var leftRightRight = leftRight.Right as BinaryOperation;

			Assert.IsInstanceOf<NumberLiteral> (rightLeft.Right);
			Assert.IsInstanceOf<BinaryOperation> (rightLeft.Left);
			var rightLeftLeft = rightLeft.Left as BinaryOperation;

			Assert.AreEqual ("*", leftRightRight.Operator);
			Assert.AreEqual ("*", rightLeftLeft.Operator);
		}

		[Test]
		[TestCase("and")]
		[TestCase("aNd")]
		[TestCase("AND")]
		public void ParseConjunction(string source)
		{
			var parser = makeParser (
				referenceToken("true"),
				operatorToken(source),
				referenceToken("false")
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<LogicalConjunction> (tree);
		}

		[Test]
		[TestCase("or")]
		[TestCase("oR")]
		[TestCase("OR")]
		public void ParseDisjunction(string source)
		{
			var parser = makeParser (
				referenceToken("true"),
				operatorToken(source),
				referenceToken("false")
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<LogicalDisjunction> (tree);
		}

		[Test]
		public void ParseLogicWithPrecedence()
		{
			var one = numberToken (1);
			var two = numberToken (2);
			var three = numberToken (3);
			var four = numberToken (4);
			var five = numberToken (5);
			var six = numberToken (6);

			var both = operatorToken ("and");
			var either = operatorToken ("or");
			var less = operatorToken ("<");
			var equals = operatorToken ("=");
			var greater = operatorToken (">");

			var left = makeParser (
				one, less, two, both, three, equals, four, either, five, greater, six
			);

			var right = makeParser (
				one, less, two, either, three, equals, four, both, five, greater, six
			);

			var leftTree = left.Parse ();
			var rightTree = right.Parse ();

			Assert.IsInstanceOf<LogicalDisjunction> (leftTree);
			Assert.IsInstanceOf<LogicalDisjunction> (rightTree);

			var leftBinary = leftTree as LogicalDisjunction;
			var rightBinary = rightTree as LogicalDisjunction;

			Assert.IsInstanceOf<LogicalConjunction> (leftBinary.Left);
			Assert.IsInstanceOf<BinaryOperation> (leftBinary.Right);

			Assert.IsInstanceOf<LogicalConjunction> (rightBinary.Right);
			Assert.IsInstanceOf<BinaryOperation> (rightBinary.Left);
		}

		[Test]
		public void ParseFunctionCalls()
		{
			var parser = makeParser (
				referenceToken("foobar"),
				openParenToken,
				referenceToken("baz"),
				commaToken,
				referenceToken("qux"),
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<MacroExpansion> (tree);
			var asCall = tree as MacroExpansion;
			Assert.AreEqual ("foobar", asCall.Macro.Value[0]);

			Assert.AreEqual (2, asCall.Arguments.Length);
			Assert.IsInstanceOf<Reference> (asCall.Arguments [0]);
			Assert.IsInstanceOf<Reference> (asCall.Arguments [1]);

			var firstArg = asCall.Arguments [0] as Reference;
			var secondArg = asCall.Arguments [1] as Reference;

			Assert.AreEqual ("baz", firstArg.Value [0]);
			Assert.AreEqual ("qux", secondArg.Value [0]);
		}

		[Test]
		public void ParseFunctionCallsWithNestedExpressions()
		{
			var parser = makeParser (
				referenceToken("foobar"),
				openParenToken,
				referenceToken("baz"),
				operatorToken("+"),
				referenceToken("qux"),
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<MacroExpansion> (tree);
			var asCall = tree as MacroExpansion;
			Assert.AreEqual ("foobar", asCall.Macro.Value[0]);

			Assert.AreEqual (1, asCall.Arguments.Length);
			Assert.IsInstanceOf<BinaryOperation> (asCall.Arguments [0]);

			var firstArg = asCall.Arguments [0] as BinaryOperation;

			Assert.AreEqual ("+", firstArg.Operator);
		}

		[Test]
		public void ParseNestedFunctionCalls()
		{
			var parser = makeParser (
				referenceToken("foobar"),
				openParenToken,
				referenceToken("baz"),
				openParenToken,
				referenceToken("qux"),
				closeParenToken,
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<MacroExpansion> (tree);
			var asCall = tree as MacroExpansion;
			Assert.AreEqual ("foobar", asCall.Macro.Value[0]);

			Assert.AreEqual (1, asCall.Arguments.Length);
			Assert.IsInstanceOf<MacroExpansion> (asCall.Arguments [0]);

			var firstArg = asCall.Arguments [0] as MacroExpansion;

			Assert.AreEqual ("baz", firstArg.Macro.Value[0]);
		}

		[Test]
		public void ParseParenthesizedExpressions()
		{
			var parser = makeParser (
				numberToken(1),
				operatorToken("*"),
				openParenToken,
				numberToken(2),
				operatorToken("+"),
				numberToken(3),
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<BinaryOperation> (tree);
			var asBinary = tree as BinaryOperation;

			Assert.AreEqual ("*", asBinary.Operator);

			Assert.IsInstanceOf<BinaryOperation> (asBinary.Right);
			var rightSide = asBinary.Right as BinaryOperation;

			Assert.AreEqual ("+", rightSide.Operator);
		}

		[Test]
		public void ParseNestedParenthesizedExpressions()
		{
			var parser = makeParser (
				numberToken(1),
				operatorToken("*"),
				openParenToken,
				openParenToken,
				numberToken(2),
				operatorToken("+"),
				numberToken(3),
				closeParenToken,
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<BinaryOperation> (tree);
			var asBinary = tree as BinaryOperation;

			Assert.AreEqual ("*", asBinary.Operator);

			Assert.IsInstanceOf<BinaryOperation> (asBinary.Right);
			var rightSide = asBinary.Right as BinaryOperation;

			Assert.AreEqual ("+", rightSide.Operator);
		}

		[Test]
		public void ParseLogicalNegation()
		{
			var parser = makeParser (
				referenceToken("NOT"),
				openParenToken,
				referenceToken("false"),
				closeParenToken
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<LogicalNegation> (tree);
			var asNegation = tree as LogicalNegation;

			Assert.IsInstanceOf<BooleanLiteral> (asNegation.Child);
		}

		[Test]
		public void NegationDoesNotRequireParentheses()
		{
			var parser = makeParser (
				referenceToken("NOT"),
				referenceToken("false")
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<LogicalNegation> (tree);
			var asNegation = tree as LogicalNegation;

			Assert.IsInstanceOf<BooleanLiteral> (asNegation.Child);
		}

		[Test]
		public void NegationBindsTighterThanOperators()
		{
			var parser = makeParser (
				referenceToken("NOT"),
				referenceToken("false"),
				operatorToken("="),
				referenceToken("true")
			);

			var tree = parser.Parse ();

			Assert.IsInstanceOf<BinaryOperation> (tree);
			var asBinary = tree as BinaryOperation;

			Assert.IsInstanceOf<LogicalNegation> (asBinary.Left);
			var asNegation = asBinary.Left as LogicalNegation;

			Assert.IsInstanceOf<BooleanLiteral> (asNegation.Child);
			Assert.IsInstanceOf<BooleanLiteral> (asBinary.Right);
		}
	}
}

