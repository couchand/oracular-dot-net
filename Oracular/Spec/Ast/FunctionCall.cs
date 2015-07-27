using System;
using System.Collections.Generic;

namespace Oracular.Spec.Ast
{
	public class FunctionCall : AstNode
	{
		public readonly Reference Function;
		public readonly AstNode[] Arguments;

		public FunctionCall (Reference fn, AstNode[] args)
		{
			this.Function = fn;
			this.Arguments = args;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			var next = walker.WalkFunctionCall (previous, Function, Arguments);
			Function.Walk (walker, next);
			foreach (var arg in Arguments)
			{
				arg.Walk (walker, next);
			}
			return next;
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			var args = new List<T> ();

			for (var i = 0; i < Arguments.Length; i++)
			{
				args.Add(Arguments [i].Walk (walker));
			}

			return walker.WalkFunctionCall (Function.Walk (walker), args.ToArray());
		}
	}
}

