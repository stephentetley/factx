// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

[<AutoOpen>]
module FactOutput = 

    open System
    open FactX.Syntax

    type Signature = Signature of string * string list

    /// A Fact is a Functor with a signature
    type Fact = Predicate


    let fact (name:string) (terms:Term list) : Predicate =
        Predicate(SimpleAtom name, terms) 

    /// eg. true, false, none, null
    let simpleAtom (input:string) : Term = Atom (SimpleAtom input)

    let quotedAtom (input:string) : Term = Atom (QuotedAtom input)

    let charTerm (input:char) : Term = Literal (Char input)

    let stringTerm (input:string) : Term = 
        match input with 
        | null -> Literal (String "")
        | ss -> Literal (String ss)

    let intTerm (i:int) : Term = Literal (Int (int64 i))

    let int64Term (i:int64) : Term = Literal (Int  i)

    let decimalTerm (d:decimal) : Term = Literal (Decimal d)


    /// ''
    let nullTerm : Term = Atom (QuotedAtom "")


    /// Output date in ISO 8601 format
    /// e.g. 2006-12-08
    let dateTerm (value:DateTime) : Term = 
        stringTerm <| value.ToString(format = "yyyy-MM-dd")

    /// Output date-timein ISO 8601 format
    /// e.g. 2006-12-08T17:29:44
    let dateTimeTerm (value:DateTime) : Term = 
        stringTerm <| value.ToString("yyyy-MM-ddThh:mm:ss")


    let listTerm (elements:Term list) : Term = List elements

    let functor (name:string) (elements:Term list) : Term = 
        Functor(SimpleAtom name, elements)

    let dictTerm (name:string) (elements: (string * Term) list) : Term = 
        Dict(name, elements)


    let predicate (name:string) (elements:Term list) : Predicate = 
        Predicate(SimpleAtom name, elements)

    let moduleDirective (modName:string) (exports:string list) : Directive = 
        Directive(Functor(SimpleAtom "module", [simpleAtom modName; List (List.map simpleAtom exports)]))

