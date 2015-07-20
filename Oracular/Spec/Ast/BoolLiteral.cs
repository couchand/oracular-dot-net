using System;

namespace Oracular.Spec.Ast
{
	public class BoolLiteral : AstNode
	{
		public readonly bool Value;

		public BoolLiteral (bool value)
		{
			this.Value = value;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkBooleanLiteral (previous, Value);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkBooleanLiteral (Value);
		}
	}
}

