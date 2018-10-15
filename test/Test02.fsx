// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\YC.PrettyPrinter.0.0.5\lib\net40"
#r "YC.PrettyPrinter"
open YC.PrettyPrinter.Pretty
open YC.PrettyPrinter.Doc
open YC.PrettyPrinter.StructuredFormat

#load "..\src\FactX\Internal\PrintProlog.fs"
open FactX.Internal.PrintProlog


let runTest (doc:Doc) : unit = printfn "%s" <| print 80 doc

let test01 () = 
    let d1:Doc = simpleAtom "tree" in runTest d1

let test02 () = 
    runTest <| prologList [simpleAtom "tree"; quotedAtom "trunk"]

let test03 () = 
    runTest <| prologComment "Rimp\nRamp\nRomp"

let test04 () = 
    runTest <| prologFact (simpleAtom "plant") [simpleAtom "cactus"; quotedAtom "succulent"]

let test05 () = 
    let xs = 
        [ sepL "name" ^^ sepL "()."
        ; wordL "name" ^^ wordL "()." 
        ; sepL "name" ++ sepL "()."
        ; wordL "name" ++ wordL "()." 
        ; sepL "name" -- sepL "()."
        ; wordL "name" -- wordL "()." 
        ; sepL "name" @@ sepL "()."
        ; wordL "name" @@ wordL "()." 
        ]
    List.iter runTest xs

let test06 () = 
    runTest <| wordL "name" ^^ wordL "()."