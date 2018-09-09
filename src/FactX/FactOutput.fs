// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX

open System
open System.IO

open FParsec

open FactX.Internal.FormatCombinators


module FactSignature = 
    open System.Runtime.Remoting.Metadata.W3cXsd2001
    open System.Runtime.Remoting.Metadata.W3cXsd2001


    type Signature = 
        | Signature of string * string list
        member v.Arity : int = match v with | Signature(_,xs) -> List.length xs
        member v.Name : string = match v with | Signature(x,_) -> x


    let exportSignature (signature:Signature) : string = 
        sprintf "%s/%d" signature.Name signature.Arity

    // Temp - parsing signatures.

    let lexeme : Parser<string, unit> = 
        let opts = IdentifierOptions(isAsciiIdStart = isLetter)
        identifier opts .>> spaces

    let lparen : Parser<unit, unit> = (pchar '(') >>. spaces
    let rparen : Parser<unit, unit> = (pchar ')') >>. spaces
    let comma : Parser<unit, unit> = (pchar ',') >>. spaces
    let dot : Parser<unit, unit> = (pchar '.') >>. spaces


    let pSignature : Parser<Signature,  unit> =
        let body = between lparen rparen (sepBy lexeme comma)
        pipe3 lexeme body dot (fun x xs _ -> Signature(x,xs))

    let parseSignature (source:string) : Signature = 
        match runParserOnString pSignature () "NONE" source with
        | Success(ans,_,_) -> ans
        | Failure(_,_,_) -> failwithf "Parsing failed on signature: '%s'" source
        

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

    /// Note - if we parsed/validated the signature we could save the user
    /// having to specify name and arity.
    type IFactHelper<'a> = 
        abstract Signature : string
        abstract ClauseBody : 'a -> Value list

    type FactSet = 
        { FactName: Identifier 
          Arity: int
          Signature: string
          Comment: string
          Clauses: Clause list }
        member v.Format : Doc = 
            let d1 = prologComment v.Signature
            let d2 = prologComment v.Comment
            let ds = List.map (fun (clause:Clause) -> clause.Format) v.Clauses
            vcat <| (d1 :: d2 :: ds)

        member v.ExportSignature = (v.FactName, v.Arity)
    
    let makeFactSet (helper:IFactHelper<'a>) (items:seq<'a>) : FactSet = 
        let signature = FactSignature.parseSignature helper.Signature
        let makeClause (item:'a) : Clause  = 
            { FactName = signature.Name
              Values = helper.ClauseBody item }
        { FactName  = signature.Name
          Arity     = signature.Arity
          Signature = helper.Signature 
          Comment   = "" 
          Clauses   = Seq.toList items |> List.map makeClause
        }
        


    /// Potentially this should be an object, not a record.
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

        member v.Format = 
            let d1 = prologComment v.GlobalComment
            let d2 = moduleDirective v.ModuleName v.Exports
            let ds = List.map (fun (col:FactSet) -> col.Format) v.Database
            vsep [ d1; d2; vsep ds ]

        member v.SaveToString () : string = 
            render v.Format
        
        member v.Save(filePath:string) = 
            use sw = new System.IO.StreamWriter(filePath)
            sw.Write (render v.Format)


