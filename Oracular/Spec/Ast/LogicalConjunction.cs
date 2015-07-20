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
	}
}

