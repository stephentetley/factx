// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190205\lib\netstandard2.0"
#r "SLFormat"



#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel

#I @"C:\Users\stephen\.nuget\packages\FSharp.Data\3.0.0\lib\netstandard2.0"
#r @"FSharp.Data.dll"
open FSharp.Data


#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.Extra.ExcelProviderHelper



// *************************************
// SAI facts


type SaiTable = 
    ExcelFile< FileName = @"G:\work\common_data\SAINumbers.xlsx",
                SheetName = "SITES",
                ForceString = true >

type SaiRow = SaiTable.Row


let readSaiRowRows () : SaiRow list = 
    let helper = 
        { new IExcelProviderHelper<SaiTable,SaiRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new SaiTable())

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\common_data\prolog", filename) 


let siteNameClause (row:SaiRow) : option<Clause> = 
    Clause.optionCons( signature = "site_name(uid, common_name)."
                     , body = [ optPrologSymbol     row.InstReference
                              ; optPrologString     row.InstCommonName ] )


let assetTypeClause (row:SaiRow) : option<Clause> = 
    Clause.optionCons( signature = "asset_type(uid, type)."
                     , body = [ optPrologSymbol     row.InstReference
                              ; optPrologSymbol  row.AssetType ] )

let assetStatusClause (row:SaiRow) : option<Clause> = 
    Clause.optionCons( signature = "asset_status(uid, status)."
                     , body = [ optPrologSymbol row.InstReference
                              ; optPrologSymbol row.AssetStatus ] )



let genSiteFacts (rows:SaiRow list) : unit = 
    let outFile = outputFileName "sai_facts.pl"

    let siteNames : FactBase     = 
        rows |> List.map siteNameClause |> FactBase.ofOptionList

    let assetTypes : FactBase    = 
        rows |> List.map assetTypeClause |> FactBase.ofOptionList

    let assetStatus : FactBase   = 
        rows |> List.map assetStatusClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("sai_facts", "sai_facts.pl", [ siteNames; assetTypes; assetStatus ])


    pmodule.Save(outFile)


    
// *************************************
// Oustation facts

    
type OustationTable = 
    CsvProvider< "G:\work\common_data\outstations.2018-07-06.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type OutstationRow = OustationTable.Row

let readOutstationRows () : OutstationRow list = 
    (new OustationTable()).Rows |> Seq.toList


let osNameClause (row:OutstationRow) : option<Clause> = 
    Clause.optionCons( signature = "os_name(od_name, outstation_name)."
                     , body = [ optPrologSymbol    row.``OD name``
                              ; optPrologSymbol    row.``OS name`` ] )

let osTypeClause (row:OutstationRow) : option<Clause> = 
    Clause.optionCons( signature = "os_type(od_name, os_type)."
                     , body = [ optPrologSymbol    row.``OD name``
                              ; optPrologSymbol    row.``OS type`` ] )


let odCommentClause (row:OutstationRow) : option<Clause> = 
    Clause.optionCons( signature = "od_comment(od_name, comment)."
                     , body = [ optPrologSymbol     row.``OD name``
                              ; optPrologSymbol     row.``OD comment`` ] )


let genOsFacts (rows:OutstationRow list) : unit = 
    let outFile = outputFileName "os_facts.pl"
    
    let osNames : FactBase  = 
        rows |> List.map osNameClause |> FactBase.ofOptionList

    let osTypes : FactBase  = 
        rows |> List.map osTypeClause |> FactBase.ofOptionList

    let comments : FactBase = 
        rows |> List.map odCommentClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("os_facts", "os_facts.pl", [osNames; osTypes; comments])

    pmodule.Save(outFile)



let main () : unit = 
     readSaiRowRows ()      |> genSiteFacts
     readOutstationRows ()  |> genOsFacts
