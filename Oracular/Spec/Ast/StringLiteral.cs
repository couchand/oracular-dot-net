using System;

namespace Oracular.Spec.Ast
{
	public class StringLiteral : AstNode
	{
		public readonly string Value;

		public StringLiteral (string value)
		{
			this.Value = value;
		}
	}
}

