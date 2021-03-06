﻿using NUnit.Framework;

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

			Assert.AreEqual (SpecType.Any, type.Type);
		}

		[Test]
		public void CheckBooleanLiterals ()
		{
			var booleanNode = new BooleanLiteral (false);

			var type = booleanNode.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		public void CheckNumberLiterals ()
		{
			var numberNode = new NumberLiteral (42);

			var type = numberNode.Walk (new TypeChecker());

			Assert.AreEqual (SpecType.Number, type.Type);
		}

		[Test]
		public void CheckStringLiterals ()
		{
			var stringNode = new StringLiteral ("foobar");

			var type = stringNode.Walk (new TypeChecker());

			Assert.AreEqual (SpecType.String, type.Type);
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
		[TestCase("+")]
		[TestCase("-")]
		[TestCase("*")]
		[TestCase("/")]
		public void ErrorOnBooleanArithmetic (string op)
		{
			var numberVal = new NumberLiteral (42);
			var boolVal = new BooleanLiteral (false);

			var left = new BinaryOperation (op, boolVal, numberVal);
			var right = new BinaryOperation (op, numberVal, boolVal);

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
		[TestCase("=")]
		[TestCase("!=")]
		[TestCase("<")]
		[TestCase("<=")]
		[TestCase(">")]
		[TestCase(">=")]
		public void AllowStringEqualityAndInequalityComparison(string op)
		{
			var left = new StringLiteral ("foobar");
			var right = new StringLiteral ("baz");
			var compared = new BinaryOperation (op, left, right);

			var type = compared.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		[TestCase("=")]
		[TestCase("!=")]
		public void AllowBooleanEqualityComparison(string op)
		{
			var left = new BooleanLiteral (true);
			var right = new BooleanLiteral (false);
			var compared = new BinaryOperation (op, left, right);

			var type = compared.Walk (new TypeChecker ());

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		[TestCase("<")]
		[TestCase("<=")]
		[TestCase(">")]
		[TestCase(">=")]
		public void ErrorOnBooleanInequalityComparison (string op)
		{
			var left = new BooleanLiteral (true);
			var right = new BooleanLiteral (false);
			var compared = new BinaryOperation (op, left, right);

			var ex = Assert.Throws<TypeCheckException> (() => compared.Walk (new TypeChecker ()));

			Assert.That (INVALID_RE.IsMatch (ex.Message));
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
			var boolVal = new BooleanLiteral (true);
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
			var boolVal = new BooleanLiteral (true);
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
			var boolVal = new BooleanLiteral (true);
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
			var boolVal = new BooleanLiteral (true);
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
			var boolVal = new BooleanLiteral (true);
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
			var boolVal = new BooleanLiteral (true);
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
			var config = new OracularConfig (tables, new List<OracularSpec> ());

			var reference = new Reference (new string[]{ "Foobar", "Test" });

			var refType = reference.Walk (new TypeChecker (config));

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
			var config = new OracularConfig (tables, new List<OracularSpec> ());

			var reference = new Reference (new string[]{ "Foobar", "Parent", "Test" });

			var refType = reference.Walk (new TypeChecker (config));

			Assert.AreEqual (specType, refType.Type);
		}

		[Test]
		public void ExpectTableToExist()
		{
			var reference = new Reference (new string[]{ "Foobar", "Test" });

			var ex = Assert.Throws<TypeCheckException> (() => reference.Walk (new TypeChecker ()));

			Assert.That (ex.Message, Is.StringContaining ("name"));
		}

		[Test]
		public void ExpectFieldToExist()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var config = new OracularConfig (tables, new List<OracularSpec> ());
			var reference = new Reference (new string[]{ "Foobar", "Test" });

			var ex = Assert.Throws<TypeCheckException> (() => reference.Walk (new TypeChecker (config)));

			Assert.That (ex.Message, Is.StringContaining ("field"));
		}

		[Test]
		public void ExpectParentToExist()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var config = new OracularConfig (tables, new List<OracularSpec> ());
			var reference = new Reference (new string[]{ "Foobar", "Parent", "Test" });

			var ex = Assert.Throws<TypeCheckException> (() => reference.Walk (new TypeChecker (config)));

			Assert.That (ex.Message, Is.StringContaining ("parent"));
		}

		[Test]
		public void ExpectParentFieldToExist()
		{
			var parentConfig = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
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
			var config = new OracularConfig (tables, new List<OracularSpec> ());

			var reference = new Reference (new string[]{ "Foobar", "Parent", "Test" });

			var ex = Assert.Throws<TypeCheckException> (() => reference.Walk (new TypeChecker (config)));

			Assert.That (ex.Message, Is.StringContaining ("field"));
		}

		[Test]
		public void ExpectSpecToExist()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var config = new OracularConfig (tables, new List<OracularSpec> ());

			var fnRef = new Reference (new string[]{ "isBaz" });
			var foobarRef = new Reference (new string[]{ "Foobar" });
			var isFoobarBaz = new MacroExpansion (fnRef, new AstNode[]{ foobarRef });

			var ex = Assert.Throws<TypeCheckException> (() => isFoobarBaz.Walk (new TypeChecker (config)));

			Assert.That (ex.Message, Is.StringContaining ("name"));
		}

		[Test]
		public void ExpectSpecTableToMatchInput()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId),
				new OracularTable("SomethingElse", null, null, justAnId)
			};
			var specs = new List<OracularSpec>
			{
				new OracularSpec("isBaz", "SomethingElse", "SomethingElse.Id = null")
			};
			var config = new OracularConfig (tables, specs);

			var fnRef = new Reference (new string[]{ "isBaz" });
			var foobarRef = new Reference (new string[]{ "Foobar" });
			var isFoobarBaz = new MacroExpansion (fnRef, new AstNode[]{ foobarRef });

			var ex = Assert.Throws<TypeCheckException> (() => isFoobarBaz.Walk (new TypeChecker (config)));

			Assert.That (ex.Message, Is.StringContaining ("mismatch"));
		}

		[Test]
		public void CheckSpecAgainstTable()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec>
			{
				new OracularSpec("isBaz", "Foobar", "Foobar.Id = null")
			};
			var config = new OracularConfig (tables, specs);

			var fnRef = new Reference (new string[]{ "isBaz" });
			var foobarRef = new Reference (new string[]{ "Foobar" });
			var isFoobarBaz = new MacroExpansion (fnRef, new AstNode[]{ foobarRef });

			var type = isFoobarBaz.Walk (new TypeChecker (config));

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}

		[Test]
		public void CheckSpecAgainstParentTable()
		{
			var parentConfig = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("IsBaz", FieldType.Boolean)
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

			var isParentBaz = new OracularSpec ("isParentBaz", "Foobar", "isBaz(Foobar.Parent)");

			var specs = new List<OracularSpec>
			{
				new OracularSpec("isBaz", "Parent", "Parent.IsBaz"),
				isParentBaz
			};
			var config = new OracularConfig (tables, specs);

			var type = isParentBaz.Spec.Walk (new TypeChecker (config));

			Assert.AreEqual (SpecType.Boolean, type.Type);
		}
	}
}

