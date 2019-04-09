// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

module FactWriter = 

    open System.IO
    open SLFormat.Pretty

    open FactX

    type ErrMsg = string

    /// TODO - potentially add indent level

    type private LineWidth = int

    type FactWriter<'a> = 
        FactWriter of (int -> StreamWriter -> LineWidth -> 'a * int)


    let inline private apply1 (ma: FactWriter<'a>) 
                              (state:int)
                              (handle: StreamWriter) 
                              (lineWidth:int) : 'a * int = 
        let (FactWriter f) = ma in f state handle lineWidth


    let inline mreturn (x:'a) : FactWriter<'a> = 
        FactWriter <| fun st _ _ -> (x, st)


    let inline private bindM (ma: FactWriter<'a>) 
                        (f :'a -> FactWriter<'b>) : FactWriter<'b> =
        FactWriter <| fun st handle lineWidth -> 
            let (x, st1) = apply1 ma st handle lineWidth
            apply1 (f x) st1 handle lineWidth

    /// Haskell's (>>)
    let inline private combineM (mfirst:FactWriter<'a>) 
                                (msecond:FactWriter<'b>) : FactWriter<'b> = 
        FactWriter <| fun st handle lineWidth -> 
            let (_, st1) =  apply1 mfirst st handle lineWidth
            apply1 msecond st1 handle lineWidth


    let inline private delayM (fn:unit -> FactWriter<'a>) : FactWriter<'a> = 
        bindM (mreturn ()) fn 

    type FactWriterBuilder() = 
        member self.Return x            = mreturn x
        member self.Bind (p,f)          = bindM p f
        member self.Combine (ma,mb)     = combineM ma mb
        member self.Delay fn            = delayM fn
        member self.ReturnFrom(ma)      = ma


    let (factWriter:FactWriterBuilder) = new FactWriterBuilder()      
    
    // ****************************************************
    // Run

    let runFactWriter (lineWidth:int) (outPath:string) (ma:FactWriter<'a>) : 'a = 
        use sw = new StreamWriter(outPath)
        apply1 ma 0 sw lineWidth |> fst

    /// Implemented in CPS 
    let mapM (mf: 'a -> FactWriter<'b>) 
             (source:'a list) : FactWriter<'b list> = 
        FactWriter <| fun state handle lineWidth -> 
            let rec work (st1:int) (xs:'a list) (cont : int -> 'b list -> 'b list * int) = 
                match xs with
                | [] -> cont st1 []
                | y :: ys -> 
                    let (ans1, st2) = apply1 (mf y) st1 handle lineWidth
                    work st2 ys (fun st3 anslist ->
                    cont st3 (ans1::anslist))
            work state source (fun s ans -> (ans, s))

    /// Implemented in CPS 
    let mapMz (mf: 'a -> FactWriter<'b>) 
              (source:'a list) : FactWriter<unit> = 
        FactWriter <| fun state handle lineWidth -> 
            let rec work (st1:int) (xs:'a list) (cont : int -> unit * int) = 
                match xs with
                | [] -> cont st1
                | y :: ys -> 
                    let (_, st2) = apply1 (mf y) st1 handle lineWidth
                    work st2 ys (fun st3 ->
                    cont st3)
            work state source (fun s -> ((), s))

    let replicateM (count:int) (ma:FactWriter<'a>) : FactWriter<'a list> = 
        FactWriter <| fun state handle lineWidth -> 
            let rec work (st1:int) (i:int) (cont : int -> 'a list -> 'a list * int) = 
                if i <= 0 then 
                    cont st1 []
                else
                    let (ans1, st2) = apply1 ma st1 handle lineWidth
                    work st2 (i-1) (fun st3 anslist ->
                    cont st3 (ans1::anslist))
            work state count (fun s ans -> (ans, s))


    let replicateMz (count:int) (ma:FactWriter<'a>) : FactWriter<unit> = 
        FactWriter <| fun state handle lineWidth -> 
            let rec work (st1:int) (i:int) (cont : int -> unit * int) = 
                if i <= 0 then 
                    cont st1
                else
                    let (_, st2) = apply1 ma st1 handle lineWidth
                    work st2 (i-1) (fun st3 ->
                    cont st3)
            work state count (fun s -> ((), s))

    let tellDoc (doc:Doc) : FactWriter<unit> =
        FactWriter <| fun st handle lineWidth ->
            let text = render lineWidth doc
            handle.WriteLine text
            ((), st)

    let newline : FactWriter<unit> = 
        tellDoc emptyDoc

    let newlines (count:int) : FactWriter<unit> = 
        replicateMz count newline

    let comment (body:string) : FactWriter<unit> = 
        tellDoc (ppComment body)

    let directive (body:Directive) : FactWriter<unit> = 
        tellDoc (ppDirective body)

    let predicate (body:Predicate) : FactWriter<unit>  =
        tellDoc (ppPredicate body)