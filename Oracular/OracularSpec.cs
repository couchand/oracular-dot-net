using System;
using System.Linq;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular
{
	public class OracularSpec
	{
		internal OracularConfig config;

		public readonly string Name;
		public readonly string Table;
		public readonly string Source;
		public readonly AstNode Spec;

		public OracularSpec (string name, string table, string spec)
		{
			if (name == null)
			{
				throw new OracularException ("spec config requires name");
			}
			this.Name = name;

			if (table == null)
			{
				throw new OracularException ("spec config requires table");
			}
			this.Table = table;

			if (spec == null)
			{
				throw new OracularException ("spec config requires spec expression");
			}
			this.Source = spec;

			var parser = new Parser (new StringLexer (spec));
			this.Spec = parser.Parse ();
		}

		public string ToSql()
		{
			var tableConfig = config.GetTable (Table);

			var builder = new Sqlizer (tableConfig, config);

			var whereClause = Spec.Walk (builder);

			var withClause = builder.CommonTableExpressions.Count () == 0 ? "" :
				"; WITH " + String.Join (",", builder.CommonTableExpressions) + "\n";

			var joinClause = builder.JoinTables.Count () == 0 ? "" :
				"\n" + String.Join ("\n", builder.JoinTables);

			return String.Format ("{0}SELECT [{1}].* FROM [{1}]{2}\nWHERE {3}",
				withClause, tableConfig.Table, joinClause, whereClause);
		}
	}
}

