// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

// Helper functions for dealing with a proprietry data source.

module Proprietary

open System.IO 
open System.Text.RegularExpressions



// *************************************
// General utils


let getFilesMatching (sourceDirectory:string) (pattern:string) : string list =
    DirectoryInfo(sourceDirectory).GetFiles(searchPattern = pattern) 
        |> Array.map (fun (info:FileInfo)  -> info.FullName)
        |> Array.toList


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
    
/// Input is "DUDLINGTON/STF/.."
let installationNameFromPath (source:string) : string = 
    let splits : string [] = source.Split([| '/' |])
    if splits.Length >= 2 then 
        String.concat "/" splits.[0 .. 1]
    else
        source


// *************************************
// Pumps & Screens

let isRegexMatch1(source:string) (regex:string) : bool = 
    Regex.Match(source, regex).Success

let isRegexMatch(source:string) (regexs:string list) : bool = 
    List.exists (fun regex -> Regex.Match(source, regex).Success) regexs

let hasSuffixAFPR (pointName:string) : bool = 
    isRegexMatch1 pointName "_[AFPR]\Z"

let isPRF (pointName:string) : bool = 
    isRegexMatch1 pointName "_[FPR]\Z"

let isPumpRtu (pointName:string) : bool = 
    isRegexMatch pointName [ "^PU?MP_"; "_PU?MP_" ]


let isScreenRtu (pointName:string) : bool = 
    isRegexMatch pointName [ "^SCREEN_"; "_SCREEN_" ]

let isLevelControlAdb (path:string) : bool = 
    isRegexMatch1 path "EQUIPMENT: ULTRASONIC LEVEL INSTRUMENT\Z"

let isFlowMeterAdb (path:string) : bool = 
    isRegexMatch1 path "EQUIPMENT: MAGNETIC FLOW INSTRUMENT\Z"

let isPressureInstAdb (path:string) : bool = 
    isRegexMatch path [ "EQUIPMENT: PRESSURE FOR LEVEL INSTRUMENT\Z"
                      ; "EQUIPMENT: PRESSURE INSTRUMENT\Z"
                      ; "EQUIPMENT: PRESSURE FLOW INSTRUMENT\Z"
                      ]

let isDissolvedOxygenInstAdb (path:string) : bool = 
    isRegexMatch1 path "EQUIPMENT: DISSOLVED OXYGEN INSTRUMENT\Z"


                      