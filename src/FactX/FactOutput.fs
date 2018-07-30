// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause


module FactX.FactOutput

open System
open System.IO

open FactX.Internal.FormatCombinators


/// Note - Sequences/tuples not represented (should they be?)
type Value = 
    | PString of string
    | PInt of int
    | PDouble of double
    | PQuotedAtom of string
    | PList of Value list
    member v.Format = 
        match v with
        | PString s -> prologString s
        | PInt i -> formatInt i
        | PDouble d -> formatDouble d
        | PQuotedAtom s -> quotedAtom s
        | PList vs -> prologList (List.map (fun (x:Value) -> x.Format) vs)

type Fact = 
    { FactName: string
      FactValues : Value list }
    member v.Format = 
        prologFact (simpleAtom v.FactName) 
                    (List.map (fun (x:Value) -> x.Format) v.FactValues)

type FactCollection = 
    { Name: string 
      Arity: int
      Signature: string
      Facts: Fact list }
    member v.Format = 
        let d1 = prologComment v.Signature
        let ds = List.map (fun (fact:Fact) -> fact.Format) v.Facts
        vcat <| (d1 :: ds)

let private genModuleDecl (moduleName:string) (factCols:FactCollection list) : Doc = 
    let pairs1 (fcol:FactCollection) = (fcol.Name, fcol.Arity)
    let allPairs = List.map pairs1 factCols
    moduleDirective moduleName allPairs

type Module = 
    { ModuleName: string
      GlobalComment: string
      FactCols: FactCollection list }
    member v.Format = 
        let d1 = prologComment v.GlobalComment
        let d2 = genModuleDecl v.ModuleName v.FactCols
        let ds = List.map (fun (col:FactCollection) -> col.Format) v.FactCols
        vcat <| (d1 :: empty :: d2 :: empty :: ds)

    member v.SaveToString () : string = render v.Format
    member v.Save(filePath:string) = 
        use sw = new System.IO.StreamWriter(filePath)
        sw.Write (render v.Format)