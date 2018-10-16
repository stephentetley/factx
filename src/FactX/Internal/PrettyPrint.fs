// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

// Acknowledgment: This an implementation of Daan Leijen's PPrint.
// This pretty priniting library has perhaps the nicest interface
// and is very well documented.


namespace FactX.Internal.PrettyPrint

open System.Text
open System

[<AutoOpen>]
module PrettyPrint = 
    
    type Doc = 
        private 
            | Empty
            | Char of char
            | Text of int * string
            | Line of bool      // true when undune by group
            | Cat of Doc * Doc
            | Nest of int * Doc
            | Union of Doc * Doc    // invariant lines of d1 longer than lines of d2
            | Column of (int -> Doc)
            | Nesting of (int -> Doc)

    let (^^) (x:Doc) (y:Doc) = Cat (x,y)
    
    let empty : Doc = Empty



    let char (c:char) : Doc = Char c

    let text (s:string) : Doc = Text(s.Length, s)

    let nest (i:int) (d:Doc) = Nest (i, d)


    //let spaceBreak : Doc = Break " "

    //let lineBreak : Doc = Break "\n"

    //let breakWith (s:string) = Break s

    //let group (d:Doc) : Doc = Group d

    type SDoc = 
        | SEmpty
        | SChar of char * SDoc
        | SText of int * string * SDoc
        | SLine of int * SDoc      

    let sdocToString (source:SDoc) : string = 
        let sb = new StringBuilder ()
        let rec work (sdoc:SDoc) cont = 
            match sdoc with 
            | SEmpty -> cont ()
            | SChar(c,d) -> 
                sb.Append(c) |> ignore
                work d cont
            | SText(_,s,d) -> 
                sb.Append(s) |> ignore
                work d cont
            | SLine(i,d) -> 
                let indent = String.replicate i " " 
                sb.Append("\n" + indent) |> ignore
                work d cont                
        work source (fun _ -> ())
        sb.ToString()


    type Mode = | Flat1 | Break1

    type private Format1 = int * Mode * Doc

    let rec private fits (w:int) (x:SDoc) : bool = 
        match x with
        | _ when w < 0          -> false
        | SEmpty                -> true
        | SChar(_,x)            -> fits (w - 1) x
        | SText(l,_,x)          -> fits (w - l) x
        | SLine _               -> true

    /// Called Docs in Daan's library PPrint
    type private DocList = 
        | Nil 
        | Cons of int * Doc * DocList


    let nicest (ribbon:int) (pageWidth:int) (n:int) (k:int) (x:SDoc) (y:SDoc) : SDoc = 
        let width = min (pageWidth - k) (ribbon - k + n)
        if fits width x then x else y



    /// TODO make tail recursive...
    let rec private best (ribbon:int) (pageWidth:int) 
                            (indentation:int) (colWidth:int) (docs:DocList) : SDoc = 
        let rec work (n:int) (k:int) (x:DocList) = 
            match x with
            | Nil -> SEmpty
            | Cons(i,d,ds) -> 
                match d with
                | Empty         -> work n k ds
                | Char(c)       -> let rest = work n (k+1) ds in SChar(c,rest)
                | Text(l,s)     -> let rest = work n (k+l) ds in SText(l,s,rest)
                | Line(_)       -> let rest = work i i ds in SLine(i,rest)
                | Cat(x,y)      -> work n k (Cons(i,x, (Cons(i,y,ds))))
                | Nest(j,x)     -> 
                    let i1 = i+j 
                    let rest = Cons(i1,x,ds)
                    work n k rest
                | Union(x,y)    -> 
                    let rest1 = Cons(i,x,ds)
                    let rest2 = Cons(i,y,ds)
                    nicest ribbon pageWidth n k (work n k rest1) (work n k rest2)

                | Column(f)     -> let rest = Cons(i,(f k),ds) in work n k rest
                | Nesting(f)    -> let rest = Cons(i,(f i),ds) in work n k rest
        work indentation colWidth docs


    let rec renderPretty1 (rfrac:float) (w:int) (x:Doc) : SDoc = 
        printfn "format w=%i" w 
        let r  = max 0 (min w (int <| Math.Round (float w * rfrac)))
        let ds = Cons(0,x,Nil)
        best r w 0 0 ds




    let rec renderPretty (rfrac:float) (w:int) (x:Doc) : string = 
        renderPretty1 rfrac w x |> sdocToString

    //let render (lineWidth:int) (doc:Doc) : string = 
    //    format lineWidth 1 [(1,Flat1,doc)] |> sdocToString


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

    /// Concatenates d1 and d2 horizontally, with optionally breaking space.
    //let (^|) (d1:Doc)  (d2:Doc) : Doc = 
    //    match d1,d2 with
    //    | Nil, _ -> d1
    //    | _, Nil -> d2
    //    |_, _    -> d1 ^^ spaceBreak ^^ d2

    ///// Concatenates d1 and d2 vertically, with optionally breaking space.
    //let (@|) (d1:Doc)  (d2:Doc) : Doc = 
    //    match d1,d2 with
    //    | Nil, _ -> d1
    //    | _, Nil -> d2
    //    |_, _    -> d1 ^^ lineBreak ^^ d2

    ///// Binop 
    //let binop (left:Doc) (op:Doc) (right:Doc) : Doc = 
    //    group (nest 2 (group (left ^| op) ^| right))



    ///// Concatenates d1 and d2 horizontally with a line between them.
    //let (@@) (d1:Doc)  (d2:Doc) : Doc = d1 ^^ lineBreak ^^ d2

    ///// Concatenates d1 and d2 horizontally with a space between them.
    //let (^+^) (d1:Doc)  (d2:Doc) : Doc = d1 ^^ spaceBreak ^^ d2

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

