using System;

namespace Oracular.Spec.Ast
{
	public abstract class AstNode
	{
		private static int idsAssigned = 0;

		public readonly int Id;

		public AstNode ()
		{
			Id = idsAssigned += 1;
		}

		public abstract T Walk<T> (IPreorderWalker<T> walker, T previous);
		public abstract T Walk<T> (IPostorderWalker<T> walker);
	}
}

