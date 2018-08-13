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
                   ; PQuotedAtom    <| installationNameFromPath row.``Common Name`` 
                   ; PQuotedAtom    <| row.``Common Name`` 
                   ; PQuotedAtom    <| row.AssetStatus 
                   ] }


let genUltrasonicInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_ultrasonic_insts.pl"
    
    let ultrasonics = 
        List.filter (fun (row:AssetRow) -> isLevelControlAdb row.``Common Name``) allRows

    let facts : FactCollection = 
        { FactName = "adb_ultrasonic_inst"
          Arity = 4
          Signature = "adb_ultrasonic_inst(uid, site_name, path, op_status)."
          Clauses = ultrasonics |> List.map (equipmentClause "adb_ultrasonic_inst") } 
    
    let pmodule : Module = 
        { ModuleName = "adb_ultrasonic_insts"
          GlobalComment = "adb_ultrasonic_insts.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)


let genFlowMeters (allRows:AssetRow list) : unit = 
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

let genPressureInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_pressure_insts.pl"
    
    let doInsts = 
        List.filter (fun (row:AssetRow) -> isPressureInstAdb row.``Common Name``) allRows

    let facts : FactCollection = 
        { FactName = "adb_pressure_inst"
          Arity = 4
          Signature = "adb_pressure_inst(uid, site_name, path, op_status)."
          Clauses = doInsts |> List.map (equipmentClause "adb_pressure_inst") } 
    
    let pmodule : Module = 
        { ModuleName = "adb_pressure_insts"
          GlobalComment = "adb_pressure_insts.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)

let genDissolvedOxygenInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_dissolved_oxygen_insts.pl"
    
    let doInsts = 
        List.filter (fun (row:AssetRow) -> isDissolvedOxygenInstAdb row.``Common Name``) allRows

    let facts : FactCollection = 
        { FactName = "adb_dissolved_oxygen_inst"
          Arity = 4
          Signature = "adb_dissolved_oxygen_inst(uid, site_name, path, op_status)."
          Clauses = doInsts |> List.map (equipmentClause "adb_dissolved_oxygen_inst") } 
    
    let pmodule : Module = 
        { ModuleName = "adb_dissolved_oxygen_insts"
          GlobalComment = "adb_dissolved_oxygen_insts.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)

// *************************************
// Installation facts

let getInstallations (rows:AssetRow list) : string list = 
    let step (ac:Set<string>) (row:AssetRow) : Set<string> = 
        let instName = installationNameFromPath row.``Common Name``
        if not (ac.Contains instName) then 
            ac.Add instName
        else ac
    
    List.fold step Set.empty rows 
        |> Set.toList

let genInstallationFacts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_installations.pl"

    let makeClause (name:string) : Clause = 
        { FactName = "adb_installation"  
          Values = [ PQuotedAtom name ] }

          
    let facts : FactCollection = 
        { FactName = "adb_installation"
          Arity = 1
          Signature = "adb_installation(installation_name)."
          Clauses = getInstallations allRows |> List.map makeClause } 

    let pmodule : Module = 
        { ModuleName = "adb_installations"
          GlobalComment = "adb_installations.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)

let main () = 
    let allAssetFiles = getFilesMatching @"G:\work\Projects\uquart\site-data\AssetDB" "AI*.xlsx"
    let allRows = 
        allAssetFiles |> List.map readAssetSpeadsheet |> List.concat
    
    genUltrasonicInsts allRows
    genFlowMeters allRows
    genPressureInsts allRows
    genDissolvedOxygenInsts allRows
    genInstallationFacts allRows


