﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class Parser
	{
		private static readonly Dictionary<string, int> PRECEDENCE = new Dictionary<string, int> {
			{ "*",   40 },
			{ "/",   40 },
			{ "+",   20 },
			{ "-",   20 },
			{ "<",   10 },
			{ ">",   10 },
			{ "<=",  10 },
			{ ">=",  10 },
			{ "=",   10 },
			{ "!=",  10 },
			{ "and",  4 },
			{ "or",   2 }
		};

		private readonly ILexer lexer;

		internal Token currentToken;
		internal Token nextToken;

		public Parser (ILexer lexer)
		{
			this.lexer = lexer;

			// prime the lookahead
			this.currentToken = lexer.GetToken ();
			this.nextToken = lexer.GetToken ();
		}

		public Token GetNextToken()
		{
			currentToken = nextToken;
			nextToken = lexer.GetToken ();

			return nextToken;
		}

		public AstNode ParseCurrentToken()
		{
			var result = _parseCurrentTokenKernel ();

			GetNextToken (); // consume token

			return result;
		}

		private AstNode _parseCurrentTokenKernel()
		{
			switch (currentToken.Type)
			{
			case TokenType.EOF:
				throw new ParserException ("not enough input", this);
					
			case TokenType.Number:
				return new NumberLiteral (currentToken.GetNumberValue ());

			case TokenType.String:
				return new StringLiteral (currentToken.GetStringValue ());

			case TokenType.OpenParen:
				GetNextToken (); // consume open paren

				var tok = ParseKernel ();

				if (currentToken.Type != TokenType.CloseParen)
				{
					throw new ParserException ("expected close paren", this);
				}

				return tok;

			case TokenType.Reference:
				var segments = currentToken.GetReferenceValue ();
				if (segments.Length == 1) {
					switch (segments [0].ToLower ()) {
					case "null":
						return new NullLiteral ();
					case "false":
						return new BooleanLiteral (false);
					case "true":
						return new BooleanLiteral (true);
					case "not":
						GetNextToken (); // consume not

						// ParseCurrentToken instead of ParseKernel
						// because not binds tighter than operators
						var child = _parseCurrentTokenKernel();

						return new LogicalNegation (child);
					}
				}
				if (nextToken.Type == TokenType.OpenParen)
				{
					var fnRef = new Reference (currentToken.GetReferenceValue ());
					GetNextToken (); // consume fn reference
					GetNextToken (); // consume open paren

					var args = new List<AstNode>();

					while (true)
					{
						args.Add (ParseKernel ());

						if (currentToken.Type == TokenType.CloseParen)
							break;

						if (currentToken.Type != TokenType.Comma)
						{
							throw new ParserException ("expected comma or close paren", this);
						}

						GetNextToken (); // consume comma
					}

					return new MacroExpansion (fnRef, args.ToArray ());
				}
				else
				{
					return new Reference (currentToken.GetReferenceValue ());
				}

			default:
				throw new ParserException ("case undefined", this);
			}
		}

		public AstNode ParseBinary(AstNode left)
		{
			if (currentToken.Type != TokenType.Operator)
			{
				throw new ParserException ("not an operator", this);
			}

			var op = currentToken.GetRawValue ();
			var prec = PRECEDENCE [op.ToLower ()];
			GetNextToken (); // consume operator

			var right = ParseCurrentToken ();

			while (currentToken.Type == TokenType.Operator && prec < PRECEDENCE [currentToken.GetRawValue ().ToLower ()])
			{
				right = ParseBinary (right);
			}

			if (op.Equals ("and", StringComparison.OrdinalIgnoreCase))
			{
				return new LogicalConjunction (left, right);
			}

			if (op.Equals ("or", StringComparison.OrdinalIgnoreCase))
			{
				return new LogicalDisjunction (left, right);
			}

			return new BinaryOperation (op, left, right);
		}

		public AstNode ParseKernel()
		{
			var result = ParseCurrentToken ();

			while (currentToken.Type == TokenType.Operator)
			{
				result = ParseBinary (result);
			}

			return result;
		}

		public AstNode Parse()
		{
			var result = ParseKernel ();

			if (currentToken.Type != TokenType.EOF)
			{
				throw new ParserException ("too much input", this);
			}

			return result;
		}
	}

	public class ParserException : OracularException
	{
		public ParserException(string message, Parser parser)
			: base(String.Format("Parse error: {0} at input {1}", message, parser.currentToken.GetRawValue())) {}
	}
}

