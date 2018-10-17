// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

// Note we should probably consider SWI-Prolog's tables
// if we need to look towards efficiency.


namespace FactX.Internal

open System
open System.IO

open FParsec

open FactX.Internal.PrettyPrint
open FactX.Internal.PrintProlog


[<AutoOpen>]
module FactSignature = 

    // If we want to allow function symbols / nesting we could encode them as
    // name, arity, parens(ellipsis), e.g. "phone_number/1(..)".
    // Thus we can get arity.

    type Signature = 
        | Signature of string * string list
        member v.Arity : int = match v with | Signature(_,xs) -> List.length xs
        member v.Name : string = match v with | Signature(x,_) -> x
        override v.ToString() = 
            match v with 
            | Signature(name,args) -> sprintf "%s(%s)." name (String.concat ", " args)
            


    let exportSignature (signature:Signature) : string = 
        sprintf "%s/%d" signature.Name signature.Arity

    // Parsing signatures.

    let private lexeme : Parser<string, unit> = 
        let opts = IdentifierOptions(isAsciiIdStart = isLetter)
        identifier opts .>> spaces

    let private lparen : Parser<unit, unit> = (pchar '(') >>. spaces
    let private rparen : Parser<unit, unit> = (pchar ')') >>. spaces
    let private comma : Parser<unit, unit> = (pchar ',') >>. spaces
    let private dot : Parser<unit, unit> = (pchar '.') >>. spaces


    let private pSignature : Parser<Signature,  unit> =
        let body = between lparen rparen (sepBy lexeme comma)
        pipe3 lexeme body dot (fun x xs _ -> Signature(x,xs))

    let parseSignature (source:string) : Signature = 
        match runParserOnString pSignature () "NONE" source with
        | Success(ans,_,_) -> ans
        | Failure(_,_,_) -> failwithf "Parsing failed on signature: '%s'" source

    let tryParseSignature (source:string) : option<Signature> = 
        match runParserOnString pSignature () "NONE" source with
        | Success(ans,_,_) -> Some ans
        | Failure(_,_,_) -> None
        

[<RequireQualifiedAccess>]
module PrologSyntax = 

    /// Note - this syntax favours output not creation.

    /// TODO - should Format take a thunk like ToString() ?

    type Identifier = string

    /// Note - Sequences/tuples not represented (should they be?)
    type Value = 
        | PChar of char
        | PString of string
        | PInt of int
        | PDecimal of decimal
        | PQuotedAtom of string
        | PList of Value list
        | PFunctor of Identifier * Value list
        member v.Format () = 
            match v with
            | PChar c -> prologChar c
            | PString s -> prologString s
            | PInt i -> prologInt i
            | PDecimal d -> prologDecimal d
            | PQuotedAtom s -> quotedAtom s
            | PList vs -> prologList (List.map (fun (x:Value) -> x.Format()) vs)
            | PFunctor(name, vs) -> prologFunctor name (List.map (fun (x:Value) -> x.Format()) vs)


    /// To consider...
    /// If Clause rather than FactSet had a signature we could add
    /// clauses to a (more-or-less opaque) factbase. 
    /// This would let factbase creating traversals to easily add 
    /// variously typed Clauses to the factbase.
    type Clause = 
        { FactName: Identifier
          Values : Value list }
        member v.Format () = 
            prologFact v.FactName (List.map (fun (x:Value) -> x.Format()) v.Values)


    type FactSet = 
        { FactName: Identifier 
          Arity: int
          Signature: string
          Comment: string
          Clauses: Clause list }
        member v.Format () : Doc = 
            let d1 = prologComment v.Signature
            let d2 = prologComment v.Comment
            let ds = List.map (fun (clause:Clause) -> clause.Format()) v.Clauses
            d1 ^@@^ d2 ^@@^ vcat ds

        member v.ExportSignature = (v.FactName, v.Arity)
    
   


    /// This an object, not a record so it can have different constructors
    type Module = 
        val ModuleName : string
        val GlobalComment : string
        val Exports : (Identifier * int) list
        val Database : FactSet list
        new (name:string, comment:string, db:FactSet list) = 
            { ModuleName = name
            ; GlobalComment = comment
            ; Exports = db |> List.map (fun a -> a.ExportSignature)
            ; Database = db }

        new (name:string, db:FactSet list) = 
            { ModuleName = name
            ; GlobalComment = sprintf "%s.pl" name
            ; Exports = db |> List.map (fun a -> a.ExportSignature)
            ; Database = db }

        new (name:string, comment:string, db:FactSet) = 
            { ModuleName = name
            ; GlobalComment = comment
            ; Exports = [db.ExportSignature]
            ; Database = [db] }
        
        new (name:string, db:FactSet) = 
            { ModuleName = name
            ; GlobalComment = sprintf "%s.pl" name
            ; Exports = [db.ExportSignature]
            ; Database = [db] }

        member v.Format () = 
            let d1 = prologComment v.GlobalComment
            let d2 = moduleDirective v.ModuleName v.Exports
            let ds = List.map (fun (col:FactSet) -> col.Format()) v.Database
            vcat [ d1; empty; d2; empty; vcat ds ]

        member v.ToProlog () : string = 
            render 160 <| v.Format()
        
        member v.Save (lineWidth:int, filePath:string) = 
            writeDoc lineWidth filePath (v.Format())


