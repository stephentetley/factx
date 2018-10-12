// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#I @"..\packages\ExcelProvider.1.0.1\lib\net45"
#r "ExcelProvider.Runtime.dll"

#I @"..\packages\ExcelProvider.1.0.1\typeproviders\fsharp41\net45"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel

#load "..\src\FactX\Internal\FormatCombinators.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src\FactX\Extra\PathString.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.PathString

#load "Proprietary.fs"
open Proprietary

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\events2\prolog\outstations\facts", filename) 


type OsTable = 
    ExcelFile< FileName = @"G:\work\Projects\events2\outstations\OUTSTATIONS.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type OsRow = OsTable.Row


let readOsSpreadsheet () : OsRow list = 
    let helper = 
        { new IExcelProviderHelper<OsTable, OsRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
    new OsTable() |> excelReadRowsAsList helper





let locale (commonName:string) = 
    let path : PathString = pathString "/" commonName
    match path.TryBetween("RTS MONITORING", "EQUIPMENT: TELEMETRY OUTSTATION") with
    | Some p1 -> p1.Output()
    | None -> "Unknown"
    

let outstationFacts () = 
    let outFile = outputFileName "adb_outstations.pl"

    let rows = readOsSpreadsheet ()
    
    let outstationClause (row:OsRow) : option<Clause> = 
        Clause.optionCons( signature = "adb_outstation(site_name, local_name, os_model)."
                         , body = [ optPrologSymbol     (siteName row.``Common Name``)
                                  ; optPrologSymbol     (locale row.``Common Name``)
                                  ; optPrologSymbol     row.Model  ])

    let outstations : FactBase  = 
        rows |> List.map outstationClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("adb_outstations", "adb_outstations.pl", outstations)

    pmodule.Save(outFile)



// ****************************************************************************
// RTS

type RtsTable = 
    CsvProvider<Sample = @"G:\work\Projects\events2\outstations\outstations-report-10.10.18.csv",
                 HasHeaders = true >

type RtsRow = RtsTable.Row


let rtsFacts () = 
    let outFile = outputFileName "rts_outstations.pl"

    let rows = (new RtsTable ()).Rows |> Seq.toList
    
    let outstationClause (row:RtsRow) : option<Clause> = 
        Clause.optionCons( signature = "rts_outstation(uid, os_name, site_name)."
                         , body = [ optPrologSymbol     row.``OD name``
                                  ; optPrologSymbol     row.``OS name``
                                  ; optPrologSymbol     row.``OD comment``  ])

    let outstations : FactBase  = 
        rows |> List.map outstationClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("rts_outstations", "rts_outstations.pl", outstations)

    pmodule.Save(outFile)

let temp01 () = 
    let path = new PathString("/", "BADMINTON VIEW 24/LMP/CONTROL SERVICES/RTS MONITORING/TELEMETRY OUTSTATION/EQUIPMENT: TELEMETRY OUTSTATION")
    printfn "%A" <| path.Subpath(0,2)
    printfn "%A" <| path.LeftOf(3)
    printfn "%A" <| path.RightOf(3)
    printfn "%A" <| path.Take(3)
    printfn "%A" <| path.Between("RTS MONITORING", "EQUIPMENT: TELEMETRY OUTSTATION")


// ****************************************************************************
// ADB sites


type AdbTable = 
    ExcelFile< FileName = @"G:\work\Projects\events2\outstations\adb_os_names.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type AdbRow = AdbTable.Row


let readAdbSpreadsheet () : AdbRow list = 
    let helper = 
        { new IExcelProviderHelper<AdbTable, AdbRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
    new AdbTable() |> excelReadRowsAsList helper


let siteFacts () = 
    let outFile = outputFileName "adb_sites.pl"

    let rows = readAdbSpreadsheet ()
    
    let siteClause (row:AdbRow) : option<Clause> = 
        Clause.optionCons( signature = "adb_site(uid, site_name, os_name)."
                         , body = [ optPrologSymbol     row.Reference
                                  ; optPrologSymbol     row.``Common Name``
                                  ; optPrologSymbol     row.``RTS Outstation Name`` ])

    let outstations : FactBase  = 
        rows |> List.map siteClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("adb_sites", "adb_sites.pl", outstations)

    pmodule.Save(outFile)