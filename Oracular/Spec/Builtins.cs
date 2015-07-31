using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular
{
	public static class Builtins
	{
		private static readonly Dictionary<string, Builtin> builtins = new Dictionary<string, Builtin>
		{
			{ "any", new PredicateBuiltin("Any", " = 1") },
			{ "none", new PredicateBuiltin("No", " != 1") },
			{ "all", new PredicateBuiltin("AnyNot", " != 1", true) }
		};

		public static bool Contains(string name)
		{
			return builtins.ContainsKey (name.ToLower());
		}

		public static Builtin Get(string name)
		{
			return builtins [name.ToLower()];
		}
	}

	public class BuiltinExpansion
	{
		public List<string> With;
		public List<string> Join;
		public string Where;

		public BuiltinExpansion()
		{
			this.With = new List<string>();
			this.Join = new List<string>();
		}
	}

	public abstract class Builtin
	{
		public readonly string Name;

		public Builtin(string name)
		{
			this.Name = name;
		}

		public abstract BuiltinExpansion ExpandMacro (OracularConfig config, OracularTable parent, OracularTable child, Reference reference, AstNode nestedSpec);
	}

	public class PredicateBuiltin : Builtin
	{
		string suffix;
		bool invertNested;

		public PredicateBuiltin(string name, string suffix, bool invertNested = false)
			: base(name)
		{
			this.suffix = suffix;
			this.invertNested = invertNested;
		}

		public override BuiltinExpansion ExpandMacro (OracularConfig config, OracularTable parent, OracularTable child, Reference reference, AstNode nestedSpec)
		{
			var withTable = String.Format ("Annotated{0}{1}", parent.Table, nestedSpec.Id);

			var relationship = child.GetRelationshipTo (parent.Table);
			if (relationship == null) 
			{
				var message = String.Format ("unable to find relationship from {0} to {1}", child.Table, parent.Table);
				throw new OracularException (message);
			}

			var mainJoin = String.Format ("LEFT JOIN [{0}] ON [{0}].[{1}] = [{2}].[{3}]",
				withTable,
				parent.Id,
				parent.Table,
				parent.Id
			);

			var macroField = String.Format ("{0}{1}{2}", Name, child.Table, nestedSpec.Id);

			var builder = new Sqlizer (parent, config);

			var nestedWhere = nestedSpec == null ? "" : String.Format("\nWHERE {0}",
				(invertNested ? nestedSpec.Invert() : nestedSpec).Walk(builder)
			);

			var nestedJoins = builder.JoinTables.Count() == 0 ? "" : "\n" + String.Join("\n",
				builder.JoinTables
			);

			var nestedQuery = String.Format (@"[{0}] AS (
SELECT DISTINCT [{1}].[{2}], 1 [{3}]
FROM [{1}]
LEFT JOIN [{4}] ON [{4}].[{5}] = [{1}].[{2}]{6}{7}
)", withTable, parent.Table, parent.Id, macroField, child.Table, relationship.Id, nestedJoins, nestedWhere);

			var expansion = new BuiltinExpansion ();

			expansion.With.AddRange (builder.CommonTableExpressions);

			expansion.With.Add (nestedQuery);
			expansion.Join.Add (mainJoin);

			expansion.Where = String.Format ("[{0}].[{1}]{2}", withTable, macroField, suffix);

			return expansion;
		}
	}
}

