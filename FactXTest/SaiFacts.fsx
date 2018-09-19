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

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\Internal\PrologSyntax.fs"
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
open FactX.Internal
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


let siteNameHelper (row:SaiRow) : Clause = 
    { Signature = parseSignature "site_name(uid, common_name)."
      Body = [ PrologSyntax.PQuotedAtom   row.InstReference
             ; PrologSyntax.PString       row.InstCommonName ]
    }


let assetTypeHelper (row:SaiRow) : Clause = 
    { Signature = parseSignature "asset_type(uid, type)."
      Body = [ PrologSyntax.PQuotedAtom   row.InstReference
             ; PrologSyntax.PQuotedAtom   row.AssetType ]
    }

let assetStatusHelper (row:SaiRow) : Clause = 
    { Signature = parseSignature "asset_status(uid, status)."
      Body = [ PrologSyntax.PQuotedAtom row.InstReference
             ; PrologSyntax.PQuotedAtom row.AssetStatus ]
    }



let genSiteFacts (rows:SaiRow list) : unit = 
    let outFile = outputFileName "sai_facts.pl"

    let siteNames : FactBase     = rows |> List.map siteNameHelper |> FactBase.ofList
    let assetTypes : FactBase    = rows |> List.map assetTypeHelper |> FactBase.ofList
    let assetStatus : FactBase   = rows |> List.map assetStatusHelper |> FactBase.ofList

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


let osNameHelper (row:OutstationRow) : Clause = 
    { Signature = parseSignature "os_name(od_name, outstation_name)."
      Body = [ PrologSyntax.PQuotedAtom    row.``OD name``
             ; PrologSyntax.PQuotedAtom    row.``OS name`` ] }

let osTypeHelper (row:OutstationRow) : Clause = 
    { Signature = parseSignature "os_type(od_name, os_type)."
      Body = [ PrologSyntax.PQuotedAtom    row.``OD name``
             ; PrologSyntax.PQuotedAtom    row.``OS type`` ] }


let odCommentHelper (row:OutstationRow) : Clause = 
    { Signature = parseSignature "od_comment(od_name, comment)."
      Body = [ PrologSyntax.PQuotedAtom   row.``OD name``
             ; PrologSyntax.PString       row.``OD comment`` ] }


let genOsFacts (rows:OutstationRow list) : unit = 
    let outFile = outputFileName "os_facts.pl"
    
    let osNames : FactBase  = rows |> List.map osNameHelper |> FactBase.ofList
    let osTypes : FactBase  = rows |> List.map osTypeHelper |> FactBase.ofList
    let comments : FactBase = rows |> List.map odCommentHelper |> FactBase.ofList

    let pmodule : Module = 
        new Module ("os_facts", "os_facts.pl", [osNames; osTypes; comments])

    pmodule.Save(outFile)



let main () : unit = 
     readSaiRowRows ()      |> genSiteFacts
     readOutstationRows ()  |> genOsFacts
