// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause


namespace FactX.Internal.Syntax2


[<AutoOpen>]
module Syntax2 = 
        
    open System

    open SLFormat.Pretty            // Lib: sl-format

    type Identifier = string

    /// Note - Sequences/tuples not represented (should they be?)
    type Literal = 
        | Char of char
        | String of string
        | Int of int64
        | Decimal of decimal

    type Atom = 
        | Atom of string
        | QuotedAtom of string

    type Term = 
        | TLiteral of Literal
        | TAtom of Atom
        | TVariable of Identifier
        | TFunctor of Atom * Term list
        | TList of Term list


    type Predicate = Predicate of Atom * Term list

    type Clause = Clause of Predicate * Predicate list


    type Directive = Directive of Identifier * Term list




    // ****************************************************

    let private commaSep (docs:Doc list) = foldDocs (fun ac e -> ac ^^ comma ^/^ e) docs

    // TODO 
    //Not sure this is right / complete.
    let private escapeSpecial (source:string) : string = 
        let s1 = source.Replace("\\" , "\\\\")
        let s2 = s1.Replace("'", "\\'")
        s2

        
    let simpleAtom (value:string) : Doc = text value

    // This must escape.
    let quotedAtom (value:string) : Doc = 
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

    let private prologFunctor (head:Doc) (body:Doc list) : Doc =
        nest 4 (head ^^ lparen ^//^ commaSep body ^^ rparen)

    /// Print vertically
    let private prologList (docs:Doc list) : Doc = 
        enclose lbracket rbracket  <| foldDocs (fun x y -> x ^^ comma ^@@^ y) docs

    let ppLiteral (literal:Literal) : Doc = 
        match literal with
        | Char c -> ppChar c
        | String s -> ppString s
        | Int i -> ppInt i
        | Decimal d -> ppDecimal d

    let ppAtom (atom:Atom) : Doc  = 
        match atom with
        | Atom s -> simpleAtom s
        | QuotedAtom s -> quotedAtom s

    let ppTerm (term:Term) : Doc  = 
        let rec work (t1:Term) (cont:Doc -> Doc) : Doc =
            match t1 with
            | TLiteral x -> cont (ppLiteral x)
            | TAtom x -> cont (ppAtom x)
            | TVariable v -> cont (text v)
            | TFunctor(a1,xs) -> 
                let name = ppAtom a1
                workList xs (fun vs -> 
                cont (prologFunctor name vs))
            | TList xs -> 
                workList xs (fun vs -> 
                cont (prologList vs))
        and workList (terms: Term list) (cont:Doc list -> Doc) : Doc =
            match terms with
            | [] -> cont []
            | x :: rest -> 
                work x (fun v1 -> 
                workList rest (fun vs -> 
                cont (v1::vs)))
        work term (fun x -> x)

    let ppPredicate (pred:Predicate) : Doc = 
        match pred with
        | Predicate(a1,[]) -> ppAtom a1
        | Predicate(a1,xs) -> 
            ppAtom a1 ^^ commaList (List.map ppTerm xs)



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
        | Directive(name, body) -> 
            text ":-" ^+^ text name ^^ parens (commaSep <| List.map ppTerm body)