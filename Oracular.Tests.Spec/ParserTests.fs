namespace Oracular.Tests.Spec

open NUnit.Framework

open Oracular.Spec.Ast
open Oracular.Spec.Lexer
open Oracular.Spec.Parser

module ParserTests =
    let makeParser (input) =
        Parser (ArrayLexer (input))

    [<Test>]
    let ``Errors on not enough input``() =
        let parser = makeParser ([])

        let ex = Assert.Throws<ParserException> (fun _ -> parser.Parse() |> ignore)

        Assert.That (ex.Message, Is.StringContaining "not enough")

    [<Test>]
    [<TestCase(42)>]
    [<TestCase(0)>]
    [<TestCase(-1)>]
    [<TestCase(4.2)>]
    let ``Parse numbers`` (value) =
        let parser = makeParser ([ Token.NumberLiteral (value) ])

        let tree = parser.Parse ()
        match tree with
        | AstNode.NumberLiteral (actual) -> Assert.AreEqual (value, actual)
        | _ -> Assert.Fail ()

    [<Test>]
    [<TestCase("foobar")>]
    [<TestCase("foobar")>]
    let ``Parse strings`` (value) =
        let parser = makeParser ([ Token.StringLiteral (value) ])

        let tree = parser.Parse ()
        match tree with
        | AstNode.StringLiteral (actual) -> Assert.AreEqual (value, actual)
        | _ -> Assert.Fail ()

    [<Test>]
    [<TestCase("null")>]
    [<TestCase("nUlL")>]
    [<TestCase("NulL")>]
    [<TestCase("NULL")>]
    let ``Parse null`` (ref) =
        let parser = makeParser ([ Token.Reference [ ref ] ])

        let tree = parser.Parse ()
        match tree with
        | AstNode.NullLiteral -> Assert.Pass ()
        | _ -> Assert.Fail ()

    [<Test>]
    [<TestCase("true", true)>]
    [<TestCase("TrUe", true)>]
    [<TestCase("TRUE", true)>]
    [<TestCase("false", false)>]
    [<TestCase("fALsE", false)>]
    [<TestCase("FALSE", false)>]
    let ``Parse booleans`` (ref, expected) =
        let parser = makeParser ([ Token.Reference [ ref ] ])

        let tree = parser.Parse ()
        match tree with
        | AstNode.BooleanLiteral b -> Assert.AreEqual (expected, b)
        | _ -> Assert.Fail ()