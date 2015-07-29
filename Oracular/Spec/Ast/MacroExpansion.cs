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
			return walker.WalkMacroExpansion (previous, Macro, Arguments);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkMacroExpansion (Macro, Arguments);
		}
	}
}

