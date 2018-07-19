// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

module FactX.FormatCombinators


open System.Text
open System
open System

/// This is not a "pretty printer" as it makes no effort to "fit" the output.
type Doc = 
    | Empty
    | Doc of string
    | HDoc of Doc * Doc
    | VDoc of Doc * Doc
    | Indent of int * Doc
    


let render (source:Doc) : string = 
    let rec work (doc:Doc) (indent:int) (sb:StringBuilder) : StringBuilder = 
        match doc with
        | Empty -> sb
        | Doc str -> sb.Append(str)
        | HDoc(d1,d2) -> 
            let sb1 = work d1 indent sb in work d2 indent sb1
        | VDoc(d1,d2) -> 
            let sb1 = work d1 indent sb
            let sb2 = sb1 // sb1.Append(String.replicate indent " ")
            let sb3 = sb2.Append("\n" + String.replicate indent " ")
            work d2 indent sb3
        | Indent(i,d1) -> 
            let sb1 = sb.Append(String.replicate i " ")
            work d1 (indent + i) sb1
    work source 0 (new StringBuilder()) |> fun sb -> sb.ToString()

    
// *************************************
// Primitive values

let empty : Doc = 
    Empty


let bool (value:bool) : Doc = 
    Doc <| if value then "true" else "false"


let int (i:int) : Doc = 
    Doc(i.ToString())


let char (ch:char) : Doc = 
    Doc (ch.ToString())

/// TODO - string escaping...
let string (value:string) : Doc = 
    Doc <| value

let singleQuoted (value:string) : Doc = 
    Doc << sprintf "'%s'" <| value.Replace("'","''")

let doubleQuoted (value:string) : Doc = 
    Doc <| sprintf "\"%s\"" value


/// A single space
let space = Doc " "

let dot = char '.'
let comma = char ','



// *************************************
// Combinators

let indent (i:int) (d:Doc) = 
    Indent(i,d)

/// Horizontal concat
let (+++) (d1:Doc) (d2:Doc) : Doc = 
    HDoc(d1,d2)

/// Horizontal concat with a separating space 
let (+^+) (d1:Doc) (d2:Doc) : Doc = 
   d1 +++ space +++ d2


/// Vertical concat
let (@@@) (d1:Doc) (d2:Doc) : Doc = 
    VDoc(d1,d2)

/// Vertical concat with a separating blank line 
let (@^@) (d1:Doc) (d2:Doc) : Doc = 
   (d1 @@@ empty) @@@ d2


let concat (operator:Doc -> Doc -> Doc) (source:Doc list) : Doc = 
    let rec work (ac:Doc) (xs:Doc list) : Doc = 
        match xs with 
        | [] -> ac
        | [y] -> operator ac y
        | y :: ys -> work (operator ac y) ys
    match source with 
    | [] -> Empty
    | x :: xs -> work x xs


let hcat (source:Doc list) : Doc = concat (+++) source
let hsep (source:Doc list) : Doc = concat (+^+) source
    
let vcat (source:Doc list) : Doc = concat (@@@) source


let punctuate (sep:Doc) (source:Doc list) : Doc = 
    let rec work (ac:Doc) (xs:Doc list) : Doc = 
        match xs with 
        | [] -> ac
        | [y] -> (ac +++ sep) +++ y
        | y :: ys -> work ((ac +++ sep) +++ y) ys
    match source with 
    | [] -> Empty
    | x :: xs -> work x xs

let punctuateVertically (sep:Doc) (source:Doc list) : Doc = 
    let rec work (ac:Doc) (xs:Doc list) : Doc = 
        match xs with 
        | [] -> ac
        | [y] -> (ac +++ sep) @@@ y
        | y :: ys -> work ((ac +++ sep) @@@ y) ys
    match source with 
    | [] -> Empty
    | x :: xs -> work x xs

let parens (d:Doc) : Doc = 
    (char '(' +++ d) +++ char ')'

let angles (d:Doc) : Doc = 
    (char '<' +++ d) +++ char '>'

let squares (d:Doc) : Doc = 
    (char '[' +++ d) +++ char ']'

let braces (d:Doc) : Doc = 
    (char '{' +++ d) +++ char '}'


let tupled (source:Doc list) : Doc = 
    parens (punctuate (string ", ") source)

let commaSepList (source: Doc list) : Doc = 
    squares (punctuate (string ", ") source)

let semiSepList (source: Doc list) : Doc = 
    squares (punctuate (string "; ") source)


let commaSepListVertically (source:Doc list) : Doc = 
    squares (punctuateVertically (char ',') source)


// *************************************
// Prolog specific

let simpleAtom (value:string) : Doc = string value

let quotedAtom (value:string) : Doc = singleQuoted value

let prologString (value:string) : Doc = doubleQuoted value

let comment (comment:string) : Doc = 
    let lines = comment.Split [|'\n'|] |> Array.toList
    vcat <| List.map (fun s -> char '%' +^+ string s) lines

let fact (head:Doc) (body:Doc list) : Doc = 
    head +++ tupled body

let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
    let exportList = 
        commaSepListVertically <| List.map (fun (s,i) -> string (sprintf "%s/%i" s i)) exports
    string ":- module" +++ parens ((string moduleName +++ comma) @@@ (indent 10 exportList)) +++ dot
    
