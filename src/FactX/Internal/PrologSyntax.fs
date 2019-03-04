// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

// Note we should probably consider SWI-Prolog's tables
// if we need to look towards efficiency.

namespace FactX.Internal


[<AutoOpen>]
module FactSignature = 

    open FParsec

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
        let opts = new CharParsers.IdentifierOptions(isAsciiIdStart = isLetter)
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


    open SLFormat.Pretty

    open FactX.Internal.PrintProlog

    /// Note - this syntax favours output not creation.

    /// TODO - should Format take a thunk like ToString() ?

    type Identifier = string

    /// Note - Sequences/tuples not represented (should they be?)
    type Value = 
        | PChar of char
        | PString of string
        | PInt of int64
        | PDecimal of decimal
        | PAtom of string       
        | PQuotedAtom of string
        | PList of Value list
        | PFunctor of Identifier * Value list

        /// CPS transformed!
        member v.Format () : Doc = 
            let rec work (term:Value) (cont:Doc -> Doc) : Doc =
                match term with
                | PChar c -> cont (prologChar c)
                | PString s -> cont (prologString s)
                | PInt i -> cont (prologInt i)
                | PDecimal d -> cont (prologDecimal d)
                | PAtom s -> cont (simpleAtom s)
                | PQuotedAtom s -> cont (quotedAtom s)
                | PList xs -> 
                    workList [] xs (fun vs -> 
                    cont (prologList vs))
                | PFunctor(name, xs) -> 
                    workList [] xs (fun vs -> 
                    cont (prologFunctor name vs))
            and workList (acc:Doc List) (terms: Value list) (cont:Doc list -> Doc) : Doc =
                match terms with
                | [] -> cont (List.rev acc)
                | x :: rest -> 
                    work x (fun v1 -> 
                    workList (v1::acc) rest cont)
            work v (fun x -> x)




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
    

    /// SWI Prolog specific
    type ImportStatement = 
        | LibraryImport of Identifier
        | FileImport of string
        member v.Format () = 
            let body =
                match v with
                | LibraryImport(name) -> text "library" ^^ parens (text name)
                | FileImport(name) -> 
                    if name.Contains(" ") then
                        dquotes (text name)
                    else
                        text name
            text ":- use_module" ^^ parens body ^^ dot


    /// This an object, not a record so it can have different constructors
    type Module = 
        val ModuleName : string
        val Imports : ImportStatement list
        val GlobalComment : string
        val Exports : (Identifier * int) list
        val Database : FactSet list

        new (name:string, imports: ImportStatement list, comment:string, db:FactSet list) = 
            { ModuleName = name
            ; Imports = imports
            ; GlobalComment = comment
            ; Exports = db |> List.map (fun a -> a.ExportSignature)
            ; Database = db }

        // Note - as this is an internal module we (likely) shouldn't 
        // need to overload the constructor.

        new (name:string, db:FactSet list) = 
            { ModuleName = name
            ; Imports = []
            ; GlobalComment = sprintf "%s.pl" name
            ; Exports = db |> List.map (fun a -> a.ExportSignature)
            ; Database = db }

        new (name:string, comment:string, db:FactSet) = 
            { ModuleName = name
            ; Imports = []
            ; GlobalComment = comment
            ; Exports = [db.ExportSignature]
            ; Database = [db] }
        
        new (name:string, db:FactSet) = 
            { ModuleName = name
            ; Imports = []
            ; GlobalComment = sprintf "%s.pl" name
            ; Exports = [db.ExportSignature]
            ; Database = [db] }

        member v.Format () = 
            let d1 = vcat (List.map (fun (x:ImportStatement) -> x.Format()) v.Imports)
            let d2 = prologComment v.GlobalComment
            let d3 = moduleDirective v.ModuleName v.Exports
            let ds = List.map (fun (col:FactSet) -> col.Format()) v.Database
            vcat [ d1; empty; d2; empty; d3; empty; vcat ds ]

        member v.ToProlog () : string = 
            render 160 <| v.Format()
        
        member v.Save (lineWidth:int, filePath:string) = 
            let ss = v.Format()
            writeDoc lineWidth filePath ss
            


