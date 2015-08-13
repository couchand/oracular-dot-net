namespace Oracular.Tests.Spec

open NUnit.Framework

open Oracular.Spec.Ast

module AstNodeTests =
    [<Test>]
    [<TestCase(false)>]
    [<TestCase(true)>]
    let ``Inverting boolean literals returns the inverted version``(b) =
        let original = AstNode.BooleanLiteral (b)
        let inverted = invert original

        let expected = not b
        match inverted with
        | BooleanLiteral (actual) -> Assert.AreEqual (expected, actual)
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting null literals fails``() =
        let nullNode = AstNode.NullLiteral

        let ex = Assert.Throws<System.ArgumentException> (fun _ -> invert nullNode |> ignore)

        Assert.That (ex.Message, Is.StringContaining "null")

    [<Test>]
    let ``Inverting string literals fails``() =
        let stringNode = AstNode.StringLiteral "foobar"

        let ex = Assert.Throws<System.ArgumentException> (fun _ -> invert stringNode |> ignore)

        Assert.That (ex.Message, Is.StringContaining "string")

    [<Test>]
    let ``Inverting number literals fails``() =
        let numberNode = AstNode.NumberLiteral 42.

        let ex = Assert.Throws<System.ArgumentException> (fun _ -> invert numberNode |> ignore)

        Assert.That (ex.Message, Is.StringContaining "number")

    [<Test>]
    let ``Inverting binary equals returns not equals``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.Equal, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.NotEqual, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting binary not equals returns equals``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.NotEqual, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.Equal, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting binary less than returns greater or equal``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.LessThan, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.GreaterOrEqual, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting binary greater or equal returns less than``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.GreaterOrEqual, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.LessThan, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting binary greater than returns less or equal``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.GreaterThan, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.LessOrEqual, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting binary less or equal returns greater than``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.LessOrEqual, one, two)

        match invert binaryNode with
        | AstNode.BinaryOperation (BinaryOperator.GreaterThan, one, two) -> ()
        | _ -> Assert.Fail ()

    [<Test>]
    let ``Inverting other binaries returns wrapped in a negation``() =
        let one = AstNode.NumberLiteral 1.
        let two = AstNode.NumberLiteral 2.
        let binaryNode = AstNode.BinaryOperation (BinaryOperator.Plus, one, two)

        match invert binaryNode with
        | AstNode.LogicalNegation (binaryNode) -> ()
        | _ -> Assert.Fail ()