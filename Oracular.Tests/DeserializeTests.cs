using NUnit.Framework;
using System;

using Oracular;

namespace Oracular.Tests
{
	[TestFixture]
	public class DeserializeTests
	{
		[Test]
		public void DeserializeEmptyConfig ()
		{
			var config = OracularConfig.Deserialize ("{}");

			Assert.That (config.Tables, Has.Count.EqualTo (0));
			Assert.That (config.Specs, Has.Count.EqualTo (0));
		}

		[Test]
		public void DeserializeEmptyTables ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[]}");

			Assert.That (config.Tables, Has.Count.EqualTo (0));
			Assert.That (config.Specs, Has.Count.EqualTo (0));
		}

		[Test]
		public void DeserializeEmptySpecs ()
		{
			var config = OracularConfig.Deserialize ("{\"specs\":[]}");

			Assert.That (config.Tables, Has.Count.EqualTo (0));
			Assert.That (config.Specs, Has.Count.EqualTo (0));
		}

		[Test]
		public void DeserializeSimpleTable ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[{\"table\":\"Foobar\",\"fields\":[{\"name\":\"Id\"}]}]}");

			Assert.That (config.Tables, Has.Count.EqualTo (1));
			Assert.That (config.Specs, Has.Count.EqualTo (0));

			var foobar = config.GetTable ("Foobar");

			Assert.NotNull (foobar);
			Assert.AreEqual ("Id", foobar.Id);

			Assert.That (foobar.Fields, Has.Count.EqualTo (1));

			var id = foobar.GetField ("Id");

			Assert.AreEqual (FieldType.String, id.Type);
		}

		[Test]
		public void DeserializeSimpleSpec ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[{\"table\":\"Foobar\",\"fields\":[{\"name\":\"Id\"}]}],\"specs\":[{\"name\":\"hasId\",\"table\":\"Foobar\",\"spec\":\"Foobar.Id != null\"}]}");

			Assert.That (config.Tables, Has.Count.EqualTo (1));
			Assert.That (config.Specs, Has.Count.EqualTo (1));

			var spec = config.GetSpec ("hasId");

			Assert.AreEqual ("hasId", spec.Name);
			Assert.AreEqual ("Foobar", spec.Table);
			Assert.AreEqual ("Foobar.Id != null", spec.Source);
		}

		[Test]
		public void DeserializeParentWithDefaults ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[{\"table\":\"Foo\",\"fields\":[{\"name\":\"Id\"},{\"name\":\"BarId\"}],\"parents\":[{\"name\":\"Bar\"}]},{\"table\":\"Bar\",\"fields\":[{\"name\":\"Id\"}]}]}");

			Assert.That (config.Tables, Has.Count.EqualTo (2));
			Assert.That (config.Specs, Has.Count.EqualTo (0));

			var foo = config.GetTable ("Foo");

			Assert.NotNull (foo);

			Assert.That (foo.Parents, Has.Count.EqualTo (1));

			var parent = foo.GetParent ("Bar");

			Assert.AreEqual ("Bar", parent.Table);
			Assert.AreEqual ("BarId", parent.Id);
		}

		[Test]
		public void DeserializeParentNoDefaults ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[{\"table\":\"Foo\",\"fields\":[{\"name\":\"Id\"},{\"name\":\"BarId\"}],\"parents\":[{\"name\":\"Other\",\"table\":\"Bar\",\"id\":\"BarId\"}]},{\"table\":\"Bar\",\"fields\":[{\"name\":\"Id\"}]}]}");

			Assert.That (config.Tables, Has.Count.EqualTo (2));
			Assert.That (config.Specs, Has.Count.EqualTo (0));

			var foo = config.GetTable ("Foo");

			Assert.NotNull (foo);

			Assert.That (foo.Parents, Has.Count.EqualTo (1));

			var parent = foo.GetParent ("Other");

			Assert.AreEqual ("Bar", parent.Table);
			Assert.AreEqual ("BarId", parent.Id);
		}

		[Test]
		public void DeserializeIdField ()
		{
			var config = OracularConfig.Deserialize ("{\"tables\":[{\"table\":\"Foobar\",\"id\":\"MyId\",\"fields\":[{\"name\":\"MyId\"}]}]}");

			Assert.That (config.Tables, Has.Count.EqualTo (1));
			Assert.That (config.Specs, Has.Count.EqualTo (0));

			var foobar = config.GetTable ("Foobar");

			Assert.NotNull (foobar);
			Assert.AreEqual ("MyId", foobar.Id);
		}
	}
}

