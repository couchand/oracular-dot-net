using System;

namespace Oracular.Spec.Ast
{
	public class LogicalNegation : AstNode
	{
		public readonly AstNode Child;

		public LogicalNegation (AstNode child)
		{
			this.Child = child;
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			var nested = Child.Walk (walker);
			return walker.WalkLogicalNegation (nested);
		}

		public override T Walk<T> (IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkLogicalNegation (previous, Child);
		}

		public override AstNode Invert ()
		{
			return Child;
		}
	}
}

