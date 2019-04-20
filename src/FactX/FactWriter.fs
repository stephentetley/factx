// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

// Open explicitly as we use some good names that we shouldn't
// have exclusive purchase on.

module FactWriter = 

    open System
    open System.IO
    
    open SLFormat.Pretty

    open FactX
    open FactX.Pretty

    // type ErrMsg = string

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


    let tellDoc (doc:Doc) : FactWriter<unit> =
        FactWriter <| fun st handle lineWidth ->
            let text = render lineWidth doc
            handle.WriteLine text
            ((), st)

    // ****************************************************
    // Monadic Operations

    /// fmap 
    let fmapM (fn:'a -> 'b) (ma:FactWriter<'a>) : FactWriter<'b> = 
        FactWriter <| fun st handle lineWidth -> 
           let (a, st1) =  apply1 ma st handle lineWidth in (fn a, st1)
           

    // liftM (which is fmap)
    let liftM (fn:'a -> 'x) (ma:FactWriter<'a>) : FactWriter<'x> = 
        fmapM fn ma

    let liftM2 (fn:'a -> 'b -> 'x) 
               (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) : FactWriter<'x> = 
        factWriter { 
            let! a = ma
            let! b = mb
            return (fn a b)
        }

    let liftM3 (fn:'a -> 'b -> 'c -> 'x) 
               (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) : FactWriter<'x> = 
        factWriter { 
            let! a = ma
            let! b = mb
            let! c = mc
            return (fn a b c)
        }

    let liftM4 (fn:'a -> 'b -> 'c -> 'd -> 'x) 
               (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) : FactWriter<'x> = 
        factWriter { 
            let! a = ma
            let! b = mb
            let! c = mc
            let! d = md
            return (fn a b c d)
        }


    let liftM5 (fn:'a -> 'b -> 'c -> 'd -> 'e -> 'x) 
               (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) 
               (me:FactWriter<'e>) : FactWriter<'x> = 
        factWriter { 
            let! a = ma
            let! b = mb
            let! c = mc
            let! d = md
            let! e = me
            return (fn a b c d e)
        }

    let liftM6 (fn:'a -> 'b -> 'c -> 'd -> 'e -> 'f -> 'x) 
               (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) 
               (me:FactWriter<'e>) 
               (mf:FactWriter<'f>) : FactWriter<'x> = 
        factWriter { 
            let! a = ma
            let! b = mb
            let! c = mc
            let! d = md
            let! e = me
            let! f = mf
            return (fn a b c d e f)
        }


    let tupleM2 (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) : FactWriter<'a * 'b> = 
        liftM2 (fun a b -> (a,b)) ma mb

    let tupleM3 (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) 
                (mc:FactWriter<'c>) : FactWriter<'a * 'b * 'c> = 
        liftM3 (fun a b c -> (a,b,c)) ma mb mc

    let tupleM4 (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) 
                (mc:FactWriter<'c>) 
                (md:FactWriter<'d>) : FactWriter<'a * 'b * 'c * 'd> = 
        liftM4 (fun a b c d -> (a,b,c,d)) ma mb mc md

    let tupleM5 (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) 
                (mc:FactWriter<'c>) 
                (md:FactWriter<'d>) 
                (me:FactWriter<'e>) : FactWriter<'a * 'b * 'c * 'd * 'e> = 
        liftM5 (fun a b c d e -> (a,b,c,d,e)) ma mb mc md me

    let tupleM6 (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) 
                (mc:FactWriter<'c>) 
                (md:FactWriter<'d>) 
                (me:FactWriter<'e>) 
                (mf:FactWriter<'f>) : FactWriter<'a * 'b * 'c * 'd * 'e * 'f> = 
        liftM6 (fun a b c d e f -> (a,b,c,d,e,f)) ma mb mc md me mf

    let pipeM2 (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (fn:'a -> 'b -> 'x) : FactWriter<'x> = 
        liftM2 fn ma mb

    let pipeM3 (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (fn:'a -> 'b -> 'c -> 'x) : FactWriter<'x> = 
        liftM3 fn ma mb mc

    let pipeM4 (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) 
               (fn:'a -> 'b -> 'c -> 'd -> 'x) : FactWriter<'x> = 
        liftM4 fn ma mb mc md

    let pipeM5 (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) 
               (me:FactWriter<'e>) 
               (fn:'a -> 'b -> 'c -> 'd -> 'e ->'x) : FactWriter<'x> = 
        liftM5 fn ma mb mc md me

    let pipeM6 (ma:FactWriter<'a>) 
               (mb:FactWriter<'b>) 
               (mc:FactWriter<'c>) 
               (md:FactWriter<'d>) 
               (me:FactWriter<'e>) 
               (mf:FactWriter<'f>) 
               (fn:'a -> 'b -> 'c -> 'd -> 'e -> 'f -> 'x) : FactWriter<'x> = 
        liftM6 fn ma mb mc md me mf

    /// Left biased choice, if ``ma`` succeeds return its result, otherwise try ``mb``.
    let altM (ma:FactWriter<'a>) (mb:FactWriter<'a>) : FactWriter<'a> = 
        combineM ma mb


    /// Haskell Applicative's (<*>)
    let apM (mf:FactWriter<'a ->'b>) (ma:FactWriter<'a>) : FactWriter<'b> = 
        factWriter { 
            let! fn = mf
            let! a = ma
            return (fn a) 
        }



    /// Perform two actions in sequence. 
    /// Ignore the results of the second action if both succeed.
    let seqL (ma:FactWriter<'a>) (mb:FactWriter<'b>) : FactWriter<'a> = 
        factWriter { 
            let! a = ma
            let! b = mb
            return a
        }

    /// Perform two actions in sequence. 
    /// Ignore the results of the first action if both succeed.
    let seqR (ma:FactWriter<'a>) (mb:FactWriter<'b>) : FactWriter<'b> = 
        factWriter { 
            let! a = ma
            let! b = mb
            return b
        }


    let kleisliL (mf:'a -> FactWriter<'b>)
                 (mg:'b -> FactWriter<'c>)
                 (source:'a) : FactWriter<'c> = 
        factWriter { 
            let! b = mf source
            let! c = mg b
            return c
        }

    /// Flipped kleisliL
    let kleisliR (mf:'b -> FactWriter<'c>)
                 (mg:'a -> FactWriter<'b>)
                 (source:'a) : FactWriter<'c> = 
        factWriter { 
            let! b = mg source
            let! c = mf b
            return c
        }


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



    // ****************************************************
    // Monadic operators

    /// Bind operator
    let ( >>= ) (ma:FactWriter<'a>) 
              (fn:'a -> FactWriter<'b>) : FactWriter<'b> = 
        bindM ma fn

    /// Flipped Bind operator
    let ( =<< ) (fn:'a -> FactWriter<'b>) 
              (ma:FactWriter<'a>) : FactWriter<'b> = 
        bindM ma fn


    /// Operator for fmap.
    let ( |>> ) (ma:FactWriter<'a>) (fn:'a -> 'b) : FactWriter<'b> = 
        fmapM fn ma

    /// Flipped fmap.
    let ( <<| ) (fn:'a -> 'b) (ma:FactWriter<'a>) : FactWriter<'b> = 
        fmapM fn ma


    /// Operator for seqL
    let ( .>> ) (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) : FactWriter<'a> = 
        seqL ma mb

    /// Operator for seqR
    let ( >>. ) (ma:FactWriter<'a>) 
                (mb:FactWriter<'b>) : FactWriter<'b> = 
        seqR ma mb



    /// Operator for kleisliL
    let ( >=> ) (mf : 'a -> FactWriter<'b>)
                (mg : 'b -> FactWriter<'c>)
                (source:'a) : FactWriter<'c> = 
        kleisliL mf mg source


    /// Operator for kleisliR
    let ( <=< ) (mf : 'b -> FactWriter<'c>)
                (mg : 'a -> FactWriter<'b>)
                (source:'a) : FactWriter<'c> = 
        kleisliR mf mg source


    // ****************************************************
    // Fact output
    let newline : FactWriter<unit> = 
        tellDoc emptyDoc

    let newlines (count:int) : FactWriter<unit> = 
        replicateMz count newline

    let tellComment (body:string) : FactWriter<unit> = 
        tellDoc (ppComment body)

    let tellDirective (body:Directive) : FactWriter<unit> = 
        tellDoc (ppDirective body)

    let tellPredicate (body:Predicate) : FactWriter<unit>  =
        tellDoc (ppPredicate body)


    let optTellPredicate (optBody:Predicate option) : FactWriter<unit>  =
        match optBody with
        | None -> mreturn ()
        | Some body -> tellDoc (ppPredicate body)

    let timestamp : FactWriter<unit> = 
        DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") |> tellComment
        