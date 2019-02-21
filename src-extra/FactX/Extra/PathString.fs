// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.PathString

open System
open System.Text.RegularExpressions

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

        override v.ToString() : string = 
            String.concat v.Separator v.Steps

        member v.Clone (separator:string) : PathString = 
            new PathString (separator = separator, pathSteps = v.Steps)

        member v.Length : int = 
            v.Steps.Length


        member v.Subpath(pathStart: int, length: int) : PathString = 
            let subArr = 
                if pathStart + length > v.Steps.Length then 
                    Array.sub v.Steps pathStart (v.Steps.Length - pathStart)
                else 
                    Array.sub v.Steps pathStart length
            new PathString ( separator = v.Separator, pathSteps = subArr) 

        member v.Contains(step:string) : bool = 
            Array.exists (fun s -> s = step) v.Steps
        
        member v.ContainsRegex(pattern:string) : bool = 
            let regex = new Regex(pattern)
            Array.exists (fun s -> regex.IsMatch(s)) v.Steps


        member v.Take (count:int) : PathString = 
            let subArr = Array.take count v.Steps
            new PathString (separator = v.Separator, pathSteps = subArr)

        member v.Skip (count:int) : PathString = 
            let subArr = Array.skip count v.Steps
            new PathString (separator = v.Separator, pathSteps = subArr)
        
        member v.SkipRight (count:int) : PathString = 
            let rightEdge = v.Steps.Length - count
            let subArr = Array.sub v.Steps 0 rightEdge
            new PathString (separator = v.Separator, pathSteps = subArr)

        member v.LeftOf(position:int) : PathString = 
            let subArr = Array.take (position - 1) v.Steps
            new PathString (separator = v.Separator, pathSteps = subArr)

        member v.RightOf(position:int) : PathString = 
            let subArr = Array.skip position v.Steps
            new PathString (separator = v.Separator, pathSteps = subArr)

        member v.Index (step:string) : int = 
            Array.findIndex (fun s -> s = step) v.Steps

        member v.IndexRegex (pattern:string) : int = 
            let regex = new Regex(pattern)
            Array.findIndex (fun s -> regex.IsMatch(s)) v.Steps

        member v.TryIndex (step:string) : option<int> = 
            try 
                v.Index(step) |> Some
            with
            | _ -> None

        member v.TryIndexRegex (pattern:string) : option<int> = 
            try 
                v.IndexRegex(pattern) |> Some
            with
            | _ -> None

        member v.Between (stepLeft:string, stepRight:string) : PathString = 
            let start = v.Index(stepLeft) + 1
            let final = v.Index(stepRight)
            v.Subpath(start, final - start)
        
        member v.TryBetween (stepLeft:string, stepRight:string) : option<PathString> = 
            try 
                v.Between(stepLeft, stepRight) |> Some
            with
            | _ -> None

        member v.Last : PathString = 
            let last = v.Length
            v.RightOf(last - 1)

    let pathString (separator:string) (path:string) : PathString = 
        new PathString(separator = separator, path = path)

    let subpath (start:int) (length:int) (path:PathString) : PathString = 
        path.Subpath(start, length)
       
    let output (path:PathString) : string = path.Output()

    let contains(step:string) (path:PathString) : bool = 
        path.Contains(step)

    let containsRegex(pattern:string) (path:PathString) : bool = 
        path.ContainsRegex(pattern)


    let take (count:int) (path:PathString) : PathString = 
        path.Take(count)

    let skip (count:int) (path:PathString) : PathString = 
        path.Skip(count)

    let index (step:string) (path:PathString) : int = 
        path.Index(step)
    
    let tryIndex (step:string) (path:PathString) : option<int> = 
        path.TryIndex(step)

    let between (stepLeft:string) (stepRight:string) (path:PathString) : PathString = 
        path.Between(stepLeft, stepRight)

