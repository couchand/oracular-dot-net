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
	}
}

