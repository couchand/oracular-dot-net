using System;
using System.Collections.Generic;
using System.Linq;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class TypeChecker : IPostorderWalker<TypeSpecifier>
	{
		private readonly Dictionary<string, OracularTable> tables;

		public TypeChecker()
		{
			this.tables = new Dictionary<string, OracularTable> ();
		}

		public TypeChecker(IEnumerable<OracularTable> tables)
		{
			this.tables = new Dictionary<string, OracularTable> ();
			foreach (var table in tables)
			{
				if (this.tables.ContainsKey (table.Table))
				{
					throw new OracularException ("table is duplicate: " + table.Table);
				}

				this.tables [table.Table] = table;
			}
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
					var message = String.Format ("table {0} has no field {1}", table.Table, segments.First());
					throw new TypeCheckException (message);
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

			if (!tables.ContainsKey (parent.Table))
			{
				var message = String.Format ("cannot find parent table {0}", parent.Table);
				throw new TypeCheckException (message);
			}

			var parentTable = tables [parent.Table];
			return findFieldInTable (parentTable, segments.Skip (1));
		}

		public TypeSpecifier WalkReference(string[] segments)
		{
			if (segments.Length == 0)
			{
				throw new OracularException ("reference has no segments");
			}

			if (!tables.ContainsKey (segments [0]))
			{
				throw new TypeCheckException ("table not found: " + segments [0]);
			}

			var table = tables [segments [0]];
			if (segments.Length == 1)
			{
				return TypeSpecifier.GetTable (table.Table);
			}

			return findFieldInTable (table, new List<string> (segments).Skip (1));
		}

		public TypeSpecifier WalkOperator(string op, TypeSpecifier left, TypeSpecifier right)
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

		public TypeSpecifier WalkConjunction(TypeSpecifier left, TypeSpecifier right)
		{
			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in conjunction: {0} and {1}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			// both are bool
			return left;
		}

		public TypeSpecifier WalkDisjunction(TypeSpecifier left, TypeSpecifier right)
		{
			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in disjunction: {0} and {1}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			// both are bool
			return left;
		}

		public TypeSpecifier WalkFunctionCall(TypeSpecifier function, TypeSpecifier[] arguments)
		{
			// TODO: anything
			return TypeSpecifier.Any;
		}
	}

	public class TypeCheckException : OracularException
	{
		public TypeCheckException(string message)
			: base(message) {}
	}
}

