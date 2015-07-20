using System;

namespace Oracular.Spec.Ast
{
	public class LogicalConjunction : AstNode
	{
		public readonly AstNode Left;
		public readonly AstNode Right;

		public LogicalConjunction (AstNode left, AstNode right)
		{
			this.Left = left;
			this.Right = right;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkConjunction (previous, Left, Right);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkConjunction (Left.Walk (walker), Right.Walk (walker));
		}
	}
}

