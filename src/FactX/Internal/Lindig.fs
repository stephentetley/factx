// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

// Acknowledgment: These are Christian Lindig's pretty printers 
// from the paper "Strictly Pretty" with extra combinators from 
// Daan Leijen's PPrint.


namespace FactX.Internal.Lindig

open System.Text

[<AutoOpen>]
module Lindig = 
    
    type Doc = 
        | Nil
        | Cons of Doc * Doc
        | Char of char
        | Text of string
        | Nest of int * Doc
        | Break of string
        | Group of Doc

    let (^^) (x:Doc) (y:Doc) = Cons (x,y)
    
    let empty : Doc = Nil

    let char (c:char) : Doc = Char c

    let text (s:string) : Doc = Text s

    let nest (i:int) (d:Doc) = Nest (i, d)

    let spaceBreak : Doc = Break " "

    let lineBreak : Doc = Break "\n"

    let breakWith (s:string) = Break s

    let group (d:Doc) : Doc = Group d

    type SDoc = 
        | SNil 
        | SText of string * SDoc
        | SLine of int * SDoc       // newline + spaces

    let sdocToString (source:SDoc) : string = 
        let sb = new StringBuilder ()
        let rec work (sdoc:SDoc) cont = 
            match sdoc with 
            | SNil -> cont ()
            | SText(s,d) -> 
                sb.Append(s) |> ignore
                work d cont
            | SLine(i,d) -> 
                let prefix = String.replicate i " " 
                sb.Append("\n" + prefix) |> ignore
                work d cont                
        work source (fun _ -> ())
        sb.ToString()


    type Mode = | Flat1 | Break1

    type private Format1 = int * Mode * Doc

    let rec private fits (w:int) (xs:Format1 list) : bool = 
        match xs with
        | _ when w < 0 -> false
        | []                     -> true
        | (_,_,Nil)             :: zs -> fits w zs
        | (i,m,Cons(x,y))       :: zs -> fits w ((i,m,x) :: (i,m,y) :: zs)
        | (i,m,Nest(j,x))       :: zs -> fits w ((i+j,m,x) :: zs)
        | (_,_,Text(s))         :: zs -> fits (w - s.Length) zs
        | (_,_,Char(c))         :: zs -> fits (w - 1) zs
        | (_,Flat1,Break(s))    :: zs -> fits (w - s.Length) zs
        | (_,Break1,Break(_))   :: _ -> true    // Impossible
        | (i,_,Group(x))        :: zs -> fits w ((i,Flat1,x) :: zs)

    let rec private format (w:int) (k:int) (xs:Format1 list) : SDoc = 
        match xs with
        | [] -> SNil
        | (_,_,Nil)             :: zs -> format w k zs
        | (i,m,Cons(x,y))       :: zs -> format w k ((i,m,x) :: (i,m,y) :: zs)
        | (i,m,Nest(j,x))       :: zs -> format w k ((i+j,m,x) :: zs)
        | (_,_,Text(s))         :: zs -> let d1 = format w (k + s.Length) zs in SText(s,d1)
        | (_,_,Char(c))         :: zs -> let d1 = format w (k + 1) zs in SText(c.ToString(),d1)
        | (_,Flat1,Break(s))    :: zs -> let d1 = format w (k + s.Length) zs in SText(s,d1)
        | (i,Break1,Break(_))   :: zs -> 
            let d1 = format w i zs in SLine(i,d1)
        | (i,_,Group(x))        :: zs -> 
            if fits (w - k) ((i,Flat1,x) :: zs) then 
                format w k ((i,Flat1,x) :: zs)
            else
                format w k ((i,Break1,x) :: zs)

    let render (lineWidth:int) (doc:Doc) : string = 
        format lineWidth 0 [(0,Flat1,doc)] |> sdocToString


    /// Single left parenthesis: '('
    let lparen : Doc = char '('

    /// Single right parenthesis: ')'
    let rparen : Doc = char ')'

    /// Single left angle: '<'
    let langle : Doc = char '<'

    /// Single right angle: '>'
    let rangle : Doc = char '>'

    /// Single left brace: '{'
    let lbrace : Doc = char '{'
    
    /// Single right brace: '}'
    let rbrace : Doc= char '}'
    
    /// Single left square bracket: '['
    let lbracket : Doc = char '['
    
    /// Single right square bracket: ']'
    let rbracket : Doc = char ']'


    /// Single quote: '
    let squote : Doc= char '\''

    ///The document @dquote@ contains a double quote, '\"'.
    let dquote : Doc = char '"'

    /// The document @semi@ contains a semi colon, \";\".
    let semi : Doc = char ';'

    /// The document @colon@ contains a colon, \":\".
    let colon : Doc = char ':'

    /// The document @comma@ contains a comma, \",\".
    let comma : Doc = char ','

    /// The document @space@ contains a single space, \" \".
    let space : Doc = char ' '

    /// The document @dot@ contains a single dot, \".\".
    let dot : Doc = char '.'

    /// The document @backslash@ contains a back slash, \"\\\".
    let backslash : Doc = char '\\'

    /// The document @equals@ contains an equal sign, \"=\".
    let equals : Doc = char '='

    /// Don't try to define (<>) - it is a reserved operator name in F#

    /// Concatenates d1 and d2 horizontally, with spaceBreak.
    let (^+^) (d1:Doc)  (d2:Doc) : Doc = 
        match d1,d2 with
        | Nil, _ -> d1
        | _, Nil -> d2
        |_, _    -> d1 ^^ spaceBreak ^^ d2

    /// Concatenates d1 and d2 vertically, with optionally breaking space.
    let (^@^) (d1:Doc)  (d2:Doc) : Doc = 
        match d1,d2 with
        | Nil, _ -> d1
        | _, Nil -> d2
        |_, _    -> d1 ^^ lineBreak ^^ d2




    let punctuate (sep:Doc) (docs:Doc list) : Doc = 
        let rec work acc ds = 
            match ds with
            | [] -> acc
            | (x :: xs) -> work (acc ^^ sep ^^ x) xs
        match docs with
        | [] -> empty
        | (x :: xs) -> work x xs


    let enclose (left:Doc) (right:Doc) (d:Doc) : Doc = left ^^ d ^^ right



    let encloseSep (left:Doc) (right:Doc) (sep:Doc) (docs:Doc list) : Doc = 
        left ^^ punctuate sep docs ^^ right
        


    /// Enclose in single quotes: '..'
    let squotes (d:Doc) : Doc = enclose squote squote d

    /// Enclose in double quotes: ".."
    let dquotes (d:Doc) : Doc = enclose dquote dquote d

    /// Enclose in braces: {..}
    let braces (d:Doc) : Doc = enclose lbrace rbrace d

    /// Enclose in parenthesis: (..)
    let parens (d:Doc) : Doc = enclose lparen rparen d

    /// Enclose in angles: <..>
    let angles (d:Doc) : Doc = enclose langle rangle d

    /// Enclose in brackets: [..]
    let brackets (d:Doc) : Doc = enclose lbracket rbracket d


    let commaList (docs:Doc list) : Doc = encloseSep lbracket rbracket comma docs

    let semiList (docs:Doc list) : Doc = encloseSep lbracket rbracket semi docs
