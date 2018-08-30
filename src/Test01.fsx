﻿#load "FactX\Internal\FormatCombinators.fs"
open FactX.Internal.FormatCombinators



let test01 () = 
    let d1 = formatString "Hello" +^+ formatString "world!"
    let d2 = formatString "***** ******"
    render (indent 2 (d1 @@@ d2)) |> printfn "%s"

    let fact1 : Doc = 
        prologFact (formatString "address") 
                    [quotedAtom "UID001"; prologString "1, Yellow Brick Road" ]
    testRender fact1 

    let mdirective = 
        moduleDirective "os_relations" 
                        [ "osName", 2
                        ; "osType", 2
                        ; "odComment", 2
                        ]
    testRender mdirective 

let test02 () = 
    let doc1 = commaSepListVertically [formatString "one"; formatString "two"; formatString "three"]
    let doc2 = indent 10 doc1
    testRender doc1 
    testRender doc2

let test03 () = 
    let doc1 = indent 10 (formatString "start")
    testRender doc1

let test04 () = 
    vsep [formatString "start"; empty; formatString "end" ]
        |> testRender