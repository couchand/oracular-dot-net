using System;
using System.Text.RegularExpressions;

namespace Oracular.Spec
{
	public class StringLexer : ILexer
	{
		private static readonly Regex TOKEN_RE = new Regex(
			@"^(
				[a-zA-Z_][a-zA-Z0-9_]* (\.[a-zA-Z_][a-zA-Z0-9_]*)* |  # reference
				' (?:\\.|[^'])* '?    | # single-quoted string
				"" (?:\\.|[^""])* ""? | # double-quoted string
				-?[0-9]+(\.[0-9]+)?   | # number
				(<=|>=|!=|[-+*/=!<>]) | # operator
				[(),]                 | # function call delimiters
				\#[^\n\r]*            | # comment
				\s+                     # ignore whitespace
			)",
			RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace
		);

		private static readonly Regex IGNORE_RE = new Regex ("^(\\s+|#[^\\n\\r]*)$", RegexOptions.Compiled);
		private static readonly Regex OPERATOR_RE = new Regex ("^(<=|>=|!=|[-+*/=!<>]|and|or)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex NUMBER_RE = new Regex ("^-?[0-9]+(\\.[0-9]+)?$",  RegexOptions.Compiled);
		private static readonly Regex OPEN_STRING_RE = new Regex ("^('(?:\\\\.|[^'])*'?|\"(?:\\\\.|[^\"])*\"?)$", RegexOptions.Compiled);
		private static readonly Regex CLOSED_STRING_RE = new Regex ("^('(?:\\\\.|[^'])*'|\"(?:\\\\.|[^\"])*\")$", RegexOptions.Compiled);
		private static readonly Regex REFERENCE_RE = new Regex ("^[a-zA-Z_][a-zA-Z0-9_]*(\\.[a-zA-Z_][a-zA-Z0-9_]*)*$", RegexOptions.Compiled);

		private string source;

		public StringLexer (string source)
		{
			this.source = source;
		}

		public Token GetToken()
		{
			if (source == null || source.Length == 0)
				return new Token (TokenType.EOF);

			var match = TOKEN_RE.Match (source);

			if (!match.Success)
			{
				throw new LexerException ("invalid input", source);
			}

			var token = accept (match);

			if (IGNORE_RE.IsMatch (token))
			{
				return GetToken ();
			}

			if (OPERATOR_RE.IsMatch (token))
			{
				return new Token (TokenType.Operator, token.ToLower());
			}

			if (NUMBER_RE.IsMatch (token))
			{
				return new Token (TokenType.Number, token);
			}

			if (CLOSED_STRING_RE.IsMatch (token))
			{
				return new Token (TokenType.String, token);
			}

			if (REFERENCE_RE.IsMatch (token))
			{
				return new Token (TokenType.Reference, token);
			}

			switch (token)
			{
			case "(":
				return new Token (TokenType.OpenParen);
			case ")":
				return new Token (TokenType.CloseParen);
			case ",":
				return new Token (TokenType.Comma);
			}

			if (OPEN_STRING_RE.IsMatch (token))
			{
				throw new LexerException ("string value not closed", token);
			}

			throw new LexerException ("invalid token", token);
		}

		private string accept(Match match)
		{
			var token = match.Value;
			var remaining = source.Substring (match.Length);

			source = remaining;
			return token;
		}
	}

	public class LexerException : ApplicationException
	{
		public LexerException (string message, string input)
			: base(String.Format("Lexer error: {0} at input {1}", message, input)) {}
	}
}

