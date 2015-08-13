namespace Oracular.Tests.Spec

open NUnit.Framework

open Oracular.Spec.Lexer

module StringLexerTests =
    [<Test>]
    let ``Returns EOF on null input``() =
        let lexer = new StringLexer (null)

        Assert.AreEqual (Token.EOF, lexer.GetToken ())

    [<Test>]
    let ``Returns EOF on empty input``() =
        let lexer = new StringLexer ("")

        Assert.AreEqual (Token.EOF, lexer.GetToken ())
        Assert.AreEqual (Token.EOF, lexer.GetToken ())
        Assert.AreEqual (Token.EOF, lexer.GetToken ())

    [<Test>]
    [<TestCase("# this is a comment")>]
    [<TestCase("# this one, too")>]
    let ``Ignores comments``(input) =
        let lexer = new StringLexer (input)

        Assert.AreEqual (Token.EOF, lexer.GetToken ())

    [<Test>]
    [<TestCase("      ")>]
    [<TestCase("\t")>]
    [<TestCase("\n")>]
    [<TestCase("\r")>]
    let ``Ignores whitespace``(input) =
        let lexer = new StringLexer (input)

        Assert.AreEqual (Token.EOF, lexer.GetToken ())

    [<Test>]
    [<TestCase("42", 42.)>]
    [<TestCase("0", 0.)>]
    [<TestCase("-1", -1.)>]
    [<TestCase("4.2", 4.2)>]
    let ``Lexes number literals``(input, expected) =
        let lexer = new StringLexer (input)

        match lexer.GetToken () with
        | Token.NumberLiteral (actual) -> Assert.AreEqual (expected, actual)
        | _ -> Assert.Fail ()


    [<Test>]
    [<TestCase("<", "<")>]
    [<TestCase(">", ">")>]
    [<TestCase("<=", "<=")>]
    [<TestCase(">=", ">=")>]
    [<TestCase("=", "=")>]
    [<TestCase("!=", "!=")>]
    [<TestCase("+", "+")>]
    [<TestCase("-", "-")>]
    [<TestCase("*", "*")>]
    [<TestCase("/", "/")>]
    [<TestCase("and", "and")>]
    [<TestCase("aNd", "and")>]
    [<TestCase("AND", "and")>]
    [<TestCase("or", "or")>]
    [<TestCase("oR", "or")>]
    [<TestCase("OR", "or")>]
    let ``Lexes operators``(input, expected) =
        let lexer = new StringLexer (input)

        match lexer.GetToken () with
        | Token.Operator (op) -> Assert.AreEqual (expected, op)
        | _ -> Assert.Fail ()


    [<Test>]
    [<TestCase("'foobar'", "foobar")>]
    [<TestCase("\"foobar\"", "foobar")>]
    [<TestCase("'\\'\\''", "''")>]
    [<TestCase("\"\\\"\\\"\"", "\"\"")>]
    [<TestCase("'\\\\'", "\\")>]
    let ``Lexes string literals``(input, expected) =
        let lexer = new StringLexer (input)

        match lexer.GetToken () with
        | Token.StringLiteral (str) -> Assert.AreEqual (expected, str)
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Errors on unclosed single quotes``() =
        let lexer = new StringLexer "'foobar"

        let ex = Assert.Throws<LexerException> (fun _ -> lexer.GetToken() |> ignore)

        Assert.That (ex.Message, Is.StringContaining "string")

    [<Test>]
    let ``Errors on unclosed double quotes``() =
        let lexer = new StringLexer "\"foobar"

        let ex = Assert.Throws<LexerException> (fun _ -> lexer.GetToken() |> ignore)

        Assert.That (ex.Message, Is.StringContaining "string")

    [<Test>]
    [<TestCase("{foobar}")>]
    let ``Errors on invalid input``(input) =
        let lexer = new StringLexer (input)

        let ex = Assert.Throws<LexerException> (fun _ -> lexer.GetToken() |> ignore)

        Assert.That (ex.Message, Is.StringContaining "invalid")