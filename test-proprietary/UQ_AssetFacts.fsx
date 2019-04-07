// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"
open System.IO 


#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"


#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel

#I @"C:\Users\stephen\.nuget\packages\FSharp.Data\3.0.1\lib\netstandard2.0"
#r @"FSharp.Data.dll"
open FSharp.Data


#load "..\src\Old\FactX\Internal\PrintProlog.fs"
#load "..\src\Old\FactX\Internal\PrologSyntax.fs"
#load "..\src\Old\FactX\FactOutput.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src-extra\FactX\Extra\PathString.fs"
open Old.FactX
open FactX.Extra.ExcelProviderHelper

#load @"Proprietary.fs"
open Proprietary


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\_old\asset\facts", filename) 


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


let equipmentClause (equipmentSig:string)  (row:AssetRow) : Option<Clause> = 
    Clause.optionCons(signature = equipmentSig
                     , body = [ optPrologSymbol     row.Reference
                              ; optPrologString     <| installationNameFromPath row.``Common Name`` 
                              ; optPrologString     row.``Common Name`` 
                              ; optPrologSymbol     row.AssetStatus ] )

let genUltrasonicInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_ultrasonic_insts.pl"
    
    let clause (row:AssetRow) : option<Clause> = 
        let signature = "adb_ultrasonic_inst(uid, site_name, path, op_status)."
        equipmentClause signature row
       
    let ultrasonics = 
        List.filter (fun (row:AssetRow) -> isLevelControlAdb row.``Common Name``) allRows

    let facts : FactBase = 
        ultrasonics |> List.map clause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("adb_ultrasonic_insts", "adb_ultrasonic_insts.pl", facts)

    pmodule.Save(outFile)


let genFlowMeters (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_flow_meters.pl"

    let clause (row:AssetRow) : option<Clause> = 
        let signature = "adb_flow_meter(uid, site_name, path, op_status)."
        equipmentClause signature row
            
    let flowMeters = 
        List.filter (fun (row:AssetRow) -> isFlowMeterAdb row.``Common Name``) allRows

    let facts : FactBase = 
        flowMeters |> List.map clause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("adb_flow_meters", "adb_flow_meters.pl", facts)

    pmodule.Save(outFile)

let genPressureInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_pressure_insts.pl"

    let clause (row:AssetRow) : option<Clause> = 
        let signature = "adb_pressure_inst(uid, site_name, path, op_status)."
        equipmentClause signature row

    let pressureInsts = 
        List.filter (fun (row:AssetRow) -> isPressureInstAdb row.``Common Name``) allRows

    let facts : FactBase = 
        pressureInsts |> List.map clause |> FactBase.ofOptionList
    
    let pmodule : Module = 
        new Module ("adb_pressure_insts", "adb_pressure_insts.pl", facts)

    pmodule.Save(outFile)


let genDissolvedOxygenInsts (allRows:AssetRow list) : unit = 
    let outFile = outputFile "adb_dissolved_oxygen_insts.pl"
    
    let clause (row:AssetRow) : option<Clause> = 
        let signature = "adb_dissolved_oxygen_inst(uid, site_name, path, op_status)."
        equipmentClause signature row

    let doxyInsts = 
        List.filter (fun (row:AssetRow) -> isDissolvedOxygenInstAdb row.``Common Name``) allRows

    let facts : FactBase = 
        doxyInsts |> List.map clause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("adb_dissolved_oxygen_insts", "adb_dissolved_oxygen_insts.pl", facts)


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

    let instClause (name:string) : option<Clause> = 
        Clause.optionCons( signature = "adb_installation(installation_name)."
                         , body = [ optPrologSymbol name ] )
     
    let facts : FactBase =  
        getInstallations allRows |> List.map instClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("adb_installations", "adb_installations.pl", facts)

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



