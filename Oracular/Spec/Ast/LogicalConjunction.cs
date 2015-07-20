using System;

namespace Oracular.Spec.Ast
{
	public class LogicalDisjunction : AstNode
	{
		public readonly AstNode Left;
		public readonly AstNode Right;

		public LogicalDisjunction (AstNode left, AstNode right)
		{
			this.Left = left;
			this.Right = right;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkDisjunction (previous, Left, Right);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkDisjunction (Left.Walk (walker), Right.Walk (walker));
		}
	}
}

