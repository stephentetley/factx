// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Internal

open YC.PrettyPrinter.Pretty
open YC.PrettyPrinter.Doc
open YC.PrettyPrinter.StructuredFormat

[<AutoOpen>]
module PrintProlog = 
    
    let  altCommaListL (docs: Doc list ) : Doc = 
        match docs with
          | []    -> emptyL
          | [x]   -> x
          | x::ys -> List.fold (fun pre y -> pre ++ wordL "," ^^ y) (wordL " " ^^ x) ys

    // let tupled (docs:Doc list) : Doc = bracketL (aboveCommaListL docs)
    
    let prologList (docs:Doc list) : Doc = 
        squareBracketL (altCommaListL docs)

    let private escapeSpecial (source:string) : string = 
        source.Replace("\\" , "\\\\")


    let simpleAtom (value:string) : Doc = sepL value

    // This must escape.
    let quotedAtom (value:string) : Doc = wordL <| sprintf "'%s'" (escapeSpecial value)

    let prologString (value:string) : Doc = 
        wordL <| sprintf "\"%s\"" (escapeSpecial value)

    let prologChar (value:char) : Doc =  wordL <| sprintf "0'%c" value

    let prologBool (value:bool) : Doc = 
        wordL <| if value then "true" else "false"

    let prologInt (i:int) : Doc = 
       wordL <| i.ToString()

    let prologFloat (d:float) : Doc = 
        wordL <| d.ToString()

    let prologDouble (d:double) : Doc = 
        wordL <| d.ToString()
    
    let prologDecimal (d:decimal) : Doc = 
        // Ensure Prolog printing renders to a decimal string.
        wordL <| let d1 = 0.0M + d in d1.ToString()

    //let prologList (elements:Doc list) : Doc = 
    //    wordL "[" ^^ sepListL (wordL ",") elements ^^ wordL "]"

    let prologComment (comment:string) : Doc = 
        let lines = comment.Split [|'\n'|] |> Array.toList
        aboveListL <| List.map (fun s -> wordL (sprintf "%c %s" '%' s)) lines

    /// TODO must be no space between head and open-paren            
    let prologFact (head:string) (body:Doc list) : Doc =
        (wordL <| sprintf "%s(" head) ^^ altCommaListL body ^^ wordL ")."

    let prologFunctor (head:string) (body:Doc list) : Doc =
        (wordL <| sprintf "%s(" head) ^^ altCommaListL body ^^ wordL ")"

    /// E.g:
    ///     :- module(installation,
    ///               [installation/3]).
    ///
    let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
        let exportList : Doc = 
            let factNames = List.map (fun (s,i) -> wordL (sprintf "%s/%i" s i)) exports
            prologList factNames
                
        wordL ":- module(" ++ altCommaListL [wordL moduleName; exportList] ++ wordL ")."


