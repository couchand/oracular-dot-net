using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular.Tests
{
	[TestFixture]
	public class SqlizerTests
	{
		private OracularTable foobarTable()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			return new OracularTable ("Foobar", null, null, justAnId);
		}

		[Test]
		public void SerializeNull ()
		{
			var nullNode = new NullLiteral ();

			var nullSql = nullNode.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual ("NULL", nullSql);
		}

		[Test]
		public void SerializeTrue ()
		{
			var trueNode = new BooleanLiteral (true);

			var trueSql = trueNode.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual ("TRUE", trueSql);
		}

		[Test]
		public void SerializeFalse ()
		{
			var falseNode = new BooleanLiteral (false);

			var falseSql = falseNode.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual ("FALSE", falseSql);
		}

		[Test]
		[TestCase(42, "42")]
		[TestCase( 0,  "0")]
		[TestCase( 1,  "1")]
		[TestCase(-1, "-1")]
		[TestCase(4.2, "4.2")]
		public void SerializeNumber (double number, string expected)
		{
			var numberNode = new NumberLiteral (number);

			var numberSql = numberNode.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual (expected, numberSql);
		}

		[Test]
		[TestCase("foobar", "'foobar'")]
		[TestCase("''", "'\\'\\''")]
		[TestCase("\\", "'\\\\'")]
		public void SerializeString (string value, string expected)
		{
			var stringNode = new StringLiteral (value);

			var stringSql = stringNode.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual (expected, stringSql);
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
		public void SerializeBinaryOperation (string op)
		{
			var binaryNode = new BinaryOperation (op,
				new NumberLiteral (1),
				new NumberLiteral (2)
			);

			var binarySql = binaryNode.Walk (new Sqlizer (foobarTable()));

			var expected = String.Format ("(1 {0} 2)", op);
			Assert.AreEqual (expected, binarySql);
		}

		[Test]
		public void SerializeLogicalConjunction ()
		{
			var conjunction = new LogicalConjunction(
				new BooleanLiteral(true),
				new BooleanLiteral(false)
			);

			var sql = conjunction.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual ("(TRUE AND FALSE)", sql);
		}

		[Test]
		public void SerializeLogicalDisjunction ()
		{
			var disjunction = new LogicalDisjunction(
				new BooleanLiteral(true),
				new BooleanLiteral(false)
			);

			var sql = disjunction.Walk (new Sqlizer (foobarTable()));

			Assert.AreEqual ("(TRUE OR FALSE)", sql);
		}

		[Test]
		public void SerializeTableName ()
		{
			var foobar = foobarTable ();
			var tables = new List<OracularTable>
			{
				foobar
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar" });

			var sql = foobarReference.Walk (new Sqlizer (foobar, config));

			Assert.AreEqual ("[Foobar]", sql);
		}

		[Test]
		public void SerializeTableAsAlias ()
		{
			var foobar = foobarTable ();
			var tables = new List<OracularTable>
			{
				foobar
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar" });

			var sql = foobarReference.Walk (new Sqlizer (foobar, config, "Alias"));

			Assert.AreEqual ("[Alias]", sql);
		}

		[Test]
		public void SerializeFieldName ()
		{
			var foobar = foobarTable ();
			var tables = new List<OracularTable>
			{
				foobar
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar", "Id" });

			var sql = foobarReference.Walk (new Sqlizer (foobar, config));

			Assert.AreEqual ("[Foobar].[Id]", sql);
		}

		[Test]
		public void SerializeBooleanField ()
		{
			var idAndBoolean = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("IsBaz", FieldType.Boolean)
			};
			var foobar = new OracularTable ("Foobar", null, null, idAndBoolean);
			var tables = new List<OracularTable>
			{
				foobar
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar", "IsBaz" });

			var sql = foobarReference.Walk (new Sqlizer (foobar, config));

			Assert.AreEqual ("[Foobar].[IsBaz] = 1", sql);
		}

		[Test]
		public void SerializeFieldOnAlias ()
		{
			var foobar = foobarTable ();
			var tables = new List<OracularTable>
			{
				foobar
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar", "Id" });

			var sql = foobarReference.Walk (new Sqlizer (foobar, config, "Alias"));

			Assert.AreEqual ("[Alias].[Id]", sql);
		}

		[Test]
		public void SerializeParentTable ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar" });

			var builder = new Sqlizer (foo, config);
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Foo.Bar]", sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var joinClause = builder.JoinTables.First();

			Assert.AreEqual ("INNER JOIN [Bar] [Foo.Bar] ON [Foo.Bar].[Id] = [Foo].[BarId]", joinClause);
		}

		[Test]
		public void SerializeParentOnAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar" });

			var builder = new Sqlizer (foo, config, "Alias");
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Alias.Bar]", sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var joinClause = builder.JoinTables.First();

			Assert.AreEqual ("INNER JOIN [Bar] [Alias.Bar] ON [Alias.Bar].[Id] = [Alias].[BarId]", joinClause);
		}

		[Test]
		public void SerializeParentField ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Id" });

			var builder = new Sqlizer (foo, config);
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Foo.Bar].[Id]", sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var joinClause = builder.JoinTables.First();

			Assert.AreEqual ("INNER JOIN [Bar] [Foo.Bar] ON [Foo.Bar].[Id] = [Foo].[BarId]", joinClause);
		}

		[Test]
		public void SerializeParentFieldOnAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Id" });

			var builder = new Sqlizer (foo, config, "Alias");
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Alias.Bar].[Id]", sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var joinClause = builder.JoinTables.First();

			Assert.AreEqual ("INNER JOIN [Bar] [Alias.Bar] ON [Alias.Bar].[Id] = [Alias].[BarId]", joinClause);
		}

		[Test]
		public void SerializeGrandparentTable ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBazId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BazId", null)
			};
			var bazRelationship = new List<ParentConfig>
			{
				new ParentConfig("Baz", null, null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz" });

			var builder = new Sqlizer (foo, config);
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Foo.Bar.Baz]", sql);

			Assert.AreEqual (2, builder.JoinTables.Count ());
		}

		[Test]
		public void SerializeGrandparentTableOnAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBazId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BazId", null)
			};
			var bazRelationship = new List<ParentConfig>
			{
				new ParentConfig("Baz", null, null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz" });

			var builder = new Sqlizer (foo, config, "Alias");
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Alias.Bar.Baz]", sql);

			Assert.AreEqual (2, builder.JoinTables.Count ());
		}

		[Test]
		public void SerializeGrandparentField ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBazId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BazId", null)
			};
			var bazRelationship = new List<ParentConfig>
			{
				new ParentConfig("Baz", null, null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz", "Id" });

			var builder = new Sqlizer (foo, config);
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Foo.Bar.Baz].[Id]", sql);

			Assert.AreEqual (2, builder.JoinTables.Count ());
		}

		[Test]
		public void SerializeGrandparentFieldOnAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBazId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BazId", null)
			};
			var bazRelationship = new List<ParentConfig>
			{
				new ParentConfig("Baz", null, null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz", "Id" });

			var builder = new Sqlizer (foo, config, "Alias");
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Alias.Bar.Baz].[Id]", sql);

			Assert.AreEqual (2, builder.JoinTables.Count ());
		}

		[Test]
		public void SerializeSpecReferences ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var tables = new List<OracularTable>
			{
				foo,
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec>
			{
				new OracularSpec("barHasId", "Bar", "Bar.Id != null")
			};
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar" });
			var specReference = new Reference (new [] { "barHasId" });
			var macroExpansion = new MacroExpansion (specReference, new [] { foobarReference });

			var builder = new Sqlizer (foo, config);
			var sql = macroExpansion.Walk (builder);

			Assert.AreEqual ("([Foo.Bar].[Id] != NULL)", sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var joinClause = builder.JoinTables.First();

			Assert.AreEqual ("INNER JOIN [Bar] [Foo.Bar] ON [Foo.Bar].[Id] = [Foo].[BarId]", joinClause);
		}

		[Test]
		[TestCase("ANY", "Any", " = 1", "1", "", "")]
		[TestCase("NONE", "No", " != 1", "1", "", "")]
		[TestCase("ALL", "AnyNot", " != 1", "1", "NOT(", ")")]
		public void SerializeReducer(string macro, string prefix, string suffix, string value, string inverter1, string inverter2)
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var idAndBarId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("BarId", null)
			};
			var barRelationship = new List<ParentConfig>
			{
				new ParentConfig("Bar", null, null)
			};
			var foo = new OracularTable ("Foo", null, barRelationship, idAndBarId);
			var bar = new OracularTable("Bar", null, null, justAnId);
			var tables = new List<OracularTable>{ foo, bar };
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var macroReference = new Reference (new[]{ macro });
			var fooReference = new Reference (new[]{ "Foo" });
			var idNotNull = new BinaryOperation ("!=",
				new Reference (new[]{ "Foo", "Id" }),
				new NullLiteral ()
			);

			var macroExpansion = new MacroExpansion (macroReference, new AstNode[] { fooReference, idNotNull });

			var builder = new Sqlizer (bar, config);
			var sql = macroExpansion.Walk (builder);

			var expected = String.Format ("[AnnotatedBar{0}].[{1}Foo{0}]{2}", idNotNull.Id, prefix, suffix);
			Assert.AreEqual (expected, sql);

			Assert.AreEqual (1, builder.JoinTables.Count ());
			var join = builder.JoinTables.First ();

			expected = String.Format ("LEFT JOIN [AnnotatedBar{0}] ON [AnnotatedBar{0}].[Id] = [Bar].[Id]", idNotNull.Id);
			Assert.AreEqual (expected, join);

			Assert.AreEqual (1, builder.CommonTableExpressions.Count ());
			var annotated = builder.CommonTableExpressions.First ();

			expected = String.Format (@"[AnnotatedBar{0}] AS (
SELECT DISTINCT [Bar].[Id], {2} [{1}Foo{0}]
FROM [Bar]
LEFT JOIN [Foo] ON [Foo].[BarId] = [Bar].[Id]
WHERE {3}([Foo].[Id] != NULL){4}
)", idNotNull.Id, prefix, value, inverter1, inverter2);
			Assert.AreEqual (expected, annotated);
		}
	}
}

