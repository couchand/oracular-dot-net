using System;

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
	}
}

