// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

open System.IO 


#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\Internal\FactWriter.fs"
#load "FactX\Utils\ExcelProviderHelper.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal.FactWriter
open FactX.Utils.ExcelProviderHelper

#load @"PropRtu.fs"
open PropRtu


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\rts\facts", filename) 


// *************************************
// Mimic facts


type MimicTable = 
    ExcelFile< FileName = @"G:\work\Projects\uquart\rts-data\rts-mimic-list.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type MimicRow = MimicTable.Row


let readMimicRows () : MimicRow list = 
    let helper = 
        { new IExcelProviderHelper<MimicTable,MimicRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new MimicTable())




let factMimicName2 (row:MimicRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "rts_mimic_name")  
                        [ quotedAtom row.``Mimic ID``
                        ; prologString row.Name
                        ]



let genMimicNameFacts (rows:MimicRow list) : unit = 
    let outfile = outputFile "rts_mimic_names.pl"
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_mimic_names.pl"
            do! tell <| moduleDirective "rts_mimic_names" 
                            [ "rts_mimic_name", 2
                            ]
            do! mapMz factMimicName2 rows
            return () 
        }
    runFactWriter outfile procAll

    
// *************************************
// Points facts

    
type PointsTable = 
    CsvProvider< Sample = @"G:\work\Projects\uquart\rts-data\points-sample.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type PointsRow = PointsTable.Row

let readPoints (sourcePath:string) : PointsRow list = 
    let sheet = PointsTable.Load(uri=sourcePath)
    sheet.Rows |> Seq.toList



let factMimicPoint3 (row:PointsRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "rts_mimic_point")  
                        [ quotedAtom (row.``Ctrl pic  Alarm pic``)
                        ; quotedAtom (getOsName row.``OS\Point name``)
                        ; quotedAtom (getPointName row.``OS\Point name``)
                        ]



let genMimicPoints (rows:PointsRow list) : unit = 
    let outfile = outputFile "rts_mimic_points.pl"
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_mimic_points.pl"
            do! tell <| moduleDirective "rts_mimic_points" 
                        [ "rts_mimic_point", 3
                        ]
            do! tell <| prologComment "rts_mimic_point(picture, os_name, point_name)."
            do! mapMz factMimicPoint3 rows
            return () 
            }
    runFactWriter outfile procAll




let getFilesMatching (sourceDirectory:string) (pattern:string) : string list =
    DirectoryInfo(sourceDirectory).GetFiles(searchPattern = pattern) 
        |> Array.map (fun (info:FileInfo)  -> info.FullName)
        |> Array.toList


        
// *************************************
// asset_to_signal

// signal is a suffix, one of _P,_R,_F,_A

type AssetToSignal = 
    { OsName: string
      AssetName: string
      PointName: string
      SignalSuffix: string }

let optAssetToSignal (row:PointsRow) : option<AssetToSignal> = 
    if hasSuffixAFPR row.``OS\Point name`` then
        Some <| 
            let pointName = getPointName row.``OS\Point name``
            { OsName = getOsName row.``OS\Point name``;
              AssetName = uptoSuffix '_' pointName;
              PointName = pointName;
              SignalSuffix = suffixOf '_' pointName }
    else None

let getAssetToSignals (rows:PointsRow list) : AssetToSignal list = 
    List.choose id <| List.map optAssetToSignal rows

let factAssetToSignal (source:AssetToSignal) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "asset_to_signal")  
                        [ quotedAtom    <| source.OsName
                        ; quotedAtom    <| source.AssetName
                        ; quotedAtom    <| source.PointName
                        ; quotedAtom    <| source.SignalSuffix
                        ]


let genAssetToSignals (source:AssetToSignal list) : unit = 
    let outfile = outputFile "rts_asset_to_signal.pl"
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_asset_to_signal.pl"
            do! tell <| moduleDirective "rts_asset_to_signal" 
                            [ "asset_to_signal", 4
                            ]
            do! tell <| prologComment "asset_to_signal(osname, assetname, signalname, suffix)."
            do! mapMz factAssetToSignal source
            return () 
            }
    runFactWriter outfile procAll
/// Name include Outstation:
/// "THORNTON_DALE_STW   \INLET_BRUSH_SCREEN_R" => "THORNTON_DALE_STW   \INLET_BRUSH_SCREEN"

