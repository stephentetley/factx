// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.ValueReader

open FactX.FactOutput

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

    let inline private unitM (x:'a) : ValueReader<'a> = ValueReader (Some x)

    let inline private bindM (ma:ValueReader<'a>) (f : 'a -> ValueReader<'b>) : ValueReader<'b> =
        ValueReader <| 
            match apply1 ma with
            | None -> None
            | Some a -> apply1 (f a)

    type ValueReaderBuilder() = 
        member __.Bind(p,f) = bindM p f
        member __.Return(x) = unitM x

    let valueReader = new ValueReaderBuilder()

    let runValueReader (ma:ValueReader<'a>) : Option<'a> = apply1 ma

    let readString (input:string) : ValueReader<Value> = 
        ValueReader <|
            match input with
            | null -> Some (PString "")
            | str -> Some (PString str)

    let readSymbol (input:string) : ValueReader<Value> = 
        ValueReader <|
            match input with
            | null -> None
            | str -> Some (PQuotedAtom str)

    let readDecimal (input:string) : ValueReader<Value> = 
        ValueReader <|
            try 
                let ans = decimal input in Some (PDecimal ans)
            with
            | _ -> None

    let readInt (input:string) : ValueReader<Value> = 
        ValueReader <|
            try 
                let ans = int input in Some (PInt ans)
            with
            | _ -> None