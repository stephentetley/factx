// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.PathString

open System

[<AutoOpen>]
module PathString = 
    open System

    type PathString = 
        val Separator : string
        val Steps : string []

        private new (separator:string, pathSteps:string []) = 
            { Separator = separator
            ; Steps = pathSteps }

        new (separator:string, path:string) = 
            { Separator = separator
            ; Steps = path.Split(separator = [| separator |], 
                                 options = System.StringSplitOptions.None ) }

        member v.Output () : string = 
            String.concat v.Separator v.Steps

        member v.Subpath(pathStart: int, length: int) : PathString = 
            let subArr = 
                if pathStart + length > v.Steps.Length then 
                    Array.sub v.Steps pathStart (v.Steps.Length - pathStart)
                else 
                    Array.sub v.Steps pathStart length

            new PathString ( separator = v.Separator, pathSteps = subArr) 

        member v.HasStep(step:string) : bool = 
            Array.exists (fun s -> s = step) v.Steps
            
    let pathString (separator:string) (path:string) : PathString = 
        new PathString(separator = separator, path = path)

    let subpath (start:int) (length:int) (path:PathString) : PathString = 
        path.Subpath(start, length)
       
    let output (path:PathString) = path.Output()

    let hasStep(step:string) (path:PathString) : bool = 
        path.HasStep(step)