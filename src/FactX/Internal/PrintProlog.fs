// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Internal.PrintProlog

open FactX.Internal.PrettyPrint


[<AutoOpen>]
module PrintProlog = 
    

    let commaSep (docs:Doc list) = foldDocs (fun ac e -> ac ^^ comma ^/^ e) docs
    
    let prologList (docs:Doc list) : Doc = brackets (commaSep docs)

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



    let prologComment (comment:string) : Doc = 
        let lines = comment.Split [|'\n'|] |> Array.toList
        vcat <| List.map (fun s -> text (sprintf "%c %s" '%' s)) lines

    /// TODO must be no space between head and open-paren            
    let prologFact (head:string) (body:Doc list) : Doc =
        (text <| sprintf "%s(" head) ^^ commaSep body ^^ text ")."

    let prologFunctor (head:string) (body:Doc list) : Doc =
        (text <| sprintf "%s(" head) ^^ commaSep body ^^ text ")"

    /// E.g:
    ///     :- module(installation,
    ///               [installation/3]).
    ///
    let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
        let exportList : Doc = 
            let factNames = List.map (fun (s,i) -> text (sprintf "%s/%i" s i)) exports
            prologList factNames
                
        text ":- module(" ^^ commaSep [text moduleName; exportList] ^^ text ")."


