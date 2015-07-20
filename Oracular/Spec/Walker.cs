﻿using System;

using Oracular.Spec.Ast;

namespace Oracular.Spec
{
	public interface IPreorderWalker<T>
	{
		T WalkNullLiteral(T previous);
		T WalkBooleanLiteral(T previous, bool value);
		T WalkNumberLiteral(T previous, double value);
		T WalkStringLiteral(T previous, string value);

		T WalkOperator(T previous, string value, AstNode left, AstNode right);
		T WalkConjunction(T previous, AstNode left, AstNode right);
		T WalkDisjunction(T previous, AstNode left, AstNode right);

		T WalkReference(T previous, string[] value);
		T WalkFunctionCall(T previous, Reference function, AstNode[] arguments);
	}

	public interface IPostorderWalker<T>
	{
		T WalkNullLiteral();
		T WalkBooleanLiteral(bool value);
		T WalkNumberLiteral(double value);
		T WalkStringLiteral(string value);

		T WalkOperator(string value, T left, T right);
		T WalkConjunction(T left, T right);
		T WalkDisjunction(T left, T right);

		T WalkReference(string[] value);
		T WalkFunctionCall(T function, T[] arguments);
	}
}

