using System;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public enum TypeSpecifier
	{
		Any,
		Boolean,
		Number,
		String,
		Date
	}

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
			if (left == TypeSpecifier.Any)
			{
				return right;
			}

			if (right == TypeSpecifier.Any)
			{
				return left;
			}

			if (right != left)
			{
				var message = String.Format ("incompatible types in {0}: {1} and {2}", op, left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			switch (op)
			{
			case "=":
			case "!=":
			case "<":
			case ">":
			case "<=":
			case ">=":
				return TypeSpecifier.Boolean;

			case "+":
			case "-":
			case "*":
			case "/":
				if (left != TypeSpecifier.Number)
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
			if (left == TypeSpecifier.Any)
			{
				return right;
			}

			if (right == TypeSpecifier.Any)
			{
				return left;
			}

			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in conjunction: {1} and {2}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			return left;
		}

		public TypeSpecifier WalkDisjunction(TypeSpecifier left, TypeSpecifier right)
		{
			if (left == TypeSpecifier.Any)
			{
				return right;
			}

			if (right == TypeSpecifier.Any)
			{
				return left;
			}

			if (right != TypeSpecifier.Boolean || left != TypeSpecifier.Boolean)
			{
				var message = String.Format ("incompatible types in disjunction: {1} and {2}", left.ToString (), right.ToString ());
				throw new TypeCheckException (message);
			}

			return left;
		}

		public TypeSpecifier WalkFunctionCall(TypeSpecifier function, TypeSpecifier[] arguments)
		{
			// TODO: anything
			return TypeSpecifier.Any;
		}
	}

	public class TypeCheckException : ApplicationException
	{
		public TypeCheckException(string message)
			: base(message) {}
	}
}

