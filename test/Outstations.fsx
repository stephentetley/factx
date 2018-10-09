// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause


#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"


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

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\events2\outstations\prolog\facts", filename) 


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



let siteName (commonName:string) = 
    let path : PathString = pathString "/" commonName
    if path.Contains("SEWER MAINTENANCE") then 
        path.Subpath(2,2).Output()
    else 
        path.Subpath(0,2).Output()

let locale (commonName:string) = 
    let path : PathString = pathString "/" commonName
    match path.TryBetween("RTS MONITORING", "EQUIPMENT: TELEMETRY OUTSTATION") with
    | Some p1 -> p1.Output()
    | None -> "Unknown"
    

let outstationFacts () = 
    let outFile = outputFileName "outstations.pl"

    let rows = readOsSpreadsheet ()
    
    let outstationClause (row:OsRow) : option<Clause> = 
        Clause.optionCons( signature = "outstation(site_name, local_name, os_model)."
                         , body = [ optPrologSymbol     (siteName row.``Common Name``)
                                  ; optPrologSymbol     (locale row.``Common Name``)
                                  ; optPrologSymbol     row.Model  ])

    let outstations : FactBase  = 
        rows |> List.map outstationClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("outstations", "outstations.pl", outstations)

    pmodule.Save(outFile)


let temp01 () = 
    let path = new PathString("/", "BADMINTON VIEW 24/LMP/CONTROL SERVICES/RTS MONITORING/TELEMETRY OUTSTATION/EQUIPMENT: TELEMETRY OUTSTATION")
    printfn "%A" <| path.Subpath(0,2)
    printfn "%A" <| path.LeftOf(3)
    printfn "%A" <| path.RightOf(3)
    printfn "%A" <| path.Take(3)
    printfn "%A" <| path.Between("RTS MONITORING", "EQUIPMENT: TELEMETRY OUTSTATION")
