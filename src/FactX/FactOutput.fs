// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX

open System
open System.IO

open FactX.Internal.FormatCombinators


[<AutoOpen>]
module FactOutput = 

    type Identifier = string

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

    type Clause = 
        { FactName: Identifier
          Values : Value list }
        member v.Format = 
            prologFact (simpleAtom v.FactName) 
                        (List.map (fun (x:Value) -> x.Format) v.Values)

    type FactSet = 
        { FactName: Identifier 
          Arity: int
          Signature: string
          Clauses: Clause list }
        member v.Format = 
            let d1 = prologComment v.Signature
            let ds = List.map (fun (clause:Clause) -> clause.Format) v.Clauses
            vcat <| (d1 :: ds)


    type Module = 
        { ModuleName: Identifier
          GlobalComment: string
          Exports: (Identifier * int) list
          Database: FactSet list }
        member v.Format = 
            let d1 = prologComment v.GlobalComment
            let d2 = moduleDirective v.ModuleName v.Exports
            let ds = List.map (fun (col:FactSet) -> col.Format) v.Database
            vcat <| (d1 :: empty :: d2 :: empty :: ds)

        member v.SaveToString () : string = 
            render v.Format
        member v.Save(filePath:string) = 
            use sw = new System.IO.StreamWriter(filePath)
            sw.Write (render v.Format)

    /// get the export signature for a Fact
    let factSignature (fc:FactSet) : Identifier * int = 
        fc.FactName, fc.Arity