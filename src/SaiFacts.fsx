// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

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



let clauseSiteName (row:SaiRow) : Clause = 
     { FactName = "site_name"
       Values = [ PQuotedAtom row.InstReference
                ; PString row.InstCommonName ]}

let siteNames (rows:SaiRow list) : FactSet = 
    { FactName = "site_name"
      Arity = 2
      Signature = "site_name(uid, common_name)."
      Comment = ""
      Clauses = List.map clauseSiteName rows } 


let clauseAssetType (row:SaiRow) : Clause = 
     { FactName = "asset_type"
       Values = [ PQuotedAtom row.InstReference
                ; PQuotedAtom row.AssetType ] }

let assetTypes (rows:SaiRow list) : FactSet = 
    { FactName = "asset_type"
      Arity = 2
      Signature = "asset_type(uid, type)."
      Comment = ""
      Clauses = List.map clauseAssetType rows } 

let clauseAssetStatus (row:SaiRow) : Clause = 
     { FactName = "asset_status"
       Values = [ PQuotedAtom row.InstReference
                ; PQuotedAtom row.AssetStatus ]}
                
let assetStatus (rows:SaiRow list) : FactSet = 
    { FactName = "asset_status"
      Arity = 2
      Signature = "asset_status(uid, status)."
      Comment = ""
      Clauses = List.map clauseAssetStatus rows } 

let genSiteFacts (rows:SaiRow list) : unit = 
    let outFile = outputFileName "sai_facts.pl"

    
    let pmodule : Module = 
        let db = [ siteNames rows; assetTypes rows; assetStatus rows ]
        { ModuleName = "sai_facts"
          GlobalComment = "sai_facts.pl"
          Exports = db |> List.map (fun a -> a.ExportSignature)
          Database = db }

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


let clauseOsName (row:OutstationRow) : Clause = 
    { FactName = "os_name"
      Values = [ PQuotedAtom row.``OD name``
               ; PQuotedAtom row.``OS name`` ]}

let osNames (rows:OutstationRow list) : FactSet = 
    { FactName = "os_name"
      Arity = 2
      Signature = "os_name(od_name, od_name)."
      Comment = ""
      Clauses = List.map clauseOsName rows } 

let clauseOsType (row:OutstationRow) : Clause = 
    { FactName = "os_type" 
      Values = [ PQuotedAtom    row.``OD name``
               ; PQuotedAtom    row.``OS type`` ]}

let osTypes (rows:OutstationRow list) : FactSet = 
    { FactName = "os_type"
      Arity = 2
      Signature = "os_type(od_name, os_type)."
      Comment = ""
      Clauses = List.map clauseOsType rows } 

let clauseOdComment (row:OutstationRow) : Clause = 
    { FactName = "od_comment"
      Values = [ PQuotedAtom  row.``OD name``
               ; PString  row.``OD comment`` ]}


let odComments (rows:OutstationRow list) : FactSet = 
    { FactName = "od_comment"
      Arity = 2
      Signature = "od_comment(od_name,od_comment)."
      Comment = ""
      Clauses = List.map clauseOdComment rows } 


let genOsFacts (rows:OutstationRow list) : unit = 
    let outFile = outputFileName "os_facts.pl"
    
    let pmodule : Module = 
        let db = [ osNames rows ; osTypes rows ; odComments rows ]
        { ModuleName = "os_facts"
          GlobalComment = "os_facts.pl"
          Exports = db |> List.map (fun a -> a.ExportSignature)
          Database = db}

    pmodule.Save(outFile)



let main () : unit = 
     readSaiRowRows ()      |> genSiteFacts
     readOutstationRows ()  |> genOsFacts
