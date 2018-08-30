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

    let facts : FactSet = 
        { FactName = "adb_ultrasonic_inst"
          Arity = 4
          Signature = "adb_ultrasonic_inst(uid, site_name, path, op_status)."
          Comment = ""
          Clauses = ultrasonics |> List.map (equipmentClause "adb_ultrasonic_inst") } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "adb_ultrasonic_insts"
          GlobalComment = "adb_ultrasonic_insts.pl"
          Exports = db |> List.map (fun a -> a.ExportSignature) 
          Database = db }

    pmodule.Save(outFile)


let genFlowMeters (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_flow_meters.pl"
    
    let flowMeters = 
        List.filter (fun (row:AssetRow) -> isFlowMeterAdb row.``Common Name``) allRows

    let facts : FactSet = 
        { FactName = "adb_flow_meter"
          Arity = 4
          Signature = "adb_flow_meter(uid, site_name, path, op_status)."
          Comment = ""
          Clauses = flowMeters |> List.map (equipmentClause "adb_flow_meter") } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "adb_flow_meters"
          GlobalComment = "adb_flow_meters.pl"
          Exports = db |> List.map (fun a -> a.ExportSignature)          
          Database = db }

    pmodule.Save(outFile)

let genPressureInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_pressure_insts.pl"
    
    let doInsts = 
        List.filter (fun (row:AssetRow) -> isPressureInstAdb row.``Common Name``) allRows

    let facts : FactSet = 
        { FactName = "adb_pressure_inst"
          Arity = 4
          Signature = "adb_pressure_inst(uid, site_name, path, op_status)."
          Comment = ""
          Clauses = doInsts |> List.map (equipmentClause "adb_pressure_inst") } 
    
    let pmodule : Module = 
        let db = [facts]
        { ModuleName = "adb_pressure_insts"
          GlobalComment = "adb_pressure_insts.pl"
          Exports = db |> List.map (fun a -> a.ExportSignature) 
          Database = db}

    pmodule.Save(outFile)

let genDissolvedOxygenInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_dissolved_oxygen_insts.pl"
    
    let doInsts = 
        List.filter (fun (row:AssetRow) -> isDissolvedOxygenInstAdb row.``Common Name``) allRows

    let facts : FactSet = 
        { FactName = "adb_dissolved_oxygen_inst"
          Arity = 4
          Signature = "adb_dissolved_oxygen_inst(uid, site_name, path, op_status)."
          Comment = ""
          Clauses = doInsts |> List.map (equipmentClause "adb_dissolved_oxygen_inst") } 
    
    let pmodule : Module = 
        let m = makeModule "adb_dissolved_oxygen_insts" "adb_dissolved_oxygen_insts.pl"
        m.AddFacts(facts)


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

          
    let facts : FactSet = 
        { FactName = "adb_installation"
          Arity = 1
          Signature = "adb_installation(installation_name)."
          Comment = ""
          Clauses = getInstallations allRows |> List.map makeClause } 

    let pmodule : Module = 
        (makeModule "adb_installations" "adb_installations.pl").AddFacts(facts)


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


// ** TEMP ** 



    
type SimpleTable = 
    CsvProvider< Schema = "Site Name (string),Instrument Type(string),AI2 Asset Ref(string),Common Name(string)",
                 HasHeaders = false >

type SimpleRow = SimpleTable.Row

let makeSimpleRow (instType:string) (row:AssetRow) : SimpleRow = 
    SimpleTable.Row( siteName = installationNameFromPath row.``Common Name``
                   , instrumentType = instType
                   , ai2AssetRef = row.Reference
                   , commonName = row.``Common Name`` )

let genCsv (inputFile:string) : unit = 
    let outputFile : string = 
        let name1 = System.IO.Path.GetFileName(inputFile)
        let name2 = System.IO.Path.Combine(@"G:\work\Projects\uquart\output", name1) 
        System.IO.Path.ChangeExtension(name2, "csv")

    let xlsxRows = readAssetSpeadsheet inputFile

    let ultrasonics = 
        List.filter (fun (row:AssetRow) -> isLevelControlAdb row.``Common Name``) xlsxRows
            |> List.map (makeSimpleRow "ULTRASONIC")
    
    let flowInsts = 
        List.filter (fun (row:AssetRow) -> isFlowMeterAdb row.``Common Name``) xlsxRows
            |> List.map (makeSimpleRow "FLOW METER")

    let pressureInsts = 
        List.filter (fun (row:AssetRow) -> isPressureInstAdb row.``Common Name``) xlsxRows
            |> List.map (makeSimpleRow "PRESSURE INST")

    let doInsts = 
        List.filter (fun (row:AssetRow) -> isDissolvedOxygenInstAdb row.``Common Name``) xlsxRows
            |> List.map (makeSimpleRow "DISSOLVED OXYGEN")

    let table = new SimpleTable(ultrasonics @ flowInsts @ pressureInsts @ doInsts) 
    use sw = new System.IO.StreamWriter(outputFile)
    sw.WriteLine "Site Name,Instrument Type,AI2 Asset Ref,Common Name"
    table.Save(writer = sw, separator = ',', quote = '"' )

    

let temp01 () = 
    let allAssetFiles = getFilesMatching @"G:\work\Projects\uquart\site-data\AssetDB" "AI*.xlsx"

    List.iter genCsv allAssetFiles


