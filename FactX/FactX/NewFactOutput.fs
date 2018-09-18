// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX

open System
open System.IO

open FParsec

open FactX.Internal.FormatCombinators
open FactX.Internal


module FactSignature = 


    type Signature = 
        | Signature of string * string list
        member v.Arity : int = match v with | Signature(_,xs) -> List.length xs
        member v.Name : string = match v with | Signature(x,_) -> x


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


[<AutoOpen>]
module FactOutput = 

    type ClauseBody = PrologSyntax.Value list

    type Clause = 
        { Signature: FactSignature.Signature
          Body : ClauseBody }
        member v.ToProlog() : PrologSyntax.Clause = 
            { FactName = v.Signature.Name
            ; Values = v.Body }


    /// FactBase should probably be extended to have e.g comments on clauses.
    [<Struct>]
    type FactBase = 
        | FactBase of Map<FactSignature.Signature, ClauseBody list>

        member v.Add (clause:Clause) : FactBase = 
            let (FactBase db) = v in 
            let db1 = 
                match Map.tryFind clause.Signature db with
                | None -> db.Add(clause.Signature, [clause.Body])
                | Some xs -> db.Add(clause.Signature, clause.Body :: xs)
            FactBase db1

        static member empty : FactBase = 
            FactBase Map.empty
