// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Internal.PrintProlog

open FactX.Internal.PrettyPrint


[<AutoOpen>]
module PrintProlog = 
    
    // Indent-level of 4 seems good in Prolog.


    let commaSep (docs:Doc list) = foldDocs (fun ac e -> ac ^^ comma ^/^ e) docs
    let commaSepV (docs:Doc list) = foldDocs (fun ac e -> ac ^@^ comma ^/^ e) docs

    let prologList (docs:Doc list) : Doc = brackets (commaSep docs)
    let prologListV (docs:Doc list) : Doc = brackets (commaSepV docs)

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

    let prologInt (i:int64) : Doc = 
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

    let prologFunctor (head:string) (body:Doc list) : Doc =
        nest 4 (text (escapeSpecial head) ^^ lparen ^/^ group (commaSep body)) ^^ rparen

    /// Must be no space between head and open-paren            
    let prologFact (head:string) (body:Doc list) : Doc =
        prologFunctor head body ^^ dot




    /// E.g:
    ///     :- module(installation,
    ///               [installation/3]).
    ///
    let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
        let exportList : Doc = 
            let factNames = List.map (fun (s,i) -> text (sprintf "%s/%i" s i)) exports
            prologList factNames                
        nest 8 (text ":-" ^+^ text "module" ^^ parens (text moduleName ^^ comma ^/^ exportList) ^^ dot)


