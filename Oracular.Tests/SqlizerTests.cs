﻿using NUnit.Framework;
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
		[Test]
		public void SerializeNull ()
		{
			var nullNode = new NullLiteral ();

			var nullSql = nullNode.Walk (new Sqlizer ());

			Assert.AreEqual ("NULL", nullSql);
		}

		[Test]
		public void SerializeTrue ()
		{
			var trueNode = new BooleanLiteral (true);

			var trueSql = trueNode.Walk (new Sqlizer ());

			Assert.AreEqual ("TRUE", trueSql);
		}

		[Test]
		public void SerializeFalse ()
		{
			var falseNode = new BooleanLiteral (false);

			var falseSql = falseNode.Walk (new Sqlizer ());

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

			var numberSql = numberNode.Walk (new Sqlizer ());

			Assert.AreEqual (expected, numberSql);
		}

		[Test]
		[TestCase("foobar", "'foobar'")]
		[TestCase("''", "'\\'\\''")]
		[TestCase("\\", "'\\\\'")]
		public void SerializeString (string value, string expected)
		{
			var stringNode = new StringLiteral (value);

			var stringSql = stringNode.Walk (new Sqlizer ());

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

			var binarySql = binaryNode.Walk (new Sqlizer ());

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

			var sql = conjunction.Walk (new Sqlizer ());

			Assert.AreEqual ("(TRUE AND FALSE)", sql);
		}

		[Test]
		public void SerializeLogicalDisjunction ()
		{
			var disjunction = new LogicalDisjunction(
				new BooleanLiteral(true),
				new BooleanLiteral(false)
			);

			var sql = disjunction.Walk (new Sqlizer ());

			Assert.AreEqual ("(TRUE OR FALSE)", sql);
		}

		[Test]
		public void SerializeTableName ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar" });

			var sql = foobarReference.Walk (new Sqlizer (config));

			Assert.AreEqual ("[Foobar]", sql);
		}

		[Test]
		public void SerializeTableAsAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar" });

			var sql = foobarReference.Walk (new Sqlizer (config, "Alias"));

			Assert.AreEqual ("[Alias]", sql);
		}

		[Test]
		public void SerializeFieldName ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar", "Id" });

			var sql = foobarReference.Walk (new Sqlizer (config));

			Assert.AreEqual ("[Foobar].[Id]", sql);
		}

		[Test]
		public void SerializeFieldOnAlias ()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var tables = new List<OracularTable>
			{
				new OracularTable("Foobar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foobar", "Id" });

			var sql = foobarReference.Walk (new Sqlizer (config, "Alias"));

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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar" });

			var builder = new Sqlizer (config);
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar" });

			var builder = new Sqlizer (config, "Alias");
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Id" });

			var builder = new Sqlizer (config);
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Id" });

			var builder = new Sqlizer (config, "Alias");
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz" });

			var builder = new Sqlizer (config);
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz" });

			var builder = new Sqlizer (config, "Alias");
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz", "Id" });

			var builder = new Sqlizer (config);
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
			var tables = new List<OracularTable>
			{
				new OracularTable("Foo", null, barRelationship, idAndBarId),
				new OracularTable("Bar", null, bazRelationship, idAndBazId),
				new OracularTable("Baz", null, null, justAnId)
			};
			var specs = new List<OracularSpec> ();
			var config = new OracularConfig (tables, specs);

			var foobarReference = new Reference (new [] { "Foo", "Bar", "Baz", "Id" });

			var builder = new Sqlizer (config, "Alias");
			var sql = foobarReference.Walk (builder);

			Assert.AreEqual ("[Alias.Bar.Baz].[Id]", sql);

			Assert.AreEqual (2, builder.JoinTables.Count ());
		}
	}
}
