using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class Sqlizer : IPostorderWalker<string>
	{
		readonly OracularConfig config;
		readonly OracularTable rootTable;
		readonly string rootAlias;
		readonly Dictionary<string, string> joinTables;
		readonly List<string> otherJoins;
		readonly List<string> commonTableExpressions;

		public Sqlizer(OracularTable root)
			: this(root, new OracularConfig (new List<OracularTable> (), new List<OracularSpec> ())) {}

		public Sqlizer(OracularTable root, OracularConfig config)
			:this(root, config, null) {}

		public Sqlizer(OracularTable root, OracularConfig config, string alias)
		{
			this.rootTable = root;
			this.config = config;
			this.rootAlias = alias;

			this.joinTables = new Dictionary<string, string> ();
			this.otherJoins = new List<string> ();
			this.commonTableExpressions = new List<string> ();
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

		public string WalkLogicalNegation(string child)
		{
			return String.Format ("NOT({0})", child);
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

		public string WalkMacroExpansion(Reference macro, AstNode[] arguments)
		{
			if (macro.Value.Length != 1)
			{
				throw new OracularException ("macro reference has invalid segment count");
			}

			var name = macro.Value [0];

			if (Builtins.Contains (name))
			{
				var builtin = Builtins.Get (name);

				if (arguments.Length == 0)
				{
					throw new OracularException ("macro reference has invalid argument count");
				}

				var reference = arguments [0] as Reference;
				if (reference == null)
				{
					throw new OracularException ("macro reference requires reference parameter");
				}

				if (reference.Value.Length != 1 && reference.Value.Length != 2)
				{
					throw new OracularException ("macro reference parameter has invalid segment count");
				}

				var child = config.GetTable (reference.Value [0]);
				if (child == null)
				{
					var err = String.Format ("table not found for macro reference: {0}", reference.Value[0]);
					throw new OracularException (err);
				}

				if (arguments.Length == 1 || arguments.Length == 2)
				{
					var expansion = builtin.ExpandMacro (config, rootTable, child, reference, arguments.Length == 1 ? null : arguments [1]);

					otherJoins.AddRange (expansion.Join);
					commonTableExpressions.AddRange (expansion.With);

					return expansion.Where;
				}

				throw new OracularException ("macro reference has invalid argument count");
			}

			var spec = config.GetSpec (name);
			if (spec != null)
			{
				if (arguments.Length != 1)
				{
					throw new OracularException ("referenced spec has invalid argument count");
				}

				var specTable = config.GetTable (spec.Table);

				var rel = arguments [0].Walk (this);
				rel = rel.Substring (1, rel.Length - 2);
				var nestedEnvironment = new Sqlizer (specTable, config, rel);

				var expanded = spec.Spec.Walk (nestedEnvironment);

				foreach (var k in nestedEnvironment.joinTables.Keys)
				{
					joinTables [k] = nestedEnvironment.joinTables [k];
				}

				return expanded;
			}

			var message = String.Format("reference not found: {0}", macro.Value [0]);
			throw new OracularException(message);
		}

		public IEnumerable<string> JoinTables => joinTables.Values.Concat(otherJoins);
		public IEnumerable<string> CommonTableExpressions => commonTableExpressions;
	}
}

