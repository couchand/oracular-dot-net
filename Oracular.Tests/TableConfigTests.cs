using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using Oracular;

namespace Oracular.Tests
{
	[TestFixture]
	public class TableConfigTests
	{
		[Test]
		public void ExpectTableName ()
		{
			var ex = Assert.Throws<OracularException> (() => new OracularTable (null, null, null, null));

			Assert.That (ex.Message, Is.StringContaining ("name"));
		}

		[Test]
		public void ExpectOneField ()
		{
			var ex = Assert.Throws<OracularException> (() => new OracularTable ("foobar", null, null, null));

			Assert.That (ex.Message, Is.StringContaining ("field"));
		}

		[Test]
		public void ExpectIdToMatch ()
		{
			var notAnId = new List<FieldConfig> { new FieldConfig("name", null) };
			var ex = Assert.Throws<OracularException> (() => new OracularTable ("foobar", null, null, notAnId));

			Assert.That (ex.Message, Is.StringContaining ("id"));
		}

		[Test]
		public void CreateATable()
		{
			var justAnId = new List<FieldConfig> { new FieldConfig("Id", null) };
			var table = new OracularTable ("foobar", null, null, justAnId);

			Assert.AreEqual ("foobar", table.Table);

			var fields = table.Fields.ToArray ();

			Assert.That (fields, Has.Length.EqualTo (1));
			Assert.AreEqual ("Id", fields[0].Name);
		}

		[Test]
		public void AllowIdConfig()
		{
			var justAnId = new List<FieldConfig> { new FieldConfig("customId", null) };
			var table = new OracularTable ("foobar", "customId", null, justAnId);

			Assert.AreEqual ("foobar", table.Table);

			var fields = table.Fields.ToArray ();

			Assert.That (fields, Has.Length.EqualTo (1));
			Assert.AreEqual ("customId", fields[0].Name);
		}

		[Test]
		public void DisallowDuplicateNames()
		{
			var twoIds = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("Id", null)
			};

			var ex = Assert.Throws<OracularException> (() => new OracularTable ("foobar", null, null, twoIds));

			Assert.That(ex.Message, Is.StringContaining("duplicate"));
		}

		[Test]
		[TestCase(FieldType.Boolean)]
		[TestCase(FieldType.Number)]
		[TestCase(FieldType.String)]
		[TestCase(FieldType.Date)]
		public void SelectFieldType(FieldType type)
		{
			var idAndTestField = new List<FieldConfig> {
				new FieldConfig("Id", null),
				new FieldConfig("Test", type)
			};
			var table = new OracularTable ("foobar", null, null, idAndTestField);

			FieldConfig testField = null;
			foreach (var field in table.Fields)
			{
				if (field.Name == "Test")
				{
					testField = field;
					break;
				}
			}
			Assert.NotNull (testField);
			Assert.AreEqual (type, testField.Type);
		}

		[Test]
		public void DefaultTypeToString()
		{
			var idAndTestField = new List<FieldConfig> {
				new FieldConfig("Id", null),
				new FieldConfig("Test", null)
			};
			var table = new OracularTable ("foobar", null, null, idAndTestField);

			FieldConfig testField = null;
			foreach (var field in table.Fields)
			{
				if (field.Name == "Test")
				{
					testField = field;
					break;
				}
			}
			Assert.NotNull (testField);
			Assert.AreEqual (FieldType.String, testField.Type);
		}

		[Test]
		public void CreateParentRelationship()
		{
			var idAndOwner = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("OwnerId", null)
			};
			var ownerRelationship = new List<ParentConfig>
			{
				new ParentConfig("Owner", null, null)
			};
			var table = new OracularTable ("Account", null, ownerRelationship, idAndOwner);

			var parents = table.Parents.ToArray ();

			Assert.That (parents, Has.Length.EqualTo (1));
			Assert.AreEqual ("Owner", parents [0].Name);
		}

		[Test]
		public void DefaultParentFields()
		{
			var idAndAccountId = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("AccountId", null)
			};
			var ownerRelationship = new List<ParentConfig>
			{
				new ParentConfig("Account", null, null)
			};
			var table = new OracularTable ("Order", null, ownerRelationship, idAndAccountId);

			var parents = table.Parents.ToArray ();

			Assert.That (parents, Has.Length.EqualTo (1));

			var account = parents [0];
			Assert.AreEqual ("Account", account.Name);
			Assert.AreEqual ("AccountId", account.Id);
			Assert.AreEqual ("Account", account.Table);
		}

		[Test]
		public void ExpectParentIdField()
		{
			var justAnId = new List<FieldConfig>
			{
				new FieldConfig("Id", null)
			};
			var ownerRelationship = new List<ParentConfig>
			{
				new ParentConfig("Account", null, null)
			};

			var ex = Assert.Throws<OracularException> (() => new OracularTable ("foobar", null, ownerRelationship, justAnId));

			Assert.That (ex.Message, Is.StringContaining ("parent"));
		}

		[Test]
		public void DisallowParentNameOverwrite()
		{
			var fields = new List<FieldConfig>
			{
				new FieldConfig("Id", null),
				new FieldConfig("Account", null),
				new FieldConfig("AccountId", null)
			};
			var ownerRelationship = new List<ParentConfig>
			{
				new ParentConfig("Account", null, null)
			};

			var ex = Assert.Throws<OracularException> (() => new OracularTable ("foobar", null, ownerRelationship, fields));

			Assert.That (ex.Message, Is.StringContaining ("parent"));
		}
	}
}

