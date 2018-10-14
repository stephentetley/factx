// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Internal

open YC.PrettyPrinter.Pretty
open YC.PrettyPrinter.Doc
open YC.PrettyPrinter.StructuredFormat

[<AutoOpen>]
module PrintProlog = 

    let private escapeSpecial (source:string) : string = 
        source.Replace("\\" , "\\\\")


    let simpleAtom (value:string) : Doc = wordL value

    // This must escape.
    let quotedAtom (value:string) : Doc = wordL <| sprintf "'%s'" (escapeSpecial value)

    let prologString (value:string) : Doc = 
        wordL <| sprintf "\"%s\"" (escapeSpecial value)

    let prologChar (value:char) : Doc =  wordL <| sprintf "0'%c" value


    let prologList (elements:Doc list) : Doc = 
        wordL "[" ^^ sepListL (wordL ",") elements ^^ wordL "]"