﻿using System;

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
			var next = walker.WalkLogicalConjunction (previous, Left, Right);
			Left.Walk (walker, next);
			Right.Walk (walker, next);
			return next;
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkLogicalConjunction (Left.Walk (walker), Right.Walk (walker));
		}

		public override AstNode Invert ()
		{
			return new LogicalDisjunction (Left.Invert (), Right.Invert ());
		}
	}
}

