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

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkNumberLiteral (previous, Value);
		}

		public override T Walk<T>(IPostorderWalker<T> walker)
		{
			return walker.WalkNumberLiteral (Value);
		}
	}
}

