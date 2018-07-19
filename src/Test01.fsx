#load "FactX\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
open FactX.FormatCombinators
open FactX.FactOutput


let demo01 () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", @"data\facts.pl")
    let proc1 : FactOutput<unit> = 
        factOutput {
            let! _ = tell <| comment "facts.pl"
            let! _ = tell <| comment "At prompt type 'make.' to reload"
            let! _ = 
                tell <| fact (simpleAtom "address") 
                                [quotedAtom "UID001"; prologString "1, Yellow Brick Road"; int 0 ]
            let! _ = 
                tell <| fact (simpleAtom "address") 
                                [quotedAtom "UID005"; prologString "15, Giants Causeway"; int 15 ]
            return () 
            }
    runFactOutput outFile proc1

let test01 () = 
    let d1 = string "Hello" +^+ string "world!"
    let d2 = string "***** ******"
    render (indent 2 (d1 @@@ d2)) |> printfn "%s"

    let fact1 : Doc = 
        fact (string "address") 
            [quotedAtom "UID001"; prologString "1, Yellow Brick Road" ]
    render fact1 |> printfn "%s"

    let mdirective = 
        moduleDirective "os_relations" 
                        [ "osName", 2
                        ; "osType", 2
                        ; "odComment", 2
                        ]
    render mdirective |> printfn "%s"

let test02 () = 
    let doc1 = commaSepListVertically [string "one"; string "two"; string "three"]
    let doc2 = indent 10 doc1
    render doc1 |> printfn "%s"
    render doc2 |> printfn "%s"

let test03 () = 
    let doc1 = indent 10 (string "start")
    render doc1 |> printfn "-----\n%s"

