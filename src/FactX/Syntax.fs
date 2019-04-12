// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause


namespace FactX


[<AutoOpen>]
module Syntax = 
        
    open System

    open SLFormat.Pretty            // Lib: sl-format
    open FactX.Internal.Common


    type Identifier = string

    /// Note - Sequences/tuples not represented (should they be?)
    type Literal = 
        | Char of char
        | String of string
        | Int of int64
        | Decimal of decimal

    type Atom = 
        | SimpleAtom of string
        | QuotedAtom of string

    type Term = 
        | Literal of Literal
        | Atom of Atom
        | Variable of Identifier
        | Functor of Atom * Term list
        | List of Term list
        | Dict of Identifier * (Identifier * Term) list


    type Predicate = Predicate of Atom * Term list

    type Clause = Clause of Predicate * Predicate list


    type Directive = Directive of Term

