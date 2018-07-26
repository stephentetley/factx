// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

// Helper functions for dealing with a proprietry data source.

module PropRtu


open System.Text.RegularExpressions

// *************************************
// General utils


let private strLeftOf (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    splits.[0]


/// Pivot is first found from the left. 
let private strRightOf (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    String.concat (pivot.ToString()) splits.[1..]

/// Pivot is last found from the left. 
let suffixOf (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    let last = splits.Length - 1
    if last >= 0 then 
        splits.[last]
    else
        ""

/// Pivot is last found from the left. 
let uptoSuffix (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    let last = splits.Length - 1
    if last >= 0 then 
        String.concat (pivot.ToString()) splits.[0 .. (last-1)]
    else
        source

// *************************************
// Names

/// Input is "SITE_NAME \POINT_NAME"
let getPointName (source:string) : string = 
    (strRightOf '\\' source).Trim()

/// Input is "SITE_NAME \POINT_NAME"
let getOsName (source:string) : string = 
    (strLeftOf '\\' source).Trim()
    

// *************************************
// Pumps & Screens

let hasSuffixAFPR (pointName:string) : bool = 
    Regex.Match(pointName, "_[AFPR]\Z").Success

let isPRF (pointName:string) : bool = 
    Regex.Match(pointName, "_[FPR]\Z").Success

let isPump (pointName:string) : bool = 
    Regex.Match(pointName, "^PU?MP_").Success || Regex.Match(pointName, "_PU?MP_").Success


let isScreen (pointName:string) : bool = 
    Regex.Match(pointName, "^SCREEN_").Success || Regex.Match(pointName, "_SCREEN_").Success

