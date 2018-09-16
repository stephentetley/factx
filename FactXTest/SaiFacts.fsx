// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#I @"..\packages\ExcelProvider.1.0.1\typeproviders\fsharp41\net45"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
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


let siteNameHelper : IFactHelper<SaiRow> = 
    { new IFactHelper<SaiRow> with 
        member this.Signature = "site_name(uid, common_name)."
        member this.ClauseBody row = 
            [ PQuotedAtom   row.InstReference
            ; PString       row.InstCommonName ]
    }


let assetTypeHelper : IFactHelper<SaiRow> = 
    { new IFactHelper<SaiRow> with 
        member this.Signature = "asset_type(uid, type)."
        member this.ClauseBody row = 
            [ PQuotedAtom   row.InstReference
            ; PQuotedAtom   row.AssetType ]
    }

let assetStatusHelper : IFactHelper<SaiRow> = 
    { new IFactHelper<SaiRow> with 
        member this.Signature = "asset_status(uid, status)."
        member this.ClauseBody row = 
            [ PQuotedAtom row.InstReference
            ; PQuotedAtom row.AssetStatus ]
    }



let genSiteFacts (rows:SaiRow list) : unit = 
    let outFile = outputFileName "sai_facts.pl"

    let siteNames : FactSet     = rows |> makeFactSet siteNameHelper
    let assetTypes : FactSet    = rows |> makeFactSet assetTypeHelper
    let assetStatus : FactSet   = rows |> makeFactSet assetStatusHelper

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


let osNameHelper : IFactHelper<OutstationRow> = 
    { new IFactHelper<OutstationRow> with 
        member this.Signature = "os_name(od_name, outstation_name)."
        member this.ClauseBody row = 
            [ PQuotedAtom    row.``OD name``
            ; PQuotedAtom    row.``OS name`` ]
    }

let osTypeHelper : IFactHelper<OutstationRow> = 
    { new IFactHelper<OutstationRow> with 
        member this.Signature = "os_type(od_name, os_type)."
        member this.ClauseBody row = 
            [ PQuotedAtom    row.``OD name``
            ; PQuotedAtom    row.``OS type`` ]
    }


let odCommentHelper : IFactHelper<OutstationRow> = 
    { new IFactHelper<OutstationRow> with 
        member this.Signature = "od_comment(od_name, comment)."
        member this.ClauseBody row = 
            [ PQuotedAtom   row.``OD name``
            ; PString       row.``OD comment`` ]
        }


let genOsFacts (rows:OutstationRow list) : unit = 
    let outFile = outputFileName "os_facts.pl"
    
    let osNames : FactSet   = rows |> makeFactSet osNameHelper
    let osTypes : FactSet   = rows |> makeFactSet osTypeHelper
    let comments : FactSet  = rows |> makeFactSet odCommentHelper

    let pmodule : Module = 
        new Module ("os_facts", "os_facts.pl", [osNames; osTypes; comments])

    pmodule.Save(outFile)



let main () : unit = 
     readSaiRowRows ()      |> genSiteFacts
     readOutstationRows ()  |> genOsFacts
