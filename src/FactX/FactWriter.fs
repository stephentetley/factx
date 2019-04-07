// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

module FactWriter = 

    open System.IO
    open SLFormat.Pretty

    open FactX.Internal.Syntax
    open FactX

    type ErrMsg = string

    /// TODO - potentially add indent level
    type FactWriter<'a> = 
        FactWriter of (StreamWriter -> int -> 'a)


    let inline private apply1 (ma: FactWriter<'a>) 
                              (handle: StreamWriter) 
                              (lineWidth:int) : 'a= 
        let (FactWriter f) = ma in f handle lineWidth


    let inline mreturn (x:'a) : FactWriter<'a> = 
        FactWriter <| fun _ _ -> x


    let inline private bindM (ma: FactWriter<'a>) 
                        (f :'a -> FactWriter<'b>) : FactWriter<'b> =
        FactWriter <| fun handle lineWidth -> 
            let x = apply1 ma handle lineWidth in apply1 (f x) handle lineWidth

    /// Haskell's (>>)
    let inline private combineM (mfirst:FactWriter<'a>) 
                                (msecond:FactWriter<'b>) : FactWriter<'b> = 
        FactWriter <| fun handle lineWidth -> 
            let _ =  apply1 mfirst handle lineWidth in apply1 msecond handle lineWidth


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

    /// This runs the finalizer on userResources
    let runRewriteMonad (outPath:string) (lineWidth:int) (ma:FactWriter<'a>) : 'a = 
        use sw = new StreamWriter(outPath)
        apply1 ma sw lineWidth


    let tellDoc (doc:Doc) : FactWriter<unit> =
        FactWriter <| fun handle lineWidth ->
            let text = render lineWidth doc
            handle.WriteLine line


    let moduleDirective (modName:string) (exports:string list) : FactWriter<unit> =  
        let prolog = FactOutput.moduleDirective modName exports
        tellDoc (ppDirective prolog)