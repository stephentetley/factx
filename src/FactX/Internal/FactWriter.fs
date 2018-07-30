// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause


module FactX.Internal.FactWriter

open System
open System.IO

open FactX.Internal.FormatCombinators

// *************************************
// Monad definition


// Note the monadic (writer) interface feels lower level than 
// e.g. CsvProviders csv write interface.

type FactWriter<'a> = 
    FactWriter of (StreamWriter -> 'a)

let inline private apply1 (ma : FactWriter<'a>) (handle:StreamWriter) : 'a = 
    match ma with | FactWriter f -> f handle

let inline private unitM (x:'a) : FactWriter<'a> = 
    FactWriter <| fun handle -> x

let inline private bindM (ma:FactWriter<'a>) (f : 'a -> FactWriter<'b>) : FactWriter<'b> =
    FactWriter <| fun handle -> 
        let a = apply1 ma handle in apply1 (f a) handle


let fail : FactWriter<'a> = 
    FactWriter (fun _ -> failwith "FactWriter fail")

type FactWriterBuilder() = 
    member self.Return x = unitM x
    member self.Bind (p,f) = bindM p f
    member self.Zero () = unitM ()

let factWriter:FactWriterBuilder = new FactWriterBuilder()


// Monadic operations
let fmapM (fn:'a -> 'b) (ma:FactWriter<'a>) : FactWriter<'b> = 
    FactWriter <| fun handle ->
        let a = apply1 ma handle in fn a


let mapM (fn: 'a -> FactWriter<'b>) (xs: 'a list) : FactWriter<'b list> = 
    let rec work ac list = 
        match list with
        | y :: ys -> bindM (fn y) (fun b -> work (b::ac) ys)
        | [] -> unitM <| List.rev ac
    work [] xs

let forM (xs:'a list) (fn:'a -> FactWriter<'b>) : FactWriter<'b list> = mapM fn xs

let mapMz (fn: 'a -> FactWriter<'b>) (xs: 'a list) : FactWriter<unit> = 
    let rec work list = 
        match list with
        | y :: ys -> bindM (fn y) (fun _ -> work ys)
        | [] -> unitM ()
    work xs

let forMz (xs:'a list) (fn:'a -> FactWriter<'b>) : FactWriter<unit> = mapMz fn xs

let traverseM (fn: 'a -> FactWriter<'b>) (source:seq<'a>) : FactWriter<seq<'b>> = 
    FactWriter <| fun handle ->
        Seq.map (fun x -> let mf = fn x in apply1 mf handle) source


let traverseiM (fn: int ->  'a -> FactWriter<'b>) (source:seq<'a>) : FactWriter<seq<'b>> = 
    FactWriter <| fun handle ->
        Seq.mapi (fun ix x -> let mf = fn ix x in apply1 mf handle) source


// Need to be strict - hence use a fold
let traverseMz (fn: 'a -> FactWriter<'b>) (source:seq<'a>) : FactWriter<unit> = 
    FactWriter <| fun handle ->
        Seq.fold (fun ac x -> 
                    let ans  = apply1 (fn x) handle in ac) 
                 () 
                 source 

let traverseiMz (fn: int -> 'a -> FactWriter<'b>) (source:seq<'a>) : FactWriter<unit> = 
    FactWriter <| fun handle ->
        ignore <| Seq.fold (fun ix x -> 
                            let ans  = apply1 (fn ix x) handle in (ix+1))
                            0
                            source 


let mapiM (fn: 'a -> int -> FactWriter<'b>) (xs: 'a list) : FactWriter<'b list> = 
    let rec work ac ix list = 
        match list with
        | y :: ys -> bindM (fn y ix) (fun b -> work (b::ac) (ix+1) ys)
        | [] -> unitM <| List.rev ac
    work [] 0 xs

let mapiMz (fn: 'a -> int -> FactWriter<'b>) (xs: 'a list) : FactWriter<unit> = 
    let rec work ix list = 
        match list with
        | y :: ys -> bindM (fn y ix) (fun _ -> work (ix+1) ys)
        | [] -> unitM ()
    work 0 xs

// FactWriter-specific operations

/// Should monadic function be first or second argument?
let runFactWriter (fileName:string) (ma:FactWriter<'a>) : 'a =
    use sw = new System.IO.StreamWriter(fileName)
    apply1 ma sw


let tell (d:Doc) : FactWriter<unit> = 
    FactWriter <| fun handle ->
        handle.WriteLine (render d)



