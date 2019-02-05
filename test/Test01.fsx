// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"
open FParsec

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190205\lib\netstandard2.0"
#r "SLFormat"
open SLFormat.Pretty

#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"

open FactX.Internal
open FactX.Internal.PrintProlog

let testRender (doc:Doc) : unit = 
    render 80 doc |> printfn "%s"


let test01 () = 
    let d1 = text "Hello" ^+^ text "world!"
    let d2 = text "***** ******"
    render 80 (indent 2 (d1 ^@@^ d2)) |> printfn "%s"

    let fact1 : Doc = 
        prologFact "address" [quotedAtom "UID001"; prologString "1, Yellow Brick Road" ]
    testRender fact1 

    let mdirective = 
        moduleDirective "os_relations" 
                        [ "osName", 2
                        ; "osType", 2
                        ; "odComment", 2
                        ]
    testRender mdirective 

let test02 () = 
    let doc1 = commaSep [text "one"; text "two"; text "three"]
    let doc2 = indent 10 doc1
    testRender doc1 
    testRender doc2

let test03 () = 
    let doc1 = indent 10 (text "start")
    testRender doc1

let test04 () = 
    vcat [text "start"; empty; text "end" ]
        |> testRender

// Temp - parsing signatures.

//let lexeme : Parser<string, 'u> = 
//    let opts = IdentifierOptions(isAsciiIdStart = isLetter)
//    identifier opts .>> spaces

//let lparen : Parser<unit, 'u> = (pchar '(') >>. spaces
//let rparen : Parser<unit, 'u> = (pchar ')') >>. spaces
//let comma : Parser<unit, 'u> = (pchar ',') >>. spaces
//let dot : Parser<unit, 'u> = (pchar '.') >>. spaces


//let pSignature : Parser<Signature, 'u> =
//    let body = between lparen rparen (sepBy lexeme comma)
//    pipe3 lexeme body dot (fun x xs _ -> Signature(x,xs))

//let test05 () = 
//    runParserOnString lexeme () "NONE" "identifier_one()."

//let test06 () = 
//    runParserOnString pSignature () "NONE" "identifier_one(blue, yellow)."

let test05 () = 
    testRender <| (PrologSyntax.PDecimal 1.078M).Format()

