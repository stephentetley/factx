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


    type SDoc = 
        private 
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

    let writeSDoc (path:string) (source:SDoc) : unit = 
        use sw = IO.File.CreateText(path)
        let rec work (sdoc:SDoc) cont = 
            match sdoc with 
            | SEmpty -> cont ()
            | SChar(c,d) -> 
                sw.Write(c) |> ignore
                work d cont
            | SText(_,s,d) -> 
                sw.Write(s) |> ignore
                work d cont
            | SLine(i,d) -> 
                let indent = String.replicate i " " 
                sw.Write("\n" + indent) |> ignore
                work d cont                
        work source (fun _ -> ())
        


    type Mode = | Flat1 | Break1

    type private Format1 = int * Mode * Doc


    /// CPS / Tail recursive
    let private fits (width:int) (sdoc:SDoc) : bool = 
        let rec work (w:int) (x:SDoc) (cont: bool -> bool) =
            match x with
            | _ when w < 0          -> cont false
            | SEmpty                -> cont true
            | SChar(_,x)            -> work (w - 1) x (fun ans -> cont ans)
            | SText(l,_,x)          -> work (w - l) x (fun ans -> cont ans)
            | SLine _               -> cont true
        work width sdoc (fun x -> x) 


    /// Called Docs in Daan's library PPrint
    type private DocList = 
        | Nil 
        | Cons of int * Doc * DocList


    let nicest (ribbon:int) (pageWidth:int) (n:int) (k:int) (x:SDoc) (y:SDoc) : SDoc = 
        let width = min (pageWidth - k) (ribbon - k + n)
        if fits width x then x else y



    /// Tail recursive / CPS but probably has error...
    let private best (ribbon:int) (pageWidth:int) 
                            (indentation:int) (colWidth:int) (docs:DocList) : SDoc =
        let rec work (n:int) (k:int) (x:DocList) (cont:SDoc -> SDoc) = 
            match x with
            | Nil -> cont SEmpty
            | Cons(i,d,ds) -> 
                match d with
                | Empty         -> work n k ds (fun ans -> cont ans)
                | Char(c)       -> 
                    work n (k+1) ds (fun ans1 -> cont (SChar(c,ans1)))
                
                | Text(l,s)     -> 
                    work n (k+l) ds (fun ans1 -> cont (SText(l,s,ans1)))

                | Line(_)       -> 
                    cont (work i i ds (fun ans1 -> SLine(i,ans1)))
                    
                | Cat(x,y)      -> 
                   work n k (Cons(i,x,(Cons(i,y,ds)))) (fun ans -> cont ans)

                | Nest(j,x)     -> 
                    work n k (Cons(i+j,x,ds)) (fun ans -> cont ans)
                
                | Union(x,y)    -> 
                    work n k (Cons(i,x,ds)) (fun ans1 ->
                    work n k (Cons(i,y,ds)) (fun ans2 ->
                    cont (nicest ribbon pageWidth n k ans1 ans2)))

                | Column(f)     -> 
                    work n k (Cons(i, f k, ds)) (fun ans -> cont ans)

                | Nesting(f)    -> 
                     work n k (Cons(i, f i, ds)) (fun ans -> cont ans)
        work indentation colWidth docs (fun x -> x)


    let renderPretty1 (rfrac:float) (w:int) (x:Doc) : SDoc = 
        let r  = max 0 (min w (int <| Math.Round (float w * rfrac)))
        let ds = Cons(0,x,Nil)
        best r w 0 0 ds


    let renderPretty (rfrac:float) (w:int) (x:Doc) : string = 
        renderPretty1 rfrac w x |> sdocToString

    let writeDoc (rfrac:float) (w:int) (path:string) (x:Doc) : unit = 
        renderPretty1 rfrac w x |> writeSDoc path



    // ************************************************************************
    // Primitives
    
    let empty : Doc = Empty

    let char (c:char) : Doc = Char c

    let text (s:string) : Doc = 
        match s with
        | null  -> Empty
        | ""    -> Empty
        | _     -> Text(s.Length, s)

    let line : Doc = Line false

    let linebreak : Doc = Line true

    let beside x y      = Cat(x,y)

    let nest (i:int) (x:Doc) : Doc = Nest(i,x)

    let column (fn:int -> Doc) : Doc = Column fn
    let nesting (fn:int -> Doc) : Doc = Nesting fn

    /// TODO - make tail recursive...
    let flatten (doc:Doc) : Doc = 
        let rec work d1 = 
            match d1 with
            | Cat(x,y)      -> Cat(work x, work y)
            | Nest(i,x)     -> Nest(i, work x)
            | Line(brk)     -> if brk then Empty else Text(1," ")
            | Union(x,y)    -> work x
            | Column(f)     -> Column (work << f)
            | Nesting(f)    -> Nesting (work << f)
            | x             -> x                     // Empty,Char,Text
        work doc

    let group (x:Doc) : Doc = Union (flatten x,x)

    let softline : Doc = group line

    let softbreak : Doc = group linebreak

    // ************************************************************************
    // Character printers

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

    // ************************************************************************
    // Concatenation operators



    // Don't try to define (<>) - it is a reserved operator name in F#
    // call it (^^)

    let (^^) (x:Doc) (y:Doc) : Doc = beside x y

    let (^+^) (x:Doc) (y:Doc) : Doc = x ^^ space ^^ y

    let (^/^) (x:Doc) (y:Doc) : Doc = x ^^ softline ^^ y

    let (^//^) (x:Doc) (y:Doc) : Doc = x ^^ softbreak ^^ y

    /// Haskell / PPrint's: <$>
    let (^@^) (x:Doc) (y:Doc) : Doc = x ^^ line ^^ y

    /// Haskell / PPrint's: <$$>
    let (^@@^) (x:Doc) (y:Doc) : Doc = x ^^ linebreak ^^ y


    // ************************************************************************
    // List concatenation 

    let foldDocs f (docs:Doc list) : Doc = 
        match docs with
        | [] -> empty
        | (x::xs) -> List.fold f x xs


    let fillSep (docs:Doc list) : Doc = foldDocs (^/^) docs


    let hsep (docs:Doc list) : Doc = foldDocs (^+^) docs

    let vsep (docs:Doc list) : Doc = foldDocs (^@^) docs

    let hcat (docs:Doc list) : Doc = foldDocs (^^) docs

    let vcat (docs:Doc list) : Doc = foldDocs (^@@^) docs

    let cat (docs:Doc list) : Doc = group <| vcat docs

   

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

