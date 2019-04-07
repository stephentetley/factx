// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX.Internal

// Explicitly this module

module Common = 
    
    open System

    /// Splits on Environment.NewLine
    let toLines (source:string) : string list = 
        source.Split(separator=[| Environment.NewLine |], options=StringSplitOptions.None) |> Array.toList

    /// Joins with Environment.NewLine
    let fromLines (source:string list) : string = 
        String.concat Environment.NewLine source