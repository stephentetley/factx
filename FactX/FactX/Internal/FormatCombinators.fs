// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause


namespace FactX.Internal

open System.Text
open System
open System

[<AutoOpen>]
module FormatCombinators = 


    /// This is not a "pretty printer" as it makes no effort to "fit" the output.
    type Doc = 
        | Empty
        | Doc of string
        | HDoc of Doc * Doc
        | VDoc of Doc * Doc
        | Indent of int * Doc
    


    let render (source:Doc) : string = 
        let sb = new StringBuilder ()
        let rec work (doc:Doc) (indent:int) cont  = 
            match doc with
            | Empty -> cont ()
            | Doc str -> sb.Append(str) |> ignore; cont ()
            | HDoc(d1,d2) -> 
                work d1 indent (fun _ ->
                work d2 indent (fun _ -> 
                cont ()))
            | VDoc(d1,d2) -> 
                work d1 indent (fun _ -> 
                sb.Append("\n" + String.replicate indent " ") |> ignore
                work d2 indent (fun _ -> 
                cont ()))
            | Indent(i,d1) -> 
                sb.Append(String.replicate i " ") |> ignore
                work d1 (indent + i) (fun _ -> 
                cont ())
        work source 0 (fun _ -> ())
        sb.ToString()


    /// Print the soc to the console.
    let testRender (source:Doc) : unit = 
        render source |> printfn  "----------\n%s\n----------\n"

    // *************************************
    // Primitive values

    let empty : Doc = 
        Empty


    let formatBool (value:bool) : Doc = 
        Doc <| if value then "true" else "false"


    let formatInt (i:int) : Doc = 
        Doc(i.ToString())

    let formatFloat (d:float) : Doc = 
        Doc(d.ToString())

    let formatDouble (d:double) : Doc = 
        Doc(d.ToString())
    
    let formatDecimal (d:Decimal) : Doc = 
        Doc(d.ToString())



    let formatChar (ch:char) : Doc = 
        Doc (ch.ToString())


    let formatString (value:string) : Doc = 
        Doc <| value

    let singleQuoted (value:string) : Doc = 
        Doc << sprintf "'%s'" <| value.Replace("'","''")

    let doubleQuoted (value:string) : Doc = 
        Doc <| sprintf "\"%s\"" value


    // *************************************
    // Character documents

    /// A single space
    let space : Doc = formatChar ' '

    let dot : Doc = formatChar '.'
    let semi : Doc = formatChar ';'
    let colon : Doc = formatChar ':'
    let comma : Doc = formatChar ','
    let backslash : Doc = formatChar '\\'
    let forwardslash : Doc = formatChar '/'

    let underscore : Doc = formatChar '_'




    // *************************************
    // Combinators

    let indent (i:int) (d:Doc) = 
        Indent(i,d)

    /// Horizontal concat
    let (+++) (d1:Doc) (d2:Doc) : Doc = 
        HDoc(d1,d2)

    /// Horizontal concat with a separating space 
    let (+^+) (d1:Doc) (d2:Doc) : Doc = 
       d1 +++ space +++ d2


    /// Vertical concat
    let (@@@) (d1:Doc) (d2:Doc) : Doc = 
        VDoc(d1,d2)

    /// Vertical concat with a separating blank line 
    let (@^@) (d1:Doc) (d2:Doc) : Doc = 
       (d1 @@@ empty) @@@ d2


    let concat (operator:Doc -> Doc -> Doc) (source:Doc list) : Doc = 
        let rec work (ac:Doc) (xs:Doc list) : Doc = 
            match xs with 
            | [] -> ac
            | [y] -> operator ac y
            | y :: ys -> work (operator ac y) ys
        match source with 
        | [] -> Empty
        | x :: xs -> work x xs


    let hcat (source:Doc list) : Doc = concat (+++) source
    let hsep (source:Doc list) : Doc = concat (+^+) source
    
    let vcat (source:Doc list) : Doc = concat (@@@) source
    let vsep (source:Doc list) : Doc = concat (@^@) source

    let punctuate (sep:Doc) (source:Doc list) : Doc = 
        let rec work (ac:Doc) (xs:Doc list) : Doc = 
            match xs with 
            | [] -> ac
            | [y] -> (ac +++ sep) +++ y
            | y :: ys -> work ((ac +++ sep) +++ y) ys
        match source with 
        | [] -> Empty
        | x :: xs -> work x xs

    let punctuateVertically (sep:Doc) (source:Doc list) : Doc = 
        let rec work (ac:Doc) (xs:Doc list) : Doc = 
            match xs with 
            | [] -> ac
            | [y] -> (ac +++ sep) @@@ y
            | y :: ys -> work ((ac +++ sep) @@@ y) ys
        match source with 
        | [] -> Empty
        | x :: xs -> work x xs

    let enclose (left:Doc) (right:Doc) (d1:Doc) : Doc = 
        (left +++ d1) +++ right


    let parens (doc:Doc) : Doc = 
        enclose (formatChar '(') (formatChar ')') doc

    let angles (doc:Doc) : Doc = 
        enclose (formatChar '<') (formatChar '>') doc

    let squares (doc:Doc) : Doc = 
        enclose (formatChar '[') (formatChar ']') doc

    let braces (doc:Doc) : Doc = 
        enclose (formatChar '{') (formatChar '}') doc



    let tupled (source:Doc list) : Doc = 
        parens (punctuate (formatString ", ") source)

    let commaSepList (source: Doc list) : Doc = 
        squares (punctuate (formatString ", ") source)

    let semiSepList (source: Doc list) : Doc = 
        squares (punctuate (formatString "; ") source)


    let commaSepListVertically (source:Doc list) : Doc = 
        squares (punctuateVertically (formatChar ',') source)


    // *************************************
    // Prolog specific

    let private escapeSpecial (source:string) : string = 
        source.Replace("\\" , "\\\\")



    let simpleAtom (value:string) : Doc = formatString value

    // This must escape.
    let quotedAtom (value:string) : Doc = singleQuoted (escapeSpecial value)

    let prologString (value:string) : Doc = 
        doubleQuoted (escapeSpecial value)

    /// Display the list horizontally.
    let prologList (elements:Doc list) : Doc = commaSepList elements


    let prologComment (comment:string) : Doc = 
        let lines = comment.Split [|'\n'|] |> Array.toList
        vcat <| List.map (fun s -> formatChar '%' +^+ formatString s) lines

    let prologFact (head:Doc) (body:Doc list) : Doc = 
        head +++ tupled body +++ dot

    let moduleDirective (moduleName:string) (exports: (string * int) list) : Doc = 
        let exportList = 
            commaSepListVertically 
                <| List.map (fun (s,i) -> formatString (sprintf "%s/%i" s i)) exports
        formatString ":- module" +++ parens ((formatString moduleName +++ comma) @@@ (indent 10 exportList)) +++ dot
    
