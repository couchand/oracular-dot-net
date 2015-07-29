using NUnit.Framework;
using System;

using Oracular;
using Oracular.Spec.Ast;

namespace Oracular.Tests
{
	[TestFixture]
	public class SpecConfigTests
	{
		[Test]
		public void ExpectsName ()
		{
			var ex = Assert.Throws<OracularException> (() => new OracularSpec (null, null, null));

			Assert.That (ex.Message, Is.StringContaining ("name"));
		}

		[Test]
		public void ExpectsTable()
		{
			var ex = Assert.Throws<OracularException> (() => new OracularSpec ("Foobar", null, null));

			Assert.That (ex.Message, Is.StringContaining ("table"));
		}

		[Test]
		public void ExpectsSpec()
		{
			var ex = Assert.Throws<OracularException> (() => new OracularSpec ("Foobar", "Baz", null));

			Assert.That (ex.Message, Is.StringContaining ("spec"));
		}

		[Test]
		[TestCase("tautology", "User", "true")]
		[TestCase("isManager", "User", "User.Type = 'Manager'")]
		[TestCase("isCustomer", "Account", "Account.Type = \"Customer\"")]
		[TestCase("customerWithManagerOwner", "Account", "isCustomer(Account) AND isManager(Account.Owner)")]
		[TestCase("managerWithCustomers", "User", "isManager(User) AND ANY(isCustomer(Account))")]
		public void ParseSpec(string name, string table, string source)
		{
			var spec = new OracularSpec (name, table, source);

			Assert.AreEqual (name, spec.Name);
			Assert.AreEqual (table, spec.Table);
			Assert.AreEqual (source, spec.Source);

			Assert.AreNotEqual (0, spec.Spec.Id);
		}
	}
}

