using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class TypeChecker : IPostorderWalker<TypeSpecifier>
	{
		private readonly OracularConfig config;

		public TypeChecker()
		{
			this.config = new OracularConfig (new List<OracularTable> (), new List<OracularSpec> ());
		}

		public TypeChecker(OracularConfig config)
		{
			this.config = config;
		}

		public TypeSpecifier WalkNullLiteral()
		{
			return TypeSpecifier.Any;
		}

		public TypeSpecifier WalkBooleanLiteral(bool value)
		{
			return TypeSpecifier.Boolean;
		}

		public TypeSpecifier WalkNumberLiteral(double value)
		{
			return TypeSpecifier.Number;
		}

		public TypeSpecifier WalkStringLiteral(string value)
		{
			return TypeSpecifier.String;
		}

		private TypeSpecifier findFieldInTable(OracularTable table, IEnumerable<string> segments)
		{
			if (segments.Count () == 0)
			{
				throw new OracularException ("no references to follow");
			}

			if (segments.Count() == 1)
			{
				var field = table.GetField (segments.First());
				if (field == null)
				{
					var tryParent = table.GetParent (segments.First ());
					if (tryParent == null)
					{
						var message = String.Format ("table {0} has no field {1}", table.Table, segments.First());
						throw new TypeCheckException (message);
					}

					return TypeSpecifier.GetTable (tryParent.Table);
				}

				switch (field.Type)
				{
				case FieldType.Boolean:
					return TypeSpecifier.Boolean;
				case FieldType.Number:
					return TypeSpecifier.Number;
				case FieldType.String:
					return TypeSpecifier.String;
				case FieldType.Date:
					return TypeSpecifier.Date;
				default:
					throw new OracularException ("unknown field type " + field.Type.ToString());
				}
			}

			var parent = table.GetParent (segments.First ());
			if (parent == null)
			{
				var message = String.Format ("table {0} has no parent {1}", table.Table, segments.First());
				throw new TypeCheckException (message);
			}

			var parentTable = config.GetTable (parent.Table);
			if (parentTable == null)
			{
				var message = String.Format ("cannot find parent table {0}", parent.Table);
				throw new TypeCheckException (message);
			}

			return findFieldInTable (parentTable, segments.Skip (1));
		}

		public TypeSpecifier WalkReference(string[] segments)
		{
			if (segments.Length == 0)
			{
				throw new OracularException ("reference has no segments");
			}

			var table = config.GetTable (segments [0]);
			if (table == null)
			{
				if (segments.Length != 1)
				{
					throw new TypeCheckException ("name not found: " + segments [0]);
				}

				if (Builtins.Contains (segments [0]))
				{
					return TypeSpecifier.GetFunction (
						TypeSpecifier.Boolean,
						new [] { TypeSpecifier.Any, TypeSpecifier.Any }
					);
				}

				var spec = config.GetSpec (segments [0]);
				if (spec == null)
				{
					throw new TypeCheckException ("name not found: " + segments [0]);
				}

				return TypeSpecifier.GetPredicate (spec.Table);
			}

			if (segments.Length == 1)
			{
				return TypeSpecifier.GetTable (table.Table);
			}

			return findFieldInTable (table, new List<string> (segments).Skip (1));
		}

		public TypeSpecifier WalkBinaryOperation(string op, TypeSpecifier left, TypeSpecifier right)
		{
			var coalesced = left.Coalesce (right);

			switch (op)
			{
			case "=":
			case "!=":
			case "<":
			case ">":
			case "<=":
			case ">=":
				if (left.Type == SpecType.Any || right.Type == SpecType.Any)
				{
					if (op == "=" || op == "!=")
					{
						return TypeSpecifier.Boolean;
					}

					var message = String.Format ("invalid types for operator {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}

				if (coalesced == null)
				{
					var message = String.Format ("incompatible types in {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}

				if (coalesced.Type == SpecType.Boolean && op != "=" && op != "!=")
				{
					var message = String.Format ("invalid types for operator {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException(message);
				}

				return TypeSpecifier.Boolean;

			case "+":
			case "-":
			case "*":
			case "/":
				if (left.Type != SpecType.Number || right.Type != SpecType.Number)
				{
					var message = String.Format ("invalid types for operator {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}
				return coalesced;
			}

			throw new OracularException ("operator not known");
		}

		public TypeSpecifier WalkLogicalConjunction(TypeSpecifier left, TypeSpecifier right)
		{
			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in conjunction: {0} and {1}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			return TypeSpecifier.Boolean;
		}

		public TypeSpecifier WalkLogicalDisjunction(TypeSpecifier left, TypeSpecifier right)
		{
			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in disjunction: {0} and {1}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			return TypeSpecifier.Boolean;
		}

		public TypeSpecifier WalkLogicalNegation(TypeSpecifier child)
		{
			if (child != TypeSpecifier.Boolean)
			{
				var message = String.Format ("invalid type in negation: {0}", child.ToString ());
				throw new TypeCheckException (message);
			}

			return TypeSpecifier.Boolean;
		}

		public TypeSpecifier WalkMacroExpansion(Reference macro, AstNode[] arguments)
		{
			var macroType = macro.Walk (this);

			if (macroType.Type != SpecType.Function)
			{
				throw new TypeCheckException ("not a function type: " + macroType.Type.ToString());
			}

			if (macroType.ParameterTypes.Length != arguments.Length)
			{
				var message = String.Format ("airity mismatch: {0} parameters and {1} arguments", macroType.ParameterTypes.Length, arguments.Length);
				throw new TypeCheckException (message);
			}

			for (var i = 0; i < macroType.ParameterTypes.Length; i += 1)
			{
				var argumentType = arguments [i].Walk (this);
				var coalesced = macroType.ParameterTypes [i].Coalesce (argumentType);

				if (coalesced == null)
				{
					var message = String.Format ("function parameter type mismatch: {0} and {1}", macroType.ParameterTypes [i].ToString (), arguments [i].ToString ());
					throw new TypeCheckException (message);
				}
			}

			return macroType.ReturnType;
		}
	}

	public class TypeCheckException : OracularException
	{
		public TypeCheckException(string message)
			: base(message) {}
	}
}

