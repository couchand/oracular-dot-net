using System;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public interface IPreorderWalker<T>
	{
		T WalkNullLiteral(T previous);
		T WalkBooleanLiteral(T previous, bool value);
		T WalkNumberLiteral(T previous, double value);
		T WalkStringLiteral(T previous, string value);

		T WalkBinaryOperation(T previous, string op, AstNode left, AstNode right);
		T WalkLogicalConjunction(T previous, AstNode left, AstNode right);
		T WalkLogicalDisjunction(T previous, AstNode left, AstNode right);

		T WalkReference(T previous, string[] value);
		T WalkMacroExpansion(T previous, Reference macro, AstNode[] arguments);
	}

	public interface IPostorderWalker<T>
	{
		T WalkNullLiteral();
		T WalkBooleanLiteral(bool value);
		T WalkNumberLiteral(double value);
		T WalkStringLiteral(string value);

		T WalkBinaryOperation(string op, T left, T right);
		T WalkLogicalConjunction(T left, T right);
		T WalkLogicalDisjunction(T left, T right);

		T WalkReference(string[] value);
		T WalkMacroExpansion(Reference macro, AstNode[] arguments);
	}
}

