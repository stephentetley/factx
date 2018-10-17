// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause


#load "..\src\FactX\Internal\Lindig.fs"
open FactX.Internal.Lindig

let runTest (doc:Doc) : unit = printfn "%s\n" <| render  80 doc
let runTestW (width:int) (doc:Doc) : unit = printfn "%s\n" <| render width doc

let test01 () = 
    let d1:Doc = text "tree" in runTest d1

let test02 () = 
    let d1:Doc = text "hello" ^+^ text "world" in runTest d1

let binop (left:Doc) (op:Doc) (right:Doc) : Doc = 
    group (nest 2 (group (left ^+^ op ^+^ right)))


let cond = binop (text "a") (text "==") (text "b")
let expr1 = binop (text "a") (text "<<") (text "2")
let expr2 = binop (text "a") (text "+") (text "b")

let ifthen c e1 e2 = 
    group ( group (nest 2 (text "if" ^+^ c))
         ^+^ group (nest 2 (text "then" ^+^ e1))
         ^+^ group (nest 2 (text "else" ^+^ e2)))

let test03 (width:int) : unit = 
    let doc = ifthen cond expr1 expr2 
    runTestW width doc

