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
	}
}

