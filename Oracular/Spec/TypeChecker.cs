using System;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public class TypeChecker : IPostorderWalker<TypeSpecifier>
	{
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

		public TypeSpecifier WalkReference(string[] segments)
		{
			// TODO: not this
			return TypeSpecifier.Any;
		}

		public TypeSpecifier WalkOperator(string op, TypeSpecifier left, TypeSpecifier right)
		{
			switch (op)
			{
			case "=":
			case "!=":
			case "<":
			case ">":
			case "<=":
			case ">=":
				if (left == TypeSpecifier.Any || right == TypeSpecifier.Any)
				{
					if (op == "=" || op == "!=")
					{
						return TypeSpecifier.Boolean;
					}

					var message = String.Format ("invalid types for operator {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}

				if (right != left)
				{
					var message = String.Format ("incompatible types in {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}

				return TypeSpecifier.Boolean;

			case "+":
			case "-":
			case "*":
			case "/":
				if (left != TypeSpecifier.Number || right != TypeSpecifier.Number)
				{
					var message = String.Format ("invalid types for operator {0}: {1} and {2}", op, left.ToString (), right.ToString ());
					throw new TypeCheckException (message);
				}
				return left;
			}

			return left;
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

