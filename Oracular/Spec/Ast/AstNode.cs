using System;

namespace Oracular.Spec.Ast
{
	public abstract class AstNode
	{
		public AstNode () {}

		public abstract T Walk<T> (IPreorderWalker<T> walker, T previous);
		public abstract T Walk<T> (IPostorderWalker<T> walker);
	}
}

