#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\Internal\FactWriter.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal.FactWriter


let demo01 () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", @"data\facts.pl")
    let proc1 : FactWriter<unit> = 
        factWriter {
            let! _ = tell <| comment "facts.pl"
            let! _ = tell <| comment "At prompt type 'make.' to reload"
            let! _ = 
                tell <| prologFact (simpleAtom "address") 
                                [quotedAtom "UID001"; prologString "1, Yellow Brick Road"; formatInt 0 ]
            let! _ = 
                tell <| prologFact (simpleAtom "address") 
                                [quotedAtom "UID005"; prologString "15, Giants Causeway"; formatInt 15 ]
            return () 
            }
    runFactWriter outFile proc1

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

