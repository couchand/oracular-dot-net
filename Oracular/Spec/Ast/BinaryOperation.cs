using System;

namespace Oracular.Spec.Ast
{
	public class BinaryOperation : AstNode
	{
		public readonly string Operator;
		public readonly AstNode Left;
		public readonly AstNode Right;

		public BinaryOperation (string op, AstNode left, AstNode right)
		{
			this.Operator = op;
			this.Left = left;
			this.Right = right;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			var next = walker.WalkBinaryOperation (previous, Operator, Left, Right);
			Left.Walk (walker, next);
			Right.Walk (walker, next);
			return next;
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkBinaryOperation (Operator, Left.Walk (walker), Right.Walk (walker));
		}
	}
}

