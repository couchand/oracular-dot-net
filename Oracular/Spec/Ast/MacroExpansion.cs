using System;
using System.Collections.Generic;

namespace Oracular.Spec.Ast
{
	public class MacroExpansion : AstNode
	{
		public readonly Reference Macro;
		public readonly AstNode[] Arguments;

		public MacroExpansion (Reference macro, AstNode[] args)
		{
			this.Macro = macro;
			this.Arguments = args;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			var next = walker.WalkMacroExpansion (previous, Macro, Arguments);
			Macro.Walk (walker, next);
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

			return walker.WalkMacroExpansion (Macro.Walk (walker), args.ToArray());
		}
	}
}

