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

		public override AstNode Invert ()
		{
			throw new OracularException ("null literal cannot be inverted");
		}
	}
}

