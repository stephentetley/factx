// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.String


[<AutoOpen>]
module String = 
    
    let leftOf (needle:string) (source:string) = 
        let ix = source.IndexOf needle 
        if ix > 0 then 
            source.Substring(0,ix)
        else 
            ""

    let rightOf (needle:string) (source:string) = 
        let ix = source.IndexOf needle 
        if ix >= 0 then 
            source.Substring(ix+needle.Length)
        else 
            ""

    let leftOfAny (needles:string list) (source:string) = 
        List.pick (fun needle -> 
                    match leftOf needle source with
                    | "" -> None
                    | str -> Some str) needles

    let rightOfAny (needles:string list) (source:string) = 
        List.pick (fun needle -> 
                    match rightOf needle source with
                    | "" -> None
                    | str -> Some str) needles

    let toUpper (source:string) : string = source.ToUpper()
    let toLower (source:string) : string = source.ToLower()

