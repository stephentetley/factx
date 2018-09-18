// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.ValueReader

open FactX.Internal

[<AutoOpen>]
module ValueReader = 
    
    [<Struct>]
    type ValueReader<'a> = 
        ValueReader of Option<'a>
        with
            override v.ToString() = 
                let (ValueReader opt) = v in opt.ToString()

    let inline private apply1 (ma : ValueReader<'a>) : Option<'a> = 
        match ma with | ValueReader a -> a

    let inline valReturn (x:'a) : ValueReader<'a> = 
        ValueReader (Some x)

    let inline bindM (ma:ValueReader<'a>) (f : 'a -> ValueReader<'b>) : ValueReader<'b> =
        ValueReader <| 
            match apply1 ma with
            | None -> None
            | Some a -> apply1 (f a)

    let inline valZero () : ValueReader<'a> = 
        ValueReader <| None

    let inline private altM (ma:ValueReader<'a>) (mb:ValueReader<'a>) : ValueReader<'a> =
        ValueReader <| 
            match apply1 ma with
            | None -> apply1 mb
            | Some a -> Some a

    let inline private  delayM (fn:unit -> ValueReader<'a>) : ValueReader<'a> = 
        bindM (valReturn ()) fn 
    
    let inline private combineM (ma:ValueReader<unit>) 
                                (mb:ValueReader<'a>) : ValueReader<'a> = 
        ValueReader <| 
            match apply1 ma with
            | None -> None
            | Some _ -> apply1 mb
        
    type ValueReaderBuilder() = 
        member __.Bind(p,f)         = bindM p f
        member __.Return(x)         = valReturn x
        member __.Zero ()           = valZero ()
        member __.Delay fn          = delayM fn
        member __.Combine (ma,mb)   = combineM ma mb

    let valueReader = new ValueReaderBuilder()

    let runValueReader (ma:ValueReader<'a>) : Option<'a> = apply1 ma

    // Common monadic operations
    let fmapM (fn:'a -> 'b) (ma:ValueReader<'a>) : ValueReader<'b> = 
        ValueReader <| 
           match apply1 ma with
           | None -> None
           | Some a -> Some (fn a)

    // This is the nub of embedding FParsec - name clashes.
    // We avoid them by using longer names in DocSoup.

    /// Operator for fmap.
    let (|>>) (ma:ValueReader<'a>) (fn:'a -> 'b) : ValueReader<'b> = 
        fmapM fn ma

    /// Flipped fmap.
    let (<<|) (fn:'a -> 'b) (ma:ValueReader<'a>) : ValueReader<'b> = 
        fmapM fn ma

    let (<||>) (ma:ValueReader<'a>) (mb:ValueReader<'a>) : ValueReader<'a> = 
        altM ma mb

    let readStringRaw (input:string) : ValueReader<string> = 
       ValueReader <|
            match input with
            | null -> None
            | str -> Some str

    let readString (input:string) : ValueReader<PrologSyntax.Value> = 
        readStringRaw input |>> PrologSyntax.PString

    let readSymbol (input:string) : ValueReader<PrologSyntax.Value> = 
        readStringRaw input |>> PrologSyntax.PQuotedAtom

    let readDecimalRaw (input:string) : ValueReader<decimal> = 
        ValueReader <|
            try 
                let ans = decimal input in Some ans
            with
            | _ -> None

    let readDecimal (input:string) : ValueReader<PrologSyntax.Value> = 
        readDecimalRaw input |>> PrologSyntax.PDecimal

    let readIntRaw (input:string) : ValueReader<int> = 
        ValueReader <|
            try 
                let ans = int input in Some ans
            with
            | _ -> None

    let readInt (input:string) : ValueReader<PrologSyntax.Value> = 
        readIntRaw input |>> PrologSyntax.PInt