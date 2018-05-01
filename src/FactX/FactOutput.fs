module FactX.FactOutput

open System
open System.IO



type FactOutput<'a> = 
    FactOutput of (StreamWriter -> 'a)

let inline private apply1 (ma : FactOutput<'a>) (handle:StreamWriter) : 'a = 
    match ma with | FactOutput f -> f handle

let inline private unitM (x:'a) : FactOutput<'a> = 
    FactOutput <| fun handle -> x

let inline private bindM (ma:FactOutput<'a>) (f : 'a -> FactOutput<'b>) : FactOutput<'b> =
    FactOutput <| fun handle -> 
        let a = apply1 ma handle in apply1 (f a) handle


let fail : FactOutput<'a> = 
    FactOutput (fun _ -> failwith "FactOutput fail")

type FactOutputBuilder() = 
    member self.Return x = unitM x
    member self.Bind (p,f) = bindM p f
    member self.Zero () = unitM ()

let factOutput:FactOutputBuilder = new FactOutputBuilder()


// Monadic operations
let fmapM (fn:'a -> 'b) (ma:FactOutput<'a>) : FactOutput<'b> = 
    FactOutput <| fun handle ->
        let a = apply1 ma handle in fn a


let mapM (fn: 'a -> FactOutput<'b>) (xs: 'a list) : FactOutput<'b list> = 
    let rec work ac list = 
        match list with
        | y :: ys -> bindM (fn y) (fun b -> work (b::ac) ys)
        | [] -> unitM <| List.rev ac
    work [] xs

let forM (xs:'a list) (fn:'a -> FactOutput<'b>) : FactOutput<'b list> = mapM fn xs

let mapMz (fn: 'a -> FactOutput<'b>) (xs: 'a list) : FactOutput<unit> = 
    let rec work list = 
        match list with
        | y :: ys -> bindM (fn y) (fun _ -> work ys)
        | [] -> unitM ()
    work xs

let forMz (xs:'a list) (fn:'a -> FactOutput<'b>) : FactOutput<unit> = mapMz fn xs

let traverseM (fn: 'a -> FactOutput<'b>) (source:seq<'a>) : FactOutput<seq<'b>> = 
    FactOutput <| fun handle ->
        Seq.map (fun x -> let mf = fn x in apply1 mf handle) source


let traverseiM (fn: int ->  'a -> FactOutput<'b>) (source:seq<'a>) : FactOutput<seq<'b>> = 
    FactOutput <| fun handle ->
        Seq.mapi (fun ix x -> let mf = fn ix x in apply1 mf handle) source


// Need to be strict - hence use a fold
let traverseMz (fn: 'a -> FactOutput<'b>) (source:seq<'a>) : FactOutput<unit> = 
    FactOutput <| fun handle ->
        Seq.fold (fun ac x -> 
                    let ans  = apply1 (fn x) handle in ac) 
                 () 
                 source 

let traverseiMz (fn: int -> 'a -> FactOutput<'b>) (source:seq<'a>) : FactOutput<unit> = 
    FactOutput <| fun handle ->
        ignore <| Seq.fold (fun ix x -> 
                            let ans  = apply1 (fn ix x) handle in (ix+1))
                            0
                            source 


let mapiM (fn: 'a -> int -> FactOutput<'b>) (xs: 'a list) : FactOutput<'b list> = 
    let rec work ac ix list = 
        match list with
        | y :: ys -> bindM (fn y ix) (fun b -> work (b::ac) (ix+1) ys)
        | [] -> unitM <| List.rev ac
    work [] 0 xs

let mapiMz (fn: 'a -> int -> FactOutput<'b>) (xs: 'a list) : FactOutput<unit> = 
    let rec work ix list = 
        match list with
        | y :: ys -> bindM (fn y ix) (fun _ -> work (ix+1) ys)
        | [] -> unitM ()
    work 0 xs

// FactOutput-specific operations

/// Should monadic function be first or second argument?
let runFactOutput (fileName:string) (ma:FactOutput<'a>) : 'a =
    use sw = new System.IO.StreamWriter(fileName)
    apply1 ma sw


type TermWriter = private TermWriter of string

let inline private getTW (tw:TermWriter) : string = 
    match tw with
    | TermWriter value -> value

let tellComment (comment:string) : FactOutput<unit> = 
    FactOutput <| fun handle ->
        let lines = comment.Split [|'\n'|] |> Array.toList
        List.iter (fun s -> handle.WriteLine (sprintf "%% %s" s)) lines

let tellFact (head:TermWriter) (body: TermWriter list) : FactOutput<unit> =
    FactOutput <| fun handle ->
        let args = String.concat ", " <| List.map getTW body
        let line:string = sprintf "%s(%s)." (getTW head) args
        handle.WriteLine line

let namedAtom (value:string) : TermWriter = TermWriter value
let quotedAtom (value:string) : TermWriter = 
    TermWriter << sprintf "'%s'" <| value.Replace("'","''")

let bool (value:bool) : TermWriter = TermWriter <| if value then "true" else "false"

let string (value:string) : TermWriter = TermWriter <| sprintf "\"%s\"" value

let int (d:int) : TermWriter = TermWriter <| sprintf "%d" d