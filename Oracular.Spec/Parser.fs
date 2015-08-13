namespace Oracular.Spec

open Oracular.Spec.Ast
open Oracular.Spec.Lexer

module Parser =
    exception ParserException of string * Token
        with
            override this.Message =
                let (ParserException(message, token)) = upcast this
                System.String.Format("Parser error: {0} at input {1}", message, token)

    type Parser(_lexer:Lexer.Lexer) =
        let lexer = _lexer

        let mutable currentToken = lexer.GetToken ()
        let mutable nextToken = lexer.GetToken ()

        let (|Null|NotNull|) (input : Token) =
            match input with
            | Token.Reference [ref] -> if ref.ToLower() = "null" then Null else NotNull
            | _ -> NotNull

        let (|Bool|_|) (input : Token) =
            match input with
            | Token.Reference [ref] ->
                match ref.ToLower () with
                | "true" -> Some true
                | "false" -> Some false
                | _ -> None
            | _ -> None

        let parse () =
            match currentToken with
            | Token.EOF -> raise (ParserException ("not enough input", currentToken))
            | Token.NumberLiteral n -> AstNode.NumberLiteral n
            | Token.StringLiteral s -> AstNode.StringLiteral s
            | Null -> AstNode.NullLiteral
            | Bool b -> AstNode.BooleanLiteral b
            | _ -> failwith "up"

        member this.Parse () =
            parse ()