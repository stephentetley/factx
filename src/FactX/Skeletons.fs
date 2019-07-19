// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX


module Skeletons = 

    open System.IO

    open FactX
    open FactX.FactWriter


    /// Predicate name must include the arity suffix.
    type PredicateSkeleton = 
        { PredicateName : string
          Comment : string
          WriteFacts : FactWriter<unit>
        }

    let private writePredicate (skeleton:PredicateSkeleton) : FactWriter<unit> = 
        factWriter { 
            do! tellComment skeleton.Comment
            do! skeleton.WriteFacts
            do! newline
            return ()
        }

    type ModuleSkeleton = 
        { OutputPath: string
          ModuleName: string
          PredicateSkeletons: PredicateSkeleton list
        } 

    
    let private collectExports (skeletons: PredicateSkeleton list) : string list = 
        skeletons |> List.map (fun a -> a.PredicateName)

    let generateModule (skeleton:ModuleSkeleton): unit =
        let justfile = FileInfo(skeleton.OutputPath).Name
        let exports = collectExports skeleton.PredicateSkeletons
        runFactWriter 160 skeleton.OutputPath 
            <|  factWriter {
                do! tellComment justfile
                do! newline
                do! tellDirective (moduleDirective skeleton.ModuleName exports)
                do! newline
                do! mapMz writePredicate skeleton.PredicateSkeletons
                return ()
            }



    let seqWriteFacts (itemFact: 'item -> Predicate option) 
                       (source: 'item seq) : FactWriter<unit> =
        Seq.toList source |> mapMz (optTellPredicate << itemFact)
