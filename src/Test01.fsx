#load "FactX\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
open FactX.FormatCombinators
open FactX.FactOutput


let demo01 () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", @"data\facts.pl")
    let proc1 : FactOutput<unit> = 
        factOutput {
            let! _ = tellComment "facts.pl"
            let! _ = tellComment "At prompt type ``make.`` to reload"
            let! _ = tellFact (namedAtom "address") [quotedAtom "UID001"; string "1, Yellow Brick Road"; int 0 ]
            let! _ = tellFact (namedAtom "address") [quotedAtom "UID005"; string "15, Giant Causeway"; int 15 ]
            return () 
            }
    runFactOutput outFile proc1

let test01 () = 
    let d1 = HCat(Doc "Hello", HCat(Doc " ", Doc "world!"))
    let d2 = Doc "***** ******"
    render (Indent(2,VCat(d1,d2))) |> printfn "%s"


        


