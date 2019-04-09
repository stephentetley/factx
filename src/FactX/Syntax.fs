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




    // ****************************************************
    // Pretty printing

    let private commaSep (docs:Doc list) = foldDocs (fun ac e -> ac ^^ comma ^/^ e) docs



        
    let ppSimpleAtom (value:string) : Doc = text value

    // This must escape.
    let ppQuotedAtom (value:string) : Doc = 
        text <| sprintf "'%s'" (escapeSpecial value)

    let ppChar (value:char) : Doc =  text <| sprintf "0'%c" value

    
    let ppString (value:string) : Doc = 
        text <| sprintf "\"%s\"" (escapeSpecial value)
    
    let ppBool (value:bool) : Doc = 
        text <| if value then "true" else "false"

    let ppInt (i:int64) : Doc = text <| i.ToString()


    let ppFloat (d:float) : Doc = text <| d.ToString()

    let ppDouble (d:double) : Doc = text <| d.ToString()
    
    let ppDecimal (d:decimal) : Doc = 
        // Ensure Prolog printing renders to a decimal string.
        text <| let d1 = 0.0M + d in d1.ToString()


    let ppIdentifier (name:Identifier) : Doc = text name

    let private ppFunctor (head:Doc) (body:Doc list) : Doc =
        nest 4 (head ^^ lparen ^//^ commaSep body ^^ rparen)

    /// Print vertically
    let private ppList (docs:Doc list) : Doc = 
        enclose lbracket rbracket  <| foldDocs (fun x y -> x ^^ comma ^@@^ y) docs



    let private ppDict (tag:Doc) (docs:Doc list) : Doc = 
        let body = foldDocs (fun x y -> x ^^ comma ^@@^ y) docs
        tag ^^ enclose lbrace rbrace body


    let ppLiteral (literal:Literal) : Doc = 
        match literal with
        | Char c -> ppChar c
        | String s -> ppString s
        | Int i -> ppInt i
        | Decimal d -> ppDecimal d

    let ppAtom (atom:Atom) : Doc  = 
        match atom with
        | SimpleAtom s -> ppSimpleAtom s
        | QuotedAtom s -> ppQuotedAtom s

    let ppTerm (term:Term) : Doc  = 
        let rec work (t1:Term) (cont:Doc -> Doc) : Doc =
            match t1 with
            | Literal x -> cont (ppLiteral x)
            | Atom x -> cont (ppAtom x)
            | Variable v -> cont (text v)
            | Functor(a1,xs) -> 
                let name = ppAtom a1
                workList xs (fun vs -> 
                cont (ppFunctor name vs))
            | List xs -> 
                workList xs (fun vs -> 
                cont (ppList vs))
            | Dict(tag,xs) -> 
                workList2 xs (fun vs -> 
                cont (ppDict (ppIdentifier tag) vs))
        and workList (terms: Term list) (cont:Doc list -> Doc) : Doc =
            match terms with
            | [] -> cont []
            | x :: rest -> 
                work x (fun v1 -> 
                workList rest (fun vs -> 
                cont (v1::vs)))
        and workList2 (terms: (Identifier * Term) list) (cont:Doc list -> Doc) : Doc =
            match terms with
            | [] -> cont []
            | (name,x) :: rest -> 
                work x (fun v1 -> 
                let pair = ppIdentifier name ^^ colon ^^ v1
                workList2 rest (fun vs -> 
                cont (pair::vs)))
        work term (fun x -> x)

    let ppPredicate (pred:Predicate) : Doc = 
        match pred with
        | Predicate(a1,[]) -> ppAtom a1
        | Predicate(a1,xs) -> 
            ppAtom a1 ^^ tupled (List.map ppTerm xs)



    let ppClause (clause:Clause) : Doc = 
        match clause with
        | Clause(p1,[]) -> ppPredicate p1 ^^ dot
        | Clause(p1,xs) -> 
            ppPredicate p1 ^+^ text ":-" ^+^ commaSep (List.map ppPredicate xs)


    let ppComment (source:string) : Doc = 
        let lines = source.Split([| "\r\n"; "\r"; "\n" |], StringSplitOptions.None) |> Array.toList
        vcat <| List.map (fun s -> text (sprintf "%c %s" '%' s)) lines

    let ppDirective (source:Directive) : Doc = 
        match source with
        | Directive(term) -> 
            text ":-" ^+^ ppTerm term