/// The map maps stem to its suffixes.
type StemPoints = Map<string,string list>

let getStemPoints (rowMatch:PointsRow -> bool) (rows:PointsRow list) : StemPoints = 
    let oper (ac:StemPoints) (row:PointsRow) : StemPoints = 
        if rowMatch row && hasSuffixAFPR row.``OS\Point name`` then
            let name = uptoSuffix '_' row.``OS\Point name``
            let suffix = suffixOf '_' row.``OS\Point name``
            match Map.tryFind name ac with
            | Some xs -> Map.add name (suffix::xs) ac
            | None -> Map.add name [suffix] ac
        else ac
    List.fold oper Map.empty rows

// *************************************
// Pump facts

let getPumpPoints (rows:PointsRow list) : StemPoints = 
    let matcher (row:PointsRow) : bool = isPump (getPointName row.``OS\Point name``)
    getStemPoints matcher rows
    

let factPumpPoints (qualName:string, pointCodes:string list)  : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "rts_pump")  
                        [ quotedAtom    <| getOsName qualName
                        ; quotedAtom    <| getPointName qualName
                        ; prologList    <| List.map quotedAtom pointCodes
                        ]

let genPumpFacts (pumpPoints:StemPoints) : unit = 
    let outfile = outputFile "rts_pump_facts.pl"
    let pumps = Map.toList pumpPoints
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_pump_facts.pl"
            do! tell <| moduleDirective "rts_pump_facts" 
                        [ "rts_pump", 3
                        ]
            do! tell <| prologComment "rts_pump(osname, pump_name, point_codes)."
            do! mapMz factPumpPoints pumps
            return () 
            }
    runFactWriter outfile procAll





// *************************************
// Screen facts

let getScreenPoints (rows:PointsRow list) : StemPoints = 
    let matcher (row:PointsRow) : bool = isScreen (getPointName row.``OS\Point name``)
    getStemPoints matcher rows

let factScreenPoints (qualName:string, pointCodes:string list)  : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "rts_screen")  
                        [ quotedAtom    <| getOsName qualName
                        ; quotedAtom    <| getPointName qualName
                        ; prologList    <| List.map quotedAtom pointCodes
                        ]

let genScreenFacts (screenPoints:StemPoints) : unit = 
    let outfile = outputFile "rts_screen_facts.pl"
    let screens = Map.toList screenPoints
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_screen_facts.pl"
            do! tell <| moduleDirective "rts_screen_facts" 
                        [ "rts_screen", 3
                        ]
            do! tell <| prologComment "rts_screen(osname, screen_name, point_codes)."
            do! mapMz factScreenPoints screens
            return () 
            }
    runFactWriter outfile procAll


// *************************************
// Outstation facts

let getOutstations (rows:PointsRow list) : string list = 
    let step (ac:Set<string>) (row:PointsRow) : Set<string> = 
        let osName = getOsName row.``OS\Point name``
        if not (ac.Contains osName) then 
            ac.Add osName
        else ac
    
    List.fold step Set.empty rows 
        |> Set.toList

let factOutstation (name:string)  : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "rts_outstation")  
                        [ quotedAtom name ]


let genOutstationFacts (allRows:PointsRow list) : unit = 
    let outfile = outputFile "rts_outstations.pl"
    let outstations = getOutstations allRows
    let procAll : FactWriter<unit> = 
        factWriter {
            do! tell <| prologComment "rts_outstations.pl"
            do! tell <| moduleDirective "rts_outstations" 
                        [ "rts_outstation", 1
                        ]
            do! tell <| prologComment "rts_outstation(osname)."
            do! mapMz factOutstation outstations
            return () 
            }
    runFactWriter outfile procAll


// *************************************
// Main

let main () : unit = 
     readMimicRows () |> genMimicNameFacts

     let allPointsFiles = getFilesMatching @"G:\work\Projects\uquart\rts-data" "*-rtu-points.csv"
     let allPoints = 
        List.map readPoints allPointsFiles |> List.concat

     allPoints |> genMimicPoints
     allPoints |> genOutstationFacts

     // Pumps pump/3
     allPoints |> getPumpPoints |> genPumpFacts
     allPoints |> getScreenPoints |> genScreenFacts
     allPoints |> getAssetToSignals |> genAssetToSignals

