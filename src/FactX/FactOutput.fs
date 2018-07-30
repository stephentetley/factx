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
    member v.Pretty = 
        match v with
        | PString s -> prologString s
        | PInt i -> formatInt i
        | PDouble d -> formatDouble d
        | PQuotedAtom s -> quotedAtom s
        | PList vs -> prologList (List.map (fun (v:Value) -> v.Pretty) vs)

type Fact = 
    { FactName: string
      FactValues : Value list }
    member v.Pretty = 
        prologFact (simpleAtom v.FactName) 
                    (List.map (fun (v:Value) -> v.Pretty) v.FactValues)

