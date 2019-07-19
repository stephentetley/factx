// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"
#r "System.Xml.Linq"
open System.IO


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


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190712\lib\netstandard2.0"
#r "SLFormat"


#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Syntax.fs"
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src-extra\FactX\Extra\Skeletons.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.FactWriter
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
          with member this.TableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new SaiTable())

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\common_data\prolog", filename) 

// signature = "site_name(uid, common_name)." 
let siteName2 (row:SaiRow) : Predicate = 
    predicate "site_name"
                [ quotedAtom row.InstReference
                ; stringTerm row.InstCommonName 
                ]

// ignature = "asset_type(uid, type)."
let assetType2 (row:SaiRow) : Predicate = 
    predicate "asset_type"
                [ quotedAtom row.InstReference
                ; quotedAtom row.AssetType 
                ]

// signature = "asset_status(uid, status)."
let assetStatus2 (row:SaiRow) : Predicate = 
    predicate "asset_status"
                [ quotedAtom row.InstReference
                ; quotedAtom row.AssetStatus 
                ]



let genSiteFacts (rows:SaiRow list) : unit = 
    let outFile = outputFileName "sai_facts.pl"
    runFactWriter 160 outFile 
        <|  factWriter {
            do! tellComment "sai_facts.pl"
            do! newline
            do! tellDirective (moduleDirective "sai_facts" ["site_name/2"; "asset_type/2"; "asset_status/2"])
            do! newline
            do! mapMz (tellPredicate << siteName2) rows
            do! newline
            do! mapMz (tellPredicate << assetType2) rows
            do! newline
            do! mapMz (tellPredicate << assetStatus2) rows
            do! newline
            return ()
        }



    
// *************************************
// Oustation facts

    
type OustationTable = 
    CsvProvider< "G:\work\common_data\outstations.2018-07-06.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type OutstationRow = OustationTable.Row

let readOutstationRows () : OutstationRow list = 
    (new OustationTable()).Rows |> Seq.toList

// signature = "os_name(od_name, outstation_name)."
let osName2 (row:OutstationRow) : Predicate = 
    predicate "os_name"
                [ quotedAtom row.``OD name``
                ; quotedAtom    row.``OS name`` 
                ]

// signature = "os_type(od_name, os_type)."
let osType2 (row:OutstationRow) : Predicate = 
    predicate "os_type"
                [ quotedAtom row.``OD name``
                ; quotedAtom row.``OS type`` 
                ]

// ignature = "od_comment(od_name, comment)."
let odComment2 (row:OutstationRow) : Predicate = 
    predicate "od_comment"
                [ quotedAtom row.``OD name``
                ; quotedAtom row.``OD comment`` 
                ]


let genOsFacts (rows:OutstationRow list) : unit = 
    let outFile = outputFileName "os_facts.pl"
    runFactWriter 160 outFile 
        <|  factWriter {
            do! tellComment "os_facts.pl"
            do! newline
            do! tellDirective (moduleDirective "os_facts" ["os_name/2"; "os_type/2"; "od_comment/2"])
            do! newline
            do! mapMz (tellPredicate << osName2) rows
            do! newline
            do! mapMz (tellPredicate << osType2) rows
            do! newline
            do! mapMz (tellPredicate << odComment2) rows
            do! newline
            return ()
        }


let main () : unit = 
     readSaiRowRows ()      |> genSiteFacts
     readOutstationRows ()  |> genOsFacts
