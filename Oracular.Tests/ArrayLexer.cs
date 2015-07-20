using System;
using Oracular.Spec;

namespace Oracular.Tests
{
	public class ArrayLexer : ILexer
	{
		private Token[] tokens;

		public ArrayLexer (Token[] tokens)
		{
			this.tokens = tokens;
		}

		public Token GetToken()
		{
			if (tokens == null || tokens.Length == 0)
			{
				return new Token (TokenType.EOF);
			}

			var result = tokens [0];

			var newTokens = new Token[tokens.Length - 1];
			for (var i = 1; i < tokens.Length; i += 1)
			{
				newTokens [i - 1] = tokens [i];
			}

			tokens = newTokens;
			return result;
		}
	}
}

