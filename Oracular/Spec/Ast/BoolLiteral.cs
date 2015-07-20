using System;

namespace Oracular.Spec.Ast
{
	public class BoolLiteral : AstNode
	{
		public readonly bool Value;

		public BoolLiteral (bool value)
		{
			this.Value = value;
		}
	}
}

