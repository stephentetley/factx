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
    System.IO.Path.Combine(@"D:\coding\prolog\asset\facts", filename) 


type AssetTable = 
    ExcelFile< @"G:\work\Projects\uquart\site-data\AssetDB\adb-site-sample.xlsx",
               ForceString = true >

type AssetRow = AssetTable.Row

let readAssetSpeadsheet (sourcePath:string) : AssetRow list = 
    let helper = 
        { new IExcelProviderHelper<AssetTable,AssetRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new AssetTable(sourcePath))

let equipmentClause (factName:string) (row:AssetRow) : Clause = 
        { FactName = factName  
          Values = [ PQuotedAtom    <| row.Reference
                   ; PQuotedAtom    <| siteNameFromPath row.``Common Name`` 
                   ; PQuotedAtom    <| row.``Common Name`` 
                   ; PQuotedAtom    <| row.AssetStatus 
                   ] }


let genPumpFacts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_ultrasonics.pl"
    
    let ultrasonics = 
        List.filter (fun (row:AssetRow) -> isLevelControlAdb row.``Common Name``) allRows

    let facts : FactCollection = 
        { FactName = "adb_ultrasonic"
          Arity = 4
          Signature = "adb_ultrasonic(uid, site_name, path, op_status)."
          Clauses = ultrasonics |> List.map (equipmentClause "adb_ultrasonic") } 
    
    let pmodule : Module = 
        { ModuleName = "adb_ultrasonics"
          GlobalComment = "adb_ultrasonics.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)


let genFlowMeterFacts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_flow_meters.pl"
    
    let flowMeters = 
        List.filter (fun (row:AssetRow) -> isFlowMeterAdb row.``Common Name``) allRows

    let facts : FactCollection = 
        { FactName = "adb_flow_meter"
          Arity = 4
          Signature = "adb_flow_meter(uid, site_name, path, op_status)."
          Clauses = flowMeters |> List.map (equipmentClause "adb_flow_meter") } 
    
    let pmodule : Module = 
        { ModuleName = "adb_flow_meters"
          GlobalComment = "adb_flow_meters.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)

let main () = 
    let allAssetFiles = getFilesMatching @"G:\work\Projects\uquart\site-data\AssetDB" "AI*.xlsx"
    let allRows = 
        allAssetFiles |> List.map readAssetSpeadsheet |> List.concat
    
    genPumpFacts allRows
    genFlowMeterFacts allRows
