using System;

namespace Oracular.Spec.Ast
{
	public class Reference : AstNode
	{
		public readonly string[] Value;

		public Reference (string[] segments)
		{
			this.Value = segments;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkReference (previous, Value);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkReference (Value);
		}
	}
}

