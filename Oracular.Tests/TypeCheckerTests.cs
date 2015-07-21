using NUnit.Framework;

using System;
using System.Collections.Generic;
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
		[TestCase("+", SpecType.Number)]
		[TestCase("-", SpecType.Number)]
		[TestCase("*", SpecType.Number)]
		[TestCase("/", SpecType.Number)]
		[TestCase("=", SpecType.Boolean)]
		[TestCase("!=", SpecType.Boolean)]
		[TestCase("<", SpecType.Boolean)]
		[TestCase(">", SpecType.Boolean)]
		[TestCase("<=", SpecType.Boolean)]
		[TestCase(">=", SpecType.Boolean)]
		public void CheckBinaryOperations (string op, SpecType expected)
		{
			var binary = new BinaryOperation (op, new NumberLiteral (1), new NumberLiteral (2));

			var type = binary.Walk (new TypeChecker ());

			Assert.AreEqual (expected, type.Type);
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
		[TestCase("+")]
		[TestCase("-")]
		[TestCase("*")]
		[TestCase("/")]
		public void ErrorOnStringArithmetric (string op)
		{
			var numberVal = new NumberLiteral (42);
			var stringVal = new StringLiteral ("qux");

			var left = new BinaryOperation (op, stringVal, numberVal);
			var right = new BinaryOperation (op, numberVal, stringVal);

			var leftEx = Assert.Throws<TypeCheckException> (() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException> (() => right.Walk (new TypeChecker ()));

			Assert.That (INVALID_RE.IsMatch (leftEx.Message));
			Assert.That (INVALID_RE.IsMatch (rightEx.Message));
		}

		[Test]
		[TestCase("+")]
		[TestCase("-")]
		[TestCase("*")]
		[TestCase("/")]
		public void ErrorOnNullArithmetic (string op)
		{
			var numberVal = new NumberLiteral (42);
			var nullVal = new NullLiteral ();

			var left = new BinaryOperation (op, nullVal, numberVal);
			var right = new BinaryOperation (op, numberVal, nullVal);

			var leftEx = Assert.Throws<TypeCheckException> (() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException> (() => right.Walk (new TypeChecker ()));

			Assert.That (INVALID_RE.IsMatch (leftEx.Message));
			Assert.That (INVALID_RE.IsMatch (rightEx.Message));
		}

		[Test]
		[TestCase("=")]
		[TestCase("!=")]
		public void AllowNullEqualityComparison(string op)
		{
			var nullVal = new NullLiteral ();
			var numberVal = new NumberLiteral (42);

			var left = new BinaryOperation (op, nullVal, numberVal);
			var right = new BinaryOperation (op, numberVal, nullVal);

			var leftType = left.Walk (new TypeChecker ());
			var rightType = right.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, leftType.Type);
			Assert.AreEqual (SpecType.Boolean, rightType.Type);
		}

		[Test]
		[TestCase("<")]
		[TestCase("<=")]
		[TestCase(">")]
		[TestCase(">=")]
		public void ErrorOnNullInequalityComparison(string op)
		{
			var nullVal = new NullLiteral ();
			var numberVal = new NumberLiteral (42);

			var left = new BinaryOperation (op, nullVal, numberVal);
			var right = new BinaryOperation (op, numberVal, nullVal);

			var leftEx = Assert.Throws<TypeCheckException> (() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException> (() => right.Walk (new TypeChecker ()));

			Assert.That (INVALID_RE.IsMatch (leftEx.Message));
			Assert.That (INVALID_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void CheckLogicalConjunction()
		{
			var lhs = new BinaryOperation ("=", new NumberLiteral (1), new NumberLiteral (2));
			var rhs = new BinaryOperation ("<", new NumberLiteral (3), new NumberLiteral (4));
			var conjunction = new LogicalConjunction (lhs, rhs);

			var type = conjunction.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		public void CheckLogicalDisjunction()
		{
			var lhs = new BinaryOperation ("=", new NumberLiteral (1), new NumberLiteral (2));
			var rhs = new BinaryOperation ("<", new NumberLiteral (3), new NumberLiteral (4));
			var disjunction = new LogicalDisjunction (lhs, rhs);

			var type = disjunction.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		public void ConjunctionErrorsOnNull()
		{
			var nullVal = new NullLiteral ();
			var boolVal = new BoolLiteral (true);
			var left = new LogicalConjunction (nullVal, boolVal);
			var right = new LogicalConjunction (boolVal, nullVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void DisjunctionErrorsOnNull()
		{
			var nullVal = new NullLiteral ();
			var boolVal = new BoolLiteral (true);
			var left = new LogicalDisjunction (nullVal, boolVal);
			var right = new LogicalDisjunction (boolVal, nullVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void ConjunctionErrorsOnNumbers()
		{
			var numberVal = new NumberLiteral (42);
			var boolVal = new BoolLiteral (true);
			var left = new LogicalConjunction (numberVal, boolVal);
			var right = new LogicalConjunction (boolVal, numberVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void DisjunctionErrorsOnNumbers()
		{
			var numberVal = new NumberLiteral (42);
			var boolVal = new BoolLiteral (true);
			var left = new LogicalDisjunction (numberVal, boolVal);
			var right = new LogicalDisjunction (boolVal, numberVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void ConjunctionErrorsOnStrings()
		{
			var stringVal = new StringLiteral ("foobar");
			var boolVal = new BoolLiteral (true);
			var left = new LogicalConjunction (stringVal, boolVal);
			var right = new LogicalConjunction (boolVal, stringVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		public void DisjunctionErrorsOnStrings()
		{
			var stringVal = new StringLiteral ("foobar");
			var boolVal = new BoolLiteral (true);
			var left = new LogicalDisjunction (stringVal, boolVal);
			var right = new LogicalDisjunction (boolVal, stringVal);

			var leftEx = Assert.Throws<TypeCheckException>(() => left.Walk (new TypeChecker ()));
			var rightEx = Assert.Throws<TypeCheckException>(() => right.Walk (new TypeChecker ()));

			Assert.That (INCOMPATIBLE_RE.IsMatch (leftEx.Message));
			Assert.That (INCOMPATIBLE_RE.IsMatch (rightEx.Message));
		}

		[Test]
		[TestCase(FieldType.Boolean, SpecType.Boolean)]
		[TestCase(FieldType.Number, SpecType.Number)]
		[TestCase(FieldType.String, SpecType.String)]
		[TestCase(FieldType.Date, SpecType.Date)]
		public void ReferenceTypesFromTable(FieldType fieldType, SpecType specType)
		{
			var fieldConfig = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("Test", fieldType)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable ("Foobar", null, null, fieldConfig)
			};

			var reference = new Reference (new string[]{ "Foobar", "Test" });

			var refType = reference.Walk (new TypeChecker (tables));

			Assert.AreEqual (specType, refType.Type);
		}

		[Test]
		[TestCase(FieldType.Boolean, SpecType.Boolean)]
		[TestCase(FieldType.Number, SpecType.Number)]
		[TestCase(FieldType.String, SpecType.String)]
		[TestCase(FieldType.Date, SpecType.Date)]
		public void ReferenceTypesFromParentTable(FieldType fieldType, SpecType specType)
		{
			var parentConfig = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("Test", fieldType)
			};
			var foobarConfig = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("ParentId", null)
			};
			var relationshipConfig = new List<ParentConfig>
			{
				new ParentConfig("Parent", null, null)
			};

			var tables = new List<OracularTable>
			{
				new OracularTable ("Parent", null, null, parentConfig),
				new OracularTable ("Foobar", null, relationshipConfig, foobarConfig)
			};

			var reference = new Reference (new string[]{ "Foobar", "Parent", "Test" });

			var refType = reference.Walk (new TypeChecker (tables));

			Assert.AreEqual (specType, refType.Type);
		}
	}
}

