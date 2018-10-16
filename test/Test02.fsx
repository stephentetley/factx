// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause



#load "..\src\FactX\Internal\PrettyPrint.fs"
#load "..\src\FactX\Internal\PrintProlog.fs"
open FactX.Internal
open FactX.Internal.PrettyPrint

let runTest (doc:Doc) : unit = printfn "%s\n" <| renderPretty 0.8 80 doc
let runTestW (width:int) (doc:Doc) : unit = printfn "%s\n" <| renderPretty 0.8 width doc

let test01 () = 
    let d1:Doc = text "tree" in runTest d1


//let cond = binop (text "a") (text "==") (text "b")
//let expr1 = binop (text "a") (text "<<") (text "2")
//let expr2 = binop (text "a") (text "+") (text "b")

//let ifthen c e1 e2 = 
//    group ( group (nest 2 (text "if" ^| c))
//         ^| group (nest 2 (text "then" ^| e1))
//         ^| group (nest 2 (text "else" ^| e2)))

//let test02 () = 
//    let doc = ifthen cond expr1 expr2 
//    runTestW 80 doc
//    runTestW 20 doc




//let test02 () = 
//    runTest <| prologList [simpleAtom "tree"; quotedAtom "trunk"]

//let test03 () = 
//    runTest <| prologComment "Rimp\nRamp\nRomp"

//let test04 () = 
//    runTest <| prologFact (simpleAtom "plant") [simpleAtom "cactus"; quotedAtom "succulent"]

//let test05 () = 
//    let xs = 
//        [ sepL "name" ^^ sepL "()."
//        ; wordL "name" ^^ wordL "()." 
//        ; sepL "name" ++ sepL "()."
//        ; wordL "name" ++ wordL "()." 
//        ; sepL "name" -- sepL "()."
//        ; wordL "name" -- wordL "()." 
//        ; sepL "name" @@ sepL "()."
//        ; wordL "name" @@ wordL "()." 
//        ]
//    List.iter runTest xs

//let test06 () = 
//    runTest <| wordL "name" ^^ wordL "()."