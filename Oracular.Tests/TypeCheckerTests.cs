using NUnit.Framework;

using System;
using System.Text.RegularExpressions;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular.Tests
{
	[TestFixture]
	public class TypeCheckerTests
	{
		[Test]
		public void CheckNullLiterals ()
		{
			var nullNode = new NullLiteral ();

			var type = nullNode.Walk (new TypeChecker());

			Assert.AreEqual (TypeSpecifier.Any, type);
		}

		[Test]
		public void CheckNumberLiterals ()
		{
			var numberNode = new NumberLiteral (42);

			var type = numberNode.Walk (new TypeChecker());

			Assert.AreEqual (TypeSpecifier.Number, type);
		}

		[Test]
		public void CheckStringLiterals ()
		{
			var stringNode = new StringLiteral ("foobar");

			var type = stringNode.Walk (new TypeChecker());

			Assert.AreEqual (TypeSpecifier.String, type);
		}

		[Test]
		[TestCase("+", TypeSpecifier.Number)]
		[TestCase("-", TypeSpecifier.Number)]
		[TestCase("*", TypeSpecifier.Number)]
		[TestCase("/", TypeSpecifier.Number)]
		[TestCase("=", TypeSpecifier.Boolean)]
		[TestCase("!=", TypeSpecifier.Boolean)]
		[TestCase("<", TypeSpecifier.Boolean)]
		[TestCase(">", TypeSpecifier.Boolean)]
		[TestCase("<=", TypeSpecifier.Boolean)]
		[TestCase(">=", TypeSpecifier.Boolean)]
		public void CheckBinaryOperations (string op, TypeSpecifier expected)
		{
			var binary = new BinaryOperation (op, new NumberLiteral (1), new NumberLiteral (2));

			var type = binary.Walk (new TypeChecker ());

			Assert.AreEqual (expected, type);
		}

		private static Regex INCOMPATIBLE_RE = new Regex("incompatible", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex INVALID_RE = new Regex("invalid", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ErrorOnTypeMismatch ()
		{
			var differentTypes = new BinaryOperation ("=", new NumberLiteral (1), new StringLiteral ("foobar"));

			var ex = Assert.Throws<TypeCheckException> (() => differentTypes.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (ex.Message));
		}

		[Test]
		public void ErrorOnNonNumberMath ()
		{
			var differentTypes = new BinaryOperation ("+", new StringLiteral ("foo"), new StringLiteral ("foobar"));

			var ex = Assert.Throws<TypeCheckException> (() => differentTypes.Walk (new TypeChecker ()));

			Assert.That (INVALID_RE.IsMatch (ex.Message));
		}
	}
}

