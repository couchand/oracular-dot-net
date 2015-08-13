namespace Oracular.Spec

open System.Text.RegularExpressions

module Lexer =
    type Token =
        | EOF
        | Reference of string list
        | StringLiteral of string
        | NumberLiteral of double
        | Operator of string
        | OpenParen
        | CloseParen
        | Comma

    let TOKEN_RE = new Regex(@"^(
                [a-zA-Z_][a-zA-Z0-9_]* (\.[a-zA-Z_][a-zA-Z0-9_]*)* |  # reference
                ' (?:\\.|[^'])* '?    | # single-quoted string
                "" (?:\\.|[^""])* ""? | # double-quoted string
                -?[0-9]+(\.[0-9]+)?   | # number
                (<=|>=|!=|[-+*/=!<>]) | # operator
                [(),]                 | # function call delimiters
                \#[^\n\r]*            | # comment
                \s+                     # ignore whitespace
            )", RegexOptions.Compiled ||| RegexOptions.IgnorePatternWhitespace
    )

    let IGNORE_RE = new Regex ("^(\\s+|#[^\\n\\r]*)$", RegexOptions.Compiled)
    let OPERATOR_RE = new Regex ("^(<=|>=|!=|[-+*/=!<>]|and|or)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
    let NUMBER_RE = new Regex ("^-?[0-9]+(\\.[0-9]+)?$",  RegexOptions.Compiled)
    let OPEN_STRING_RE = new Regex ("^('(?:\\\\.|[^'])*'?|\"(?:\\\\.|[^\"])*\"?)$", RegexOptions.Compiled)
    let CLOSED_STRING_RE = new Regex ("^('(?:\\\\.|[^'])*'|\"(?:\\\\.|[^\"])*\")$", RegexOptions.Compiled)
    let REFERENCE_RE = new Regex ("^[a-zA-Z_][a-zA-Z0-9_]*(\\.[a-zA-Z_][a-zA-Z0-9_]*)*$", RegexOptions.Compiled)
    let BACKSLASHES_RE = new Regex ("\\\\\\\\", RegexOptions.Compiled)

    exception LexerException of string * string
        with
            override this.Message =
                let (LexerException(message, input)) = upcast this
                System.String.Format("Lexer error: {0} at input {1}", message, input)

    type Lexer =
        abstract member GetToken: unit -> Token

    type StringLexer (_source:string) =
        let mutable source = _source

        let (|MatchToken|_|) (pattern:Regex) input =
            if input = null then None
            else
                let m = pattern.Match input
                if m.Success then
                    Some (m.ToString ())
                else None

        let parseNumber num =
            let result = ref 0.

            if System.Double.TryParse (num, result) then !result
            else failwith "cannot parse number literal"

        let parseString (str:string) =
            let quote = str.Substring (0, 1)
            let inner = str.Substring (1, str.Length - 2)

            let matcher = new Regex ("\\\\" + quote)

            BACKSLASHES_RE.Replace (matcher.Replace (inner, quote), "\\")

        let rec getToken () =
            match source with
            | null
            | "" -> EOF

            | MatchToken TOKEN_RE matched ->
                source <- source.Substring (matched.Length)

                match matched with
                | MatchToken IGNORE_RE _ -> getToken ()
                | MatchToken OPERATOR_RE op -> Operator (op.ToLower ())
                | MatchToken NUMBER_RE num -> NumberLiteral (parseNumber num)

                | MatchToken CLOSED_STRING_RE str -> StringLiteral (parseString str)
                | MatchToken OPEN_STRING_RE str -> raise (LexerException ("string value not closed", str))

                | _ -> Comma

            | _ -> raise (LexerException ("invalid input", source))

        member this.GetToken () = (this :> Lexer).GetToken()

        interface Lexer with
            member this.GetToken () = getToken ()

    type ArrayLexer (_source:Token list) =
        let mutable source = _source

        let getToken () =
            match source with
            | [] -> Token.EOF
            | x::xs ->
                source <- xs
                x

        member this.GetToken () = (this :> Lexer).GetToken()

        interface Lexer with
            member this.GetToken () = getToken ()