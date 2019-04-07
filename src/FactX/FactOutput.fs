// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

module FactOutput = 

    open FactX.Internal.Syntax



    type Signature = Signature of string * string list

    /// A Fact is a Functor with a signature
    type Fact = Predicate


    let fact (name:string) (terms:Term list) : Predicate =
        Predicate(SimpleAtom name, terms) 

    /// eg. true, false, none, null
    let simpleAtom (input:string) : Term = Atom (SimpleAtom input)

    let quotedAtom (input:string) : Term = Atom (QuotedAtom input)

    let prologString (input:string) : Term = 
        match input with 
        | null -> Literal (String "")
        | ss -> Literal (String ss)

    let prologDecimal (d:decimal) : Term = Literal (Decimal d)




    let moduleDirective (modName:string) (exports:string list) : Directive = 
        Directive(Functor(SimpleAtom "module", [simpleAtom modName; List (List.map simpleAtom exports)]))

