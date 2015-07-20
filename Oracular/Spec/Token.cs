using System;
using System.Text.RegularExpressions;

namespace Oracular
{
	// poor-man's polymorphism
	public enum TokenType
	{
		EOF,
		Reference,
		String,
		Number,
		Operator,
		OpenParen,
		CloseParen,
		Comma
	}

	public class Token
	{
		private static readonly Regex BACKSLASHES_RE = new Regex ("\\\\\\\\", RegexOptions.Compiled);

		public readonly TokenType Type;
		private readonly string value;

		public Token (TokenType type)
		{
			this.Type = type;
		}

		public Token (TokenType type, string value)
		{
			this.Type = type;
			this.value = value;
		}

		public string GetRawValue()
		{
			return value;
		}

		public double GetNumberValue()
		{
			if (Type != TokenType.Number)
			{
				throw new TokenException ("not a number", this);
			}

			double result;
			if (Double.TryParse (value, out result))
			{
				return result;
			}

			throw new TokenException ("number invalid", this);
		}
		
		public string GetStringValue()
		{
			if (Type != TokenType.String)
			{
				throw new TokenException ("not a string", this);
			}

			var quote = value.Substring (0, 1);
			var unquoted = value.Substring (1, value.Length - 2);

			var matcher = new Regex ("\\\\" + quote);

			return BACKSLASHES_RE.Replace(matcher.Replace (unquoted, quote), "\\");
		}

		public string[] GetReferenceValue()
		{
			if (Type != TokenType.Reference)
			{
				throw new TokenException ("not a reference", this);
			}

			return value.Split ('.');
		}
	}

	public class TokenException : ApplicationException
	{
		public TokenException(string message, Token token)
			: base(String.Format("Token error: {0} on token {1} of type {2}", message, token.GetRawValue(), token.Type.ToString())) {}
	}
}

