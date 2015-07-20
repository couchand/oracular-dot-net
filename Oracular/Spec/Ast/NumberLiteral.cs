using System;

namespace Oracular.Spec.Ast
{
	public class NumberLiteral : AstNode
	{
		public readonly double Value;

		public NumberLiteral (double value)
		{
			this.Value = value;
		}
	}
}

