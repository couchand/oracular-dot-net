using System;

namespace Oracular.Spec.Ast
{
	public class StringLiteral : AstNode
	{
		public readonly string Value;

		public StringLiteral (string value)
		{
			this.Value = value;
		}

		public override T Walk<T>(IPreorderWalker<T> walker, T previous)
		{
			return walker.WalkStringLiteral (previous, Value);
		}

		public override T Walk<T> (IPostorderWalker<T> walker)
		{
			return walker.WalkStringLiteral (Value);
		}
	}
}

