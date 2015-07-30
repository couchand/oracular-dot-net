using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

using Oracular;

namespace Oracular.Tests
{
	[TestFixture]
	public class IntegrationTests
	{
		private static readonly string CRM_CONFIG = @"
{
	""tables"": [
		{
			""table"": ""Account"",
			""fields"": [
				{
					""name"": ""Id""
				},
				{
					""name"": ""OwnerId""
				}
			],
			""parents"": [
				{
					""name"": ""Owner"",
					""table"": ""User""
				}
			]
		},
		{
			""table"": ""User"",
			""fields"": [
				{
					""name"": ""Id""
				},
				{
					""name"": ""Name""
				},
				{
					""name"": ""Type""
				}
			]
		}
	],
	""specs"": [
		{
			""name"": ""isManager"",
			""table"": ""User"",
			""spec"": ""User.Type = 'Manager'""
		},
		{
			""name"": ""ownerIsManager1"",
			""table"": ""Account"",
			""spec"": ""Account.Owner.Type = 'Manager'""
		},
		{
			""name"": ""ownerIsManager2"",
			""table"": ""Account"",
			""spec"": ""isManager(Account.Owner)""
		},
		{
			""name"": ""hasManagerOwnedAccount1"",
			""table"": ""User"",
			""spec"": ""ANY(Account, Account.Owner.Type = 'Manager')""
		},
		{
			""name"": ""hasManagerOwnedAccount2"",
			""table"": ""User"",
			""spec"": ""ANY(Account, isManager(Account.Owner))""
		},
		{
			""name"": ""hasManagerOwnedAccount3"",
			""table"": ""User"",
			""spec"": ""ANY(Account, ownerIsManager1(Account))""
		},
		{
			""name"": ""hasManagerOwnedAccount4"",
			""table"": ""User"",
			""spec"": ""ANY(Account, ownerIsManager2(Account))""
		}
	]
}";
		[Test]
		public void TestBasicUsage ()
		{
			var parsed = OracularConfig.Deserialize (CRM_CONFIG);
			parsed.Check ();

			var spec = parsed.GetSpec ("isManager");

			var sql = spec.ToSql ();

			Console.WriteLine (sql);

			Assert.NotNull (sql);
		}

		[Test]
		public void TestSpecReferences ()
		{
			var parsed = OracularConfig.Deserialize (CRM_CONFIG);
			parsed.Check ();

			var one = parsed.GetSpec ("ownerIsManager1");
			var two = parsed.GetSpec ("ownerIsManager2");

			var oneSql = one.ToSql ();
			var twoSql = two.ToSql ();

			Console.WriteLine (oneSql);
			Console.WriteLine (twoSql);

			Assert.AreEqual(oneSql, twoSql);
		}

		private static readonly Regex GENERATED_ID_RE = new Regex("\\[(Annotated|Any)[^\\]]+\\d+\\]", RegexOptions.Compiled);

		[Test]
		public void TestJoinMacros ()
		{
			var parsed = OracularConfig.Deserialize (CRM_CONFIG);
			parsed.Check ();

			var one = parsed.GetSpec ("hasManagerOwnedAccount1");
			var two = parsed.GetSpec ("hasManagerOwnedAccount2");
			var three = parsed.GetSpec ("hasManagerOwnedAccount3");
			var four = parsed.GetSpec ("hasManagerOwnedAccount4");

			var oneSql = one.ToSql ();
			var twoSql = two.ToSql ();
			var threeSql = three.ToSql ();
			var fourSql = four.ToSql ();

			Console.WriteLine (oneSql);
			Console.WriteLine (twoSql);
			Console.WriteLine (threeSql);
			Console.WriteLine (fourSql);

			oneSql = GENERATED_ID_RE.Replace (oneSql, "");
			twoSql = GENERATED_ID_RE.Replace (twoSql, "");
			threeSql = GENERATED_ID_RE.Replace (threeSql, "");
			fourSql = GENERATED_ID_RE.Replace (fourSql, "");

			Assert.AreEqual(oneSql, twoSql);
			Assert.AreEqual(oneSql, threeSql);
			Assert.AreEqual(threeSql, fourSql);
		}
	}
}

