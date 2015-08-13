namespace Oracular.Spec

module Ast =
    type BinaryOperator =
        | Plus
        | Minus
        | Times
        | Divide
        | Equal
        | NotEqual
        | LessThan
        | LessOrEqual
        | GreaterThan
        | GreaterOrEqual

    type AstNode =
        | NullLiteral
        | BooleanLiteral of bool
        | NumberLiteral of double
        | StringLiteral of string
        | ReferenceLiteral of string[]
        | BinaryOperation of BinaryOperator * AstNode * AstNode
        | LogicalConjunction of AstNode * AstNode
        | LogicalDisjunction of AstNode * AstNode
        | LogicalNegation of AstNode
        | MacroExpansion of string * AstNode[]

    let rec invert node =
        match node with
        | NullLiteral       -> invalidArg "node" "null literal cannot be inverted"
        | NumberLiteral (_) -> invalidArg "node" "number literal cannot be inverted"
        | StringLiteral (_) -> invalidArg "node" "string literal cannot be inverted"

        | BooleanLiteral (v) -> BooleanLiteral (not v)

        | BinaryOperation (Equal, l, r)             -> BinaryOperation (NotEqual, l, r)
        | BinaryOperation (NotEqual, l, r)          -> BinaryOperation (Equal, l, r)
        | BinaryOperation (LessThan, l, r)          -> BinaryOperation (GreaterOrEqual, l, r)
        | BinaryOperation (GreaterOrEqual, l, r)    -> BinaryOperation (LessThan, l, r)
        | BinaryOperation (GreaterThan, l, r)       -> BinaryOperation (LessOrEqual, l, r)
        | BinaryOperation (LessOrEqual, l, r)       -> BinaryOperation (GreaterThan, l, r)

        | LogicalConjunction (l, r) -> LogicalDisjunction (invert l, invert r)
        | LogicalDisjunction (l, r) -> LogicalConjunction (invert l, invert r)

        | LogicalNegation (n) -> n

        /// Fallback to just wrapping in a negation
        | BinaryOperation (_, _, _) -> LogicalNegation (node)
        | ReferenceLiteral (_)      -> LogicalNegation (node)
        | MacroExpansion (_, _)     -> LogicalNegation (node)