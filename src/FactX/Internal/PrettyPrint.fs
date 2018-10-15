// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause
// Acknowledgment: These are Christian Lindig's pretty printers from the paper "Strictly Pretty"


namespace FactX.Internal



[<AutoOpen>]
module PrettyPrint = 
    
    type Doc = 
        | Nil
        | Cons of Doc * Doc
        | Text of string
        | Nest of int * Doc
        | Space of string
        | Group of Doc

    let (^^) (x:Doc) (y:Doc) = Cons (x,y)
    
    let empty : Doc = Nil

    let text (s:string) : Doc = Text s

    let nest (i:int) (d:Doc) = Nest (i, d)

    let sep : Doc = Space " "

    let sepWith (s:string) = Space s

    let group (d:Doc) : Doc = Group d

    type SDoc = 
        | SNil 
        | SText of string * SDoc
        | SLine of int * SDoc       // newline + spaces

    let rec sdocToString (sdoc:SDoc) : string = 
        match sdoc with 
        | SNil -> ""
        | SText(s,d) -> s + sdocToString d
        | SLine(i,d) -> 
            let prefix = String.replicate i " " 
            "\n" + prefix + sdocToString d

    type Mode = | Flat | Break

    type Format1 = int * Mode * Doc

    let rec fits (w:int) (xs:Format1 list) : bool = 
        match xs with
        | _ when w < 0 -> false
        | []                     -> true
        | (_,_,Nil)             :: zs -> fits w zs
        | (i,m,Cons(x,y))       :: zs -> fits w ((i,m,x) :: (i,m,y) :: zs)
        | (i,m,Nest(j,x))       :: zs -> fits w ((i+j,m,x) :: zs)
        | (_,_,Text(s))         :: zs -> fits (w - s.Length) zs
        | (_,Flat,Space(s))     :: zs -> fits (w - s.Length) zs
        | (_,Break,Space(_))    :: _ -> true    // Impossible
        | (i,_,Group(x))        :: zs -> fits w ((i,Flat,x) :: zs)

    let rec format (w:int) (k:int) (xs:Format1 list) : SDoc = 
        match xs with
        | [] -> SNil
        | (_,_,Nil)             :: zs -> format w k zs
        | (i,m,Cons(x,y))       :: zs -> format w k ((i,m,x) :: (i,m,y) :: zs)
        | (i,m,Nest(j,x))       :: zs -> format w k ((i+j,m,x) :: zs)
        | (_,_,Text(s))         :: zs -> let d1 = format w (k + s.Length) zs in SText(s,d1)
        | (_,Flat,Space(s))     :: zs -> let d1 = format w (k + s.Length) zs in SText(s,d1)
        | (i,Break,Space(_))    :: zs -> let d1 = format w i zs in SLine(i,d1)
        | (i,_,Group(x))        :: zs -> 
            if fits (w - k) ((i,Flat,x) :: zs) then 
                format w k ((i,Flat,x) :: zs)
            else
                format w k ((i,Break,x) :: zs)

    let render (lineWidth:int) (doc:Doc) : string = 
        format lineWidth 0 [(0,Flat,doc)] |> sdocToString







