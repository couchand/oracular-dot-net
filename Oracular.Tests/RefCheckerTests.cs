using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular.Tests
{
	[TestFixture]
	public class RefCheckerTests
	{
		[Test]
		public void CheckNullDoesNothing ()
		{
			var nullNode = new NullLiteral ();

			var result = nullNode.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckBooleanDoesNothing ()
		{
			var boolean = new BooleanLiteral (true);

			var result = boolean.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckNumberDoesNothing ()
		{
			var number = new NumberLiteral (42);

			var result = number.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckStringDoesNothing ()
		{
			var str = new StringLiteral ("baz");

			var result = str.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckOperatorDoesNothing ()
		{
			var str = new StringLiteral ("baz");
			var nullNode = new NullLiteral ();
			var operation = new BinaryOperation ("=", str, nullNode);

			var result = operation.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckConjunctionDoesNothing ()
		{
			var yes = new BooleanLiteral (true);
			var no = new BooleanLiteral (false);
			var conjunction = new LogicalConjunction (yes, no);

			var result = conjunction.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckDisjunctionDoesNothing ()
		{
			var yes = new BooleanLiteral (true);
			var no = new BooleanLiteral (false);
			var disjunction = new LogicalDisjunction (yes, no);

			var result = disjunction.Walk (new RefChecker (), new [] { "Foobar" });

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		private static readonly Regex JOIN_RE = new Regex("join", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[Test]
		public void ExpectReferenceRootToBeInTables()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Other", null, null, justAnId),
				new OracularTable("Tables", null, null, justAnId)
			};
			var config = new OracularConfig (tables, new List<OracularSpec> ());
			var checker = new RefChecker (config);

			var reference = new Reference (new []{ "Foobar" });

			var initial = new []{ "Other", "Tables" };

			var ex = Assert.Throws<RefCheckException> (() => reference.Walk (checker, initial));

			Assert.That (JOIN_RE.IsMatch (ex.Message));
		}

		[Test]
		public void CheckReferenceRoot()
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
			var checker = new RefChecker (config);

			var reference = new Reference (new []{ "Foobar" });

			var initial = new []{ "Foobar" };

			var result = reference.Walk (checker, initial);

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		[Test]
		public void CheckSpecReferences()
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
				new OracularSpec("isBaz", "Foobar", "Foobar.Id != null")
			};
			var config = new OracularConfig (tables, specs);
			var checker = new RefChecker (config);

			var reference = new Reference (new [] { "Foobar" });
			var fn = new Reference (new [] { "isBaz" });
			var call = new FunctionCall (fn, new [] { reference });

			var initial = new []{ "Foobar" };

			var result = call.Walk (checker, initial);

			Assert.AreEqual (1, result.Length);
			Assert.Contains ("Foobar", result);
		}

		private static readonly Regex REFERENCE_RE = new Regex("reference", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
			var specs = new List<OracularSpec>();
			var config = new OracularConfig (tables, specs);
			var checker = new RefChecker (config);

			var reference = new Reference (new [] { "Foobar" });
			var fn = new Reference (new [] { "isBaz" });
			var call = new FunctionCall (fn, new [] { reference });

			var initial = new []{ "Foobar" };

			var ex = Assert.Throws<RefCheckException>(() => call.Walk (checker, initial));

			Assert.That (REFERENCE_RE.IsMatch (ex.Message));
		}

		[Test]
		public void ExpectJoinTableToExist()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec>();
			var config = new OracularConfig (tables, specs);
			var checker = new RefChecker (config);

			var fingerlingPotatoes = new BinaryOperation("=",
				new Reference(new [] { "Potato", "Type" }),
				new StringLiteral("Fingerling")
			);

			var call = new FunctionCall(
				new Reference(new [] { "ANY" }),
				new [] { fingerlingPotatoes }
			);

			var initial = new []{ "Foobar" };

			var ex = Assert.Throws<RefCheckException> (() => call.Walk (checker, initial));

			Assert.That (REFERENCE_RE.IsMatch (ex.Message));
		}

		[Test]
		public void CheckJoinTables()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var potatoFields = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("FoobarId", null),
				new FieldConfig("Type", null)
			};
			var foobarRelationship = new ParentConfig ("Foobar", null, null);
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId),
				new OracularTable("Potato", null, new List<ParentConfig>{foobarRelationship}, potatoFields)
			};
			var specs = new List<OracularSpec>();
			var config = new OracularConfig (tables, specs);
			var checker = new RefChecker (config);

			var fingerlingPotatoes = new BinaryOperation("=",
				new Reference(new [] { "Potato", "Type" }),
				new StringLiteral("Fingerling")
			);

			var call = new FunctionCall(
				new Reference(new [] { "ANY" }),
				new AstNode[] {
					new Reference(new [] { "Potato" }),
					fingerlingPotatoes
				}
			);

			var initial = new []{ "Foobar" };

			var result = call.Walk (checker, initial);

			Assert.AreEqual (2, result.Length);
			Assert.Contains ("Foobar", result);
			Assert.Contains ("Potato", result);
		}
	}
}

