using System;

namespace Oracular.Spec.Ast
{
	public class NullLiteral : AstNode
	{
		public NullLiteral () {}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkNullLiteral (previous);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkNullLiteral ();
		}
	}
}

