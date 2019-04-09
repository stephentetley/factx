// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

[<AutoOpen>]
module FactOutput = 

    open FactX.Syntax



    type Signature = Signature of string * string list

    /// A Fact is a Functor with a signature
    type Fact = Predicate


    let fact (name:string) (terms:Term list) : Predicate =
        Predicate(SimpleAtom name, terms) 

    /// eg. true, false, none, null
    let simpleAtom (input:string) : Term = Atom (SimpleAtom input)

    let quotedAtom (input:string) : Term = Atom (QuotedAtom input)


    let stringTerm (input:string) : Term = 
        match input with 
        | null -> Literal (String "")
        | ss -> Literal (String ss)

    let intTerm (i:int64) : Term = Literal (Int i)

    let decimalTerm (d:decimal) : Term = Literal (Decimal d)


    /// ''
    let nullTerm : Term = Atom (QuotedAtom "")

    let functor (name:string) (elements:Term list) = 
        Functor(SimpleAtom name, elements)



    let moduleDirective (modName:string) (exports:string list) : Directive = 
        Directive(Functor(SimpleAtom "module", [simpleAtom modName; List (List.map simpleAtom exports)]))

