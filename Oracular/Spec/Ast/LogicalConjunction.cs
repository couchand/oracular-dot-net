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
			var next = walker.WalkLogicalDisjunction (previous, Left, Right);
			Left.Walk (walker, next);
			Right.Walk (walker, next);
			return next;
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkLogicalDisjunction (Left.Walk (walker), Right.Walk (walker));
		}
	}
}

