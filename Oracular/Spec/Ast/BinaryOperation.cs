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
	}
}

