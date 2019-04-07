// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"
open FParsec

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"
open SLFormat.Pretty


#load "..\src\FactX\Internal\Syntax.fs"
#load "..\src\FactX\FactOutput.fs"

open FactX.Internal.Syntax
open FactX.FactOutput

let testRender (doc:Doc) : unit = 
    render 80 doc |> printfn "%s"


let test01 () = 
    let d1 = text "Hello" ^+^ text "world!"
    let d2 = text "***** ******"
    render 80 (indent 2 (d1 ^@@^ d2)) |> printfn "%s"

    let fact1 : Fact = 
        fact "address" [quotedAtom "UID001"; prologString "1, Yellow Brick Road" ]
    testRender (ppPredicate fact1 )

    let mdirective = 
        moduleDirective "os_relations" 
                        [ "osName/2"
                        ; "osType/2"
                        ; "odComment/2"
                        ]
    testRender (ppDirective mdirective)

let test02 () = 
    let doc1 = commaList [text "one"; text "two"; text "three"]
    let doc2 = indent 10 doc1
    testRender doc1 
    testRender doc2

let test03 () = 
    let doc1 = indent 10 (text "start")
    testRender doc1

let test04 () = 
    vcat [text "start"; empty; text "end" ]
        |> testRender

let test05 () = 
    testRender <| ppLiteral (Decimal 1.078M)

let test06 () = 
    testRender <| ppTerm (List [Literal (Decimal 1.078M)])


let test07 () = 
    List [Literal (Decimal 1.078M)] |> printfn "%O"