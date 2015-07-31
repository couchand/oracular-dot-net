using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class RefChecker : IPreorderWalker<string[]>
	{
		private readonly OracularConfig config;

		public RefChecker()
		{
			this.config = new OracularConfig (new List<OracularTable> (), new List<OracularSpec> ());
		}

		public RefChecker(OracularConfig config)
		{
			this.config = config;
		}

		// all of these are intentionally blank

		public string[] WalkNullLiteral(string[] tables)
		{
			return tables;
		}

		public string[] WalkBooleanLiteral(string[] tables, bool value)
		{
			return tables;
		}

		public string[] WalkNumberLiteral(string[] tables, double value)
		{
			return tables;
		}

		public string[] WalkStringLiteral(string[] tables, string value)
		{
			return tables;
		}

		public string[] WalkBinaryOperation(string[] tables, string op, AstNode left, AstNode right)
		{
			return tables;
		}

		public string[] WalkLogicalConjunction(string[] tables, AstNode left, AstNode right)
		{
			return tables;
		}

		public string[] WalkLogicalDisjunction(string[] tables, AstNode left, AstNode right)
		{
			return tables;
		}

		public string[] WalkLogicalNegation(string[] tables, AstNode child)
		{
			return tables;
		}

		// this is where the meat is

		public string[] WalkReference(string[] tables, string[] segments)
		{
			if (segments.Length == 0)
			{
				throw new OracularException ("reference has no segments");
			}

			if (segments.Length == 1)
			{
				if (Builtins.Contains (segments [0]))
					return tables;

				var refSpec = config.GetSpec (segments [0]);
				if (refSpec != null)
					return tables;
			}

			var tableSet = new HashSet<string> (tables);

			if (!tableSet.Contains (segments [0]))
			{
				var message = String.Format("reference is not to a joined table: {0}", segments[0]);
				throw new RefCheckException (message);
			}

			// TODO
			return tables;
		}

		public string[] WalkMacroExpansion(string[] tables, Reference macro, AstNode[] args)
		{
			if (macro.Value.Length != 1)
			{
				throw new OracularException ("function reference has invalid segment count");
			}

			var name = macro.Value [0];
			if (Builtins.Contains (name))
			{
				if (args.Length == 0)
				{
					throw new RefCheckException ("builtin requires reference parameter");
				}

				var first = args [0] as Reference;
				if (first == null)
				{
					throw new RefCheckException ("builtin requires reference parameter");
				}

				if (first.Value.Length == 0)
				{
					throw new OracularException ("reference has no segments");
				}

				return new List<string>(tables)
					.Concat(new [] { first.Value [0] })
					.ToArray();
			}

			var refChecked = macro.Walk (this, tables);

			// TODO
			return tables;
		}
	}

	public class RefCheckException : OracularException
	{
		public RefCheckException(string message)
			: base(message) {}
	}
}

