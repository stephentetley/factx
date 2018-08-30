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
#load "FactX\FactOutput.fs"
#load "FactX\ExcelProviderHelper.fs"
open FactX
open FactX.ExcelProviderHelper

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

    let makeClause (row:MimicRow) : Clause = 
        { FactName = "rts_mimic_name"
          Values = [PQuotedAtom row.``Mimic ID``; PString row.Name ] }

    let facts : FactSet = 
        { FactName = "rts_mimic_name"
          Arity = 2
          Signature = "rts_mimic_name(mimic_id, mimic_name)."
          Comment = ""
          Clauses = readMimicRows () |> List.map makeClause } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_mimic_names"
          GlobalComment = "rts_mimic_names.pl"
          Exports = List.map factSignature db
          Database = db }
    
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

    let makeClause (row:PointsRow) : Clause = 
        { FactName = "rts_mimic_point"  
          Values = [ PQuotedAtom (row.``Ctrl pic  Alarm pic``)
                   ; PQuotedAtom (getOsName row.``OS\Point name``)
                   ; PQuotedAtom (getPointName row.``OS\Point name``)
                   ] }

    let facts : FactSet = 
        { FactName = "rts_mimic_point"
          Arity = 2
          Signature = "rts_mimic_point(picture, os_name, point_name)."
          Comment = ""
          Clauses = rows |> List.map makeClause } 

    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_mimic_points"
          GlobalComment = "rts_mimic_points.pl"
          Exports = List.map factSignature db
          Database = db }

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

    let makeClause (atos:AssetToSignal) : Clause = 
        { FactName = "asset_to_signal"  
          Values = [ PQuotedAtom    <| atos.OsName
                   ; PQuotedAtom    <| atos.AssetName
                   ; PQuotedAtom    <| atos.PointName
                   ; PQuotedAtom    <| atos.SignalSuffix
                   ]}

    let facts : FactSet = 
        { FactName = "asset_to_signal"
          Arity = 4
          Signature = "asset_to_signal(os_name, asset_name, signal_name, suffix)."
          Comment = ""
          Clauses = source |> List.map makeClause } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_asset_to_signal"
          GlobalComment = "rts_asset_to_signal.pl"
          Exports = List.map factSignature db
          Database = db }

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
    
    let makeClause (qualName:string, pointCodes:string list) : Clause = 
        { FactName = "rts_pump"  
          Values = [ PQuotedAtom    <| getOsName qualName
                   ; PQuotedAtom    <| getPointName qualName
                   ; PList          <| List.map PQuotedAtom pointCodes
                   ] }

    let facts : FactSet = 
        { FactName = "rts_pump"
          Arity = 3
          Signature = "rts_pump(osname, pump_name, point_codes)."
          Comment = ""
          Clauses = pumps |> List.map makeClause } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_pump_facts"
          GlobalComment = "rts_pump_facts.pl"
          Exports = List.map factSignature db
          Database = db }

    pmodule.Save(outFile)





// *************************************
// Screen facts

let getScreenPoints (rows:PointsRow list) : StemPoints = 
    let matcher (row:PointsRow) : bool = isScreenRtu (getPointName row.``OS\Point name``)
    getStemPoints matcher rows


let genScreenFacts (screenPoints:StemPoints) : unit = 
    let outFile = outputFile "rts_screen_facts.pl"

    let screens = Map.toList screenPoints

    let makeClause (qualName:string, pointCodes:string list) : Clause = 
        { FactName = "rts_screen"  
          Values = [ PQuotedAtom    <| getOsName qualName
                   ; PQuotedAtom    <| getPointName qualName
                   ; PList          <| List.map PQuotedAtom pointCodes] }

    let facts : FactSet = 
        { FactName = "rts_screen"
          Arity = 3
          Signature = "rts_screen(os_name, screen_name, point_codes)."
          Comment = ""
          Clauses = screens |> List.map makeClause } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_screen_facts"
          GlobalComment = "rts_screen_facts.pl"
          Exports = List.map factSignature db
          Database = db }

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

    let makeClause (name:string) : Clause = 
        { FactName = "rts_outstation"  
          Values = [ PQuotedAtom name ] }

          
    let facts : FactSet = 
        { FactName = "rts_outstation"
          Arity = 1
          Signature = "rts_outstation(os_name)."
          Comment = ""
          Clauses = getOutstations allRows |> List.map makeClause } 

    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "rts_outstations"
          GlobalComment = "rts_outstations.pl"
          Exports = List.map factSignature db
          Database = db }

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

