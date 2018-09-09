﻿// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

open System.IO 


#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
#load "FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.Extra.ExcelProviderHelper

#load @"PropUtils.fs"
open PropUtils


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\rts\facts", filename) 


// *************************************
// Mimic facts


type MimicTable = 
    ExcelFile< FileName = @"G:\work\Projects\uquart\site-data\RTS\rts-mimic-list.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type MimicRow = MimicTable.Row


let readMimicRows () : MimicRow list = 
    let helper = 
        { new IExcelProviderHelper<MimicTable,MimicRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new MimicTable())


let genMimicNameFacts (rows:MimicRow list) : unit = 
    let outFile = outputFile "rts_mimic_names.pl"

    let mimicNameHelper : IFactHelper<MimicRow> = 
        { new IFactHelper<MimicRow> with
            member this.Signature = "rts_mimic_name(mimic_id, mimic_name)."
            member this.ClauseBody row = 
                [ PQuotedAtom   row.``Mimic ID``
                ; PString       row.Name ]
        }

    let facts : FactSet = 
        readMimicRows () |> makeFactSet mimicNameHelper
    
    let pmodule : Module = 
        new Module ("rts_mimic_names", "rts_mimic_names.pl", facts)
    
    pmodule.Save(outFile)


    
// *************************************
// Points facts

    
type PointsTable = 
    CsvProvider< Sample = @"G:\work\Projects\uquart\site-data\RTS\points-sample.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type PointsRow = PointsTable.Row

let readPoints (sourcePath:string) : PointsRow list = 
    let sheet = PointsTable.Load(uri=sourcePath)
    sheet.Rows |> Seq.toList



let genMimicPoints (rows:PointsRow list) : unit = 
    let outFile = outputFile "rts_mimic_points.pl"

    let mimicPointHelper : IFactHelper<PointsRow> = 
        { new IFactHelper<PointsRow> with
            member this.Signature = "rts_mimic_point(picture, os_name, point_name)."
            member this.ClauseBody row = 
                [ PQuotedAtom (row.``Ctrl pic  Alarm pic``)
                ; PQuotedAtom (getOsName row.``OS\Point name``)
                ; PQuotedAtom (getPointName row.``OS\Point name``) ]
        }

    let facts : FactSet = 
        rows |> makeFactSet mimicPointHelper

    let pmodule : Module = 
        let db = [facts]
        new Module ("rts_mimic_points", "rts_mimic_points.pl", facts) 

    pmodule.Save(outFile)





        
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


let genAssetToSignals (source:AssetToSignal list) : unit = 
    let outFile = outputFile "rts_asset_to_signal.pl"

    let assetSignalHelper : IFactHelper<AssetToSignal> = 
        { new IFactHelper<AssetToSignal> with
            member this.Signature = "asset_to_signal(os_name, asset_name, signal_name, suffix)."
            member this.ClauseBody row = 
                [ PQuotedAtom    <| row.OsName
                ; PQuotedAtom    <| row.AssetName
                ; PQuotedAtom    <| row.PointName
                ; PQuotedAtom    <| row.SignalSuffix ]
        }

    let facts : FactSet = 
        source |> makeFactSet assetSignalHelper
    
    let pmodule : Module = 
        new Module ("rts_asset_to_signal", "rts_asset_to_signal.pl", facts) 

    pmodule.Save(outFile)



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
    let matcher (row:PointsRow) : bool = isPumpRtu (getPointName row.``OS\Point name``)
    getStemPoints matcher rows
    

let genPumpFacts (pumpPoints:StemPoints) : unit = 
    let outFile = outputFile "rts_pump_facts.pl"
    
    let pumps = Map.toList pumpPoints
    
    let pumpPointsHelper : IFactHelper<string * string list> = 
        { new IFactHelper<string * string list> with
            member this.Signature = "rts_pump(osname, pump_name, point_codes)."
            member this.ClauseBody arg = 
                match arg with
                | (qualName, pointCodes) -> 
                    [ PQuotedAtom    <| getOsName qualName
                    ; PQuotedAtom    <| getPointName qualName
                    ; PList          <| List.map PQuotedAtom pointCodes
                    ]
        }
        
    let facts : FactSet = 
        pumps |> makeFactSet pumpPointsHelper
    
    let pmodule : Module = 
        new Module ("rts_pump_facts", "rts_pump_facts.pl", facts) 

    pmodule.Save(outFile)





// *************************************
// Screen facts

let getScreenPoints (rows:PointsRow list) : StemPoints = 
    let matcher (row:PointsRow) : bool = isScreenRtu (getPointName row.``OS\Point name``)
    getStemPoints matcher rows


let genScreenFacts (screenPoints:StemPoints) : unit = 
    let outFile = outputFile "rts_screen_facts.pl"

    let screens = Map.toList screenPoints

    let screenPointsHelper : IFactHelper<string * string list> = 
        { new IFactHelper<string * string list> with
            member this.Signature = "rts_screen(os_name, screen_name, point_codes)."
            member this.ClauseBody arg = 
                match arg with
                | (qualName, pointCodes) -> 
                    [ PQuotedAtom    <| getOsName qualName
                    ; PQuotedAtom    <| getPointName qualName
                    ; PList          <| List.map PQuotedAtom pointCodes
                    ]
        }


    let facts : FactSet = 
        screens |> makeFactSet screenPointsHelper
    
    let pmodule : Module = 
        new Module ("rts_screen_facts", "rts_screen_facts.pl", facts)

    pmodule.Save(outFile)



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


let genOutstationFacts (allRows:PointsRow list) : unit = 
    let outFile = outputFile "rts_outstations.pl"

    let outstationHelper : IFactHelper<string> = 
        { new IFactHelper<string> with
            member this.Signature = "rts_outstation(os_name)."
            member this.ClauseBody row = 
                [ PQuotedAtom row ]
        }
        
    let facts : FactSet = 
        getOutstations allRows |> makeFactSet outstationHelper

    let pmodule : Module = 
        new Module ("rts_outstations", "rts_outstations.pl", facts) 

    pmodule.Save(outFile)



// *************************************
// Main

let main () : unit = 
     readMimicRows () |> genMimicNameFacts

     let allPointsFiles = getFilesMatching @"G:\work\Projects\uquart\site-data\RTS" "*-rtu-points.csv"
     let allPoints = 
        List.map readPoints allPointsFiles |> List.concat

     allPoints |> genMimicPoints
     allPoints |> genOutstationFacts

     // Pumps pump/3
     allPoints |> getPumpPoints |> genPumpFacts
     allPoints |> getScreenPoints |> genScreenFacts
     allPoints |> getAssetToSignals |> genAssetToSignals

