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
	}
}

