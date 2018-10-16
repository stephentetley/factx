// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Internal.PrintProlog

open FactX.Internal.PrettyPrint


[<AutoOpen>]
module PrintProlog = 
    
    let  altCommaListL (docs: Doc list ) : Doc = 
        match docs with
          | []    -> empty
          | [x]   -> x
          | x::ys -> List.fold (fun pre y -> pre ^^ comma ^^ y) (space ^^ x) ys

    // let tupled (docs:Doc list) : Doc = bracketL (aboveCommaListL docs)
    
    let prologList (docs:Doc list) : Doc = 
        brackets (altCommaListL docs)

    let private escapeSpecial (source:string) : string = 
        source.Replace("\\" , "\\\\")


    let simpleAtom (value:string) : Doc = text value

    // This must escape.
    let quotedAtom (value:string) : Doc = text <| sprintf "'%s'" (escapeSpecial value)

    let prologString (value:string) : Doc = 
        text <| sprintf "\"%s\"" (escapeSpecial value)

    let prologChar (value:char) : Doc =  text <| sprintf "0'%c" value

    let prologBool (value:bool) : Doc = 
        text <| if value then "true" else "false"

    let prologInt (i:int) : Doc = 
       text <| i.ToString()

    let prologFloat (d:float) : Doc = 
        text <| d.ToString()

    let prologDouble (d:double) : Doc = 
        text <| d.ToString()
    
    let prologDecimal (d:decimal) : Doc = 
        // Ensure Prolog printing renders to a decimal string.
        text <| let d1 = 0.0M + d in d1.ToString()

    //let prologList (elements:Doc list) : Doc = 
    //    wordL "[" ^^ sepListL (wordL ",") elements ^^ wordL "]"

    let prologComment (comment:string) : Doc = 
        let lines = comment.Split [|'\n'|] |> Array.toList
        altCommaListL <| List.map (fun s -> text (sprintf "%c %s" '%' s)) lines

    /// TODO must be no space between head and open-paren            
    let prologFact (head:string) (body:Doc list) : Doc =
        (text <| sprintf "%s(" head) ^^ altCommaListL body ^^ text ")."

    let prologFunctor (head:string) (body:Doc list) : Doc =
        (text <| sprintf "%s(" head) ^^ altCommaListL body ^^ text ")"

    /// E.g:
    ///     :- module(installation,
    ///               [installation/3]).
    ///
    let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
        let exportList : Doc = 
            let factNames = List.map (fun (s,i) -> text (sprintf "%s/%i" s i)) exports
            prologList factNames
                
        text ":- module(" ^^ altCommaListL [text moduleName; exportList] ^^ text ")."


