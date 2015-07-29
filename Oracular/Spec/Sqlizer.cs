using System;
using System.Collections.Generic;
using System.Linq;

namespace Oracular.Spec
{
	public class Sqlizer : IPostorderWalker<string>
	{
		readonly OracularConfig config;
		readonly string rootAlias;
		readonly Dictionary<string, string> joinTables;

		public Sqlizer()
			: this(new OracularConfig (new List<OracularTable> (), new List<OracularSpec> ())) {}

		public Sqlizer(OracularConfig config)
			:this(config, null) {}

		public Sqlizer(OracularConfig config, string alias)
		{
			this.config = config;
			this.rootAlias = alias;

			this.joinTables = new Dictionary<string, string> ();
		}

		public string WalkNullLiteral()
		{
			return "NULL";
		}

		public string WalkBooleanLiteral(bool value)
		{
			return value ? "TRUE" : "FALSE";
		}

		public string WalkNumberLiteral(double value)
		{
			return value.ToString();
		}

		public string WalkStringLiteral(string value)
		{
			var escapedBackslashes = value.Replace ("\\", "\\\\");
			var escapedSingleQuotes = escapedBackslashes.Replace ("'", "\\'");
			
			return "'" + escapedSingleQuotes + "'";
		}

		public string WalkBinaryOperation(string op, string left, string right)
		{
			return String.Format ("({1} {0} {2})", op, left, right);
		}

		public string WalkLogicalConjunction(string left, string right)
		{
			return String.Format ("({0} AND {1})", left, right);
		}

		public string WalkLogicalDisjunction(string left, string right)
		{
			return String.Format ("({0} OR {1})", left, right);
		}

		public string WalkReference(string[] value)
		{
			if (value.Length == 0) {
				throw new OracularException ("reference has no segments");
			}

			var root = rootAlias ?? value [0];
			var table = config.GetTable (value [0]);

			return serializeReference (root, table, value.Skip (1));
		}

		private void joinTo (string rel, OracularTable parentTable, string joinFrom, ParentConfig parentConfig)
		{
			var joinExpression = String.Format (
				"INNER JOIN [{0}] [{1}] ON [{1}].[{2}] = [{3}].[{4}]",
				parentTable.Table,
				rel,
				parentTable.Id,
				joinFrom,
				parentConfig.Id
			);

			joinTables [rel] = joinExpression;
		}

		private string serializeReference (string root, OracularTable table, IEnumerable<string> segments)
		{
			var segmentCount = segments.Count ();

			var name = "[" + root + "]";

			if (segmentCount == 0)
			{
				return name;
			}

			if (segmentCount == 1)
			{
				var field = table.GetField (segments.First());

				if (field != null)
				{
					name += ".[" + field.Name + "]";

					switch (field.Type)
					{
					case FieldType.Boolean:
						return name + " = 1";

					default:
						return name;
					}
				}

				var parent = table.GetParent (segments.First());

				if (parent != null)
				{
					var parentTable = config.GetTable (parent.Table);
					if (parentTable == null)
					{
						throw new OracularException ("parent table not found: "+ parent.Table);
					}

					var rel = root + "." + parent.Name;

					joinTo (rel, parentTable, root, parent);

					return "[" + rel + "]";
				}

				var message = String.Format ("reference not found: {0}", segments.First());
				throw new OracularException (message);
			}
			else
			{
				var parent = table.GetParent(segments.First());

				if (parent == null)
				{
					var message = String.Format ("parent not found on table {0}: {1}", table.Table, segments.First());
					throw new OracularException (message);
				}

				var nextRel = root + "." + parent.Name;
				var nextTable = config.GetTable (parent.Table);

				if (nextTable == null)
				{
					var message = String.Format ("table not found: {0}", parent.Table);
					throw new OracularException (message);
				}

				joinTo (nextRel, nextTable, root, parent);

				return serializeReference (nextRel, nextTable, segments.Skip (1));
			}
		}

		public string WalkMacroExpansion(string macro, string[] arguments)
		{
			return "NULL";
		}

		public IEnumerable<string> JoinTables => joinTables.Values;
	}
}

