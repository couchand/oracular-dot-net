using System;

namespace Oracular.Spec.Ast
{
	public class BooleanLiteral : AstNode
	{
		public readonly bool Value;

		public BooleanLiteral (bool value)
		{
			this.Value = value;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkBooleanLiteral (previous, Value);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkBooleanLiteral (Value);
		}

		public override AstNode Invert ()
		{
			return new BooleanLiteral (!Value);
		}
	}
}

