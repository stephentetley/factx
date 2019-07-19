// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190712\lib\netstandard2.0"
#r "SLFormat"
open SLFormat.Pretty



let runTest (doc:Doc) : unit = printfn "%s\n" <| render  80 doc
let runTestW (width:int) (doc:Doc) : unit = printfn "%s\n" <| render width doc

let test01 () = 
    let d1:Doc = text "tree" in runTest d1

let test02 () = 
    let d1:Doc = text "hello" ^+^ text "world" in runTest d1

let binop (left:Doc) (op:Doc) (right:Doc) : Doc = 
    group (nest 2 (group (left ^/^ op ^/^ right)))


let cond = binop (text "a") (text "==") (text "b")
let expr1 = binop (text "a") (text "<<") (text "2")
let expr2 = binop (text "a") (text "+") (text "b")

let ifthen c e1 e2 = 
    group ( group (nest 2 (text "if" ^/^ c))
         ^/^ group (nest 2 (text "then" ^/^ e1))
         ^/^ group (nest 2 (text "else" ^/^ e2)))

let test03 (width:int) : unit = 
    let doc = ifthen cond expr1 expr2 
    runTestW width doc

let test04 (ntimes:int) = 
    let s = "orange, blue, white, black"
    let doc = text s ^/^ text s
    runTestW 35 (vcat <| List.replicate ntimes doc)

let test05 () = 
     runTestW 80 <| text "one" ^!!^ text "two"

let test05a () = 
     runTestW 80 <| text "one" ^!^ text "two"

let test06 () = 
     runTestW 80 <| text "one" ^!!^ empty ^!!^ text "two"

type Tree = 
    | Node of int
    | Tree of Tree list

let tree1 : Tree = Tree [Node 1; Node 2]


/// Bad - not tail recursive...
let treeSum (v:Tree) : int = 
    let rec work (term:Tree) (acc:int) : int = 
        match term with 
        | Node i -> i + acc
        | Tree xs -> workList xs acc
    and workList (terms:Tree list) (acc:int) : int = 
        match terms with
        | [] -> acc
        | x :: xs -> let acc1 = work x acc in workList xs acc1
    work v 0

let test07 () = 
    treeSum tree1

let treeSumCPS (v:Tree) : int = 
    let rec work (acc:int) (term:Tree) (cont : int -> int) : int = 
        match term with 
        | Node i -> cont (i + acc)
        | Tree xs -> workList acc xs cont 
    and workList (acc:int) (terms:Tree list) (cont:int -> int) : int = 
        match terms with
        | [] -> cont acc
        | x :: xs ->  
            work acc x (fun acc1 -> 
            workList acc1 xs cont)
    work 0 v (fun x -> x)

let test07b () = 
    treeSumCPS tree1


let treeLeaves (v:Tree) : int list = 
    let rec work (acc:int list) (term:Tree) (cont : int list -> int list) : int list = 
        match term with 
        | Node i -> cont (i::acc)
        | Tree xs -> workList acc xs cont 
    and workList (acc:int list) (terms:Tree list) (cont:int list -> int list) : int list = 
        match terms with
        | [] -> cont acc
        | x :: xs ->  
            work acc x (fun acc1 -> 
            workList acc1 xs cont)
    work [] v (fun xs -> List.rev xs)

let test08 () = 
    treeLeaves tree1


let treeFormat (source:Tree) : Doc = 
    printfn "Value.Format"
    let rec work (acc:Doc) (term:Tree) (cont:Doc -> Doc) : Doc =
        printfn "work term=%O" term
        match term with
        | Node i -> let doc1 = text <| sprintf "Node:%i" i in cont (acc ^!^ doc1)
        | Tree xs -> 
            workList [] xs (fun d1 -> 
            cont (acc ^!^ d1))
    and workList (acc:Doc List) (terms: Tree list) (cont:Doc -> Doc) : Doc =
        match terms with
        | [] -> cont (List.rev acc |> semiList)
        | x :: xs -> 
            work empty x (fun v1 -> 
            workList (v1::acc) xs cont)
    work empty source (fun x -> x)


let testZZ () = 
    runTest <| treeFormat tree1