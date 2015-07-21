using System;

using Oracular.Spec;
using Oracular.Spec.Ast;

namespace Oracular
{
	public class OracularSpec
	{
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
	}
}

