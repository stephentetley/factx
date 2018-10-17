// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX



open FactX.Internal
open FactX.Internal.PrettyPrint

[<AutoOpen>]
module FactOutput = 

    let allSomes (source:(option<'a>) list) : option<'a list> = 
        let rec work ac xs = 
            match xs with 
            | [] -> Some <| List.rev ac
            | (None :: _)-> None
            | (Some(x) :: rest) -> work (x::ac) rest
        work [] source

    type ClauseBody = PrologSyntax.Value list

    type Clause = 
        { Signature: FactSignature.Signature
          Body : ClauseBody }
        member v.ToProlog() : PrologSyntax.Clause = 
            { FactName = v.Signature.Name
            ; Values = v.Body }

        static member cons (signature:string, body:PrologSyntax.Value list) : Clause = 
            match FactSignature.tryParseSignature signature with
            | None -> failwithf "Clause.cons - Invalid signature: '%s'" signature
            | Some sig1 -> { Signature = sig1; Body = body }

        /// If parsing the signature fails this generates None.
        static member optionCons (signature:string, body:PrologSyntax.Value list) : option<Clause> = 
            match FactSignature.tryParseSignature signature with
            | None -> None
            | Some sig1 -> Some { Signature = sig1; Body = body }
        
        static member optionCons (signature:string, body:(option<PrologSyntax.Value>) list) : option<Clause> = 
            match FactSignature.tryParseSignature signature, allSomes body with
            | Some sig1, Some values -> Some { Signature = sig1; Body = values }
            | _, _ -> None
            

    let private makeFactSet (signature:FactSignature.Signature) 
                            (clauses: ClauseBody list) : PrologSyntax.FactSet =
        let makeClause1 (body:ClauseBody)  = 
            { Signature = signature; Body = body }
        { FactName = signature.Name
          Arity = signature.Arity
          Signature = signature.ToString()
          Comment = ""
          Clauses = List.map (fun (v:ClauseBody) -> (makeClause1 v).ToProlog()) clauses
          }


    let private mapToClauseList (omap: Map<Signature, ClauseBody list>) : Clause list = 
        let makeClauses (key:Signature, bodies:ClauseBody list) = 
            List.map (fun body -> { Signature = key; Body = body}) bodies
        Map.toList omap 
            |> List.map makeClauses
            |> List.concat

    /// Extending FactBase to include e.g comments on clauses, would be 
    /// nice but we lose the simplicity (and potentially the efficiency) 
    /// of just wrapping Map<>.
    /// Also we want FactBase to be immutable so we can have e.g backtracking 
    /// fact extractors.
    [<Struct>]
    type FactBase = 
        | FactBase of Map<FactSignature.Signature, ClauseBody list>
     
        static member empty : FactBase = 
            FactBase Map.empty

        member v.Add (clause:Clause) : FactBase = 
            let (FactBase db) = v
            let db1 = 
                match Map.tryFind clause.Signature db with
                | None -> db.Add(clause.Signature, [clause.Body])
                | Some xs -> db.Add(clause.Signature, clause.Body :: xs)
            FactBase db1

        member v.Add(opt:option<Clause>) : FactBase = 
            match opt with
            | None -> v
            | Some clause -> v.Add(clause)

        member v.AddList (clauses: Clause list) : FactBase = 
            List.foldBack (fun (c:Clause) (ac:FactBase) -> ac.Add(c)) clauses v

        member v.AddList (opts: option<Clause> list) : FactBase = 
            List.foldBack (fun (c:option<Clause>) (ac:FactBase) -> ac.Add(c)) opts v

        member v.Concat (facts:FactBase) : FactBase = 
            let (FactBase db) = v
            let clauses = mapToClauseList db
            facts.AddList(clauses)


        static member ofList(clauses:Clause list) : FactBase =
            List.foldBack (fun (clz:Clause) ac -> ac.Add(clz)) clauses FactBase.empty

        static member ofArray(clauses:Clause []) : FactBase =
            Array.foldBack (fun (clz:Clause) ac -> ac.Add(clz)) clauses FactBase.empty

        static member ofOptionList(optClauses:option<Clause> list) : FactBase =
            List.foldBack (fun (opt:option<Clause>) ac -> 
                                match opt with
                                | None -> ac
                                | Some clz -> ac.Add(clz) ) 
                          optClauses 
                          FactBase.empty

        static member ofOptionArray(optClauses:option<Clause> []) : FactBase =
            Array.foldBack (fun (opt:option<Clause>) ac -> 
                                match opt with
                                | None -> ac
                                | Some clz -> ac.Add(clz) ) 
                          optClauses 
                          FactBase.empty

        member v.ToProlog() : PrologSyntax.FactSet list = 
            let (FactBase db) = v in 
            Map.toList db |> List.map (fun (k,x) -> makeFactSet k x)
    
    let mergeFactBases (dbs:FactBase list) : FactBase = 
        match dbs with
        | [] -> FactBase.empty
        | x :: xs -> List.foldBack (fun e ac -> ac.Concat(e)) xs x

    type Module = 
        val ModuleName : string
        val GlobalComment : string
        val Database : FactBase
        new (name:string, db:FactBase) = 
            { ModuleName = name
            ; GlobalComment = ""
            ; Database = db }

        new (name:string, dbs:FactBase list) = 
            { ModuleName = name
            ; GlobalComment = ""
            ; Database = mergeFactBases dbs }

        new (name:string, comment:string, db:FactBase) = 
            { ModuleName = name
            ; GlobalComment = comment
            ; Database = db }

        new (name:string, comment:string, dbs:FactBase list) = 
            { ModuleName = name
            ; GlobalComment = comment
            ; Database = mergeFactBases dbs }

        member v.ToProlog() : PrologSyntax.Module = 
            let prologFacts = v.Database.ToProlog () 
            new PrologSyntax.Module (name = v.ModuleName, comment = v.GlobalComment, db = prologFacts)

        member v.Save(filePath:string) = 
            let prologModule = v.ToProlog()
            prologModule.Save(240, filePath)

        member v.Save(lineWidth:int, filePath:string) = 
            let prologModule = v.ToProlog()
            prologModule.Save(lineWidth, filePath)

[<AutoOpen>]
module Values = 

    type Value = PrologSyntax.Value

    /// Create a Prolog string value.
    /// No error checking if the string is null or empty
    let prologSymbol (input:string) : Value = PrologSyntax.PQuotedAtom input

    /// Safe version of pSymbol.
    /// If the string is null or empty None is returned.
    /// Note Symbols are trimmed to remove trailing whitespace.
    let optPrologSymbol (input:string) : option<Value> = 
        match input with
        | null -> None
        | "" -> None
        | ss -> 
            let s1 = ss.Trim() 
            if s1.Length > 0 then
                Some (prologSymbol s1)
            else None

    let prologChar (input:char) : Value = PrologSyntax.PChar input

    // char cannot be null so no need for optPrologChar

    let prologString (input:string) : Value = PrologSyntax.PString input

    let optPrologString (input:string) : option<Value> = 
        match input with
        | null -> None
        | ss -> Some (prologString ss)

    let prologDecimal (d:decimal) : Value = PrologSyntax.PDecimal d
    
    let readPrologDecimal (input:string) : option<Value> = 
        try 
            let ans = decimal input in Some (prologDecimal ans)
        with
        | _ -> None

    let prologInt (i:int) : Value = PrologSyntax.PInt i
    
    let readPrologInt (input:string) : option<Value> = 
        try 
            let ans = int input in Some (prologInt ans)
        with
        | _ -> None

    let prologList (elements:Value list) = PrologSyntax.PList elements

    let optPrologList (elements:(option<Value>) list) = 
        match allSomes elements with
        | None -> None
        | Some xs -> Some (prologList xs)

    let prologFunctor (name:string) (elements:Value list) = 
        PrologSyntax.PFunctor(name, elements)

    let optPrologFunctor (name:string)  (elements:(option<Value>) list) = 
        match allSomes elements with
        | None -> None
        | Some xs -> Some (prologFunctor name xs)

    /// TODO - what does Prolog use for null / unknown?
    let valueOrUnknown (value:option<Value>) : Value = 
        match value with
        | Some v -> v
        | None -> prologSymbol "UNKNOWN"