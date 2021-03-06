﻿using System;
using System.Collections.Generic;

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

		private static readonly Dictionary<string,string> INVERTED_OP = new Dictionary<string, string>
		{
			{ "=", "!=" },
			{ "!=", "=" },
			{ "<", ">=" },
			{ "<=", ">" },
			{ ">", "<=" },
			{ ">=", "<" }
		};

		public override AstNode Invert ()
		{
			if (!INVERTED_OP.ContainsKey (Operator))
			{
				throw new OracularException ("operator unknown: " + Operator);
			}

			return new BinaryOperation (INVERTED_OP [Operator], Left, Right);
		}
	}
}

