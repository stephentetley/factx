// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

module FactX.FormatCombinators


open System.Text
open System
open System

/// This is not a "pretty printer" as it makes no effort to "fit" the output.
type Doc = 
    | Doc of string
    | HCat of Doc * Doc
    | VCat of Doc * Doc
    | Indent of int * Doc
    


let render (source:Doc) : string = 
    let rec work (doc:Doc) (indent:int) (sb:StringBuilder) : StringBuilder = 
        match doc with
        | Doc str -> sb.Append(str)
        | HCat(d1,d2) -> 
            let sb1 = work d1 indent sb in work d2 indent sb1
        | VCat(d1,d2) -> 
            let sb1 = sb.Append(String.replicate indent " ")
            let sb2 = work d1 indent sb1
            let sb3 = sb2.Append("\n" + String.replicate indent " ")
            work d2 indent sb3
        | Indent(i,d1) -> 
            work d1 (indent + i) sb
    work source 0 (new StringBuilder()) |> fun sb -> sb.ToString()

    


    