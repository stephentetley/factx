// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\Internal\FactWriter.fs"
#load "FactX\ExcelProviderHelper.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal.FactWriter
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

let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\common_data\prolog", filename) 



let factSiteName (row:SaiRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "site_name")  
                        [ quotedAtom row.InstReference
                        ; prologString row.InstCommonName
                        ]

let factAssetType (row:SaiRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "asset_type")  
                        [ quotedAtom row.InstReference
                        ; quotedAtom row.AssetType
                        ]

let factAssetStatus (row:SaiRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "asset_status")  
                        [ quotedAtom row.InstReference
                        ; quotedAtom row.AssetStatus
                        ]


let genSiteFacts (rows:SaiRow list) : unit = 
    let outfile = outputFile "sai_facts.pl"
    let procAll : FactWriter<unit> = 
        factWriter {
            let! _ = tell <| comment "sai_facts.pl"
            let! _ = tell <| moduleDirective "sai_facts" 
                        [ "site_name", 2
                        ; "asset_type", 2
                        ; "asset_status", 2
                        ]
            let! _ = mapMz factSiteName rows
            let! _ = mapMz factAssetType rows
            let! _ = mapMz factAssetStatus rows
            return () 
            }
    runFactWriter outfile procAll

    
// *************************************
// Oustation facts

    
type OustationTable = 
    CsvProvider< "G:\work\common_data\outstations.2018-07-06.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type OutstationRow = OustationTable.Row

let readOutstationRows () : OutstationRow list = 
    (new OustationTable()).Rows |> Seq.toList


let factOsName (row:OutstationRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "os_name")  
                        [ quotedAtom row.``OD name``
                        ; quotedAtom row.``OS name``
                        ]

let factOsType (row:OutstationRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "os_type")  
                        [ quotedAtom    row.``OD name``
                        ; quotedAtom    row.``OS type``
                        ]

let factOdComment (row:OutstationRow) : FactWriter<unit> = 
     tell <| prologFact (simpleAtom "od_comment")  
                        [ quotedAtom    row.``OD name``
                        ; prologString  row.``OD comment``
                        ]


let genOsFacts (rows:OutstationRow list) : unit = 
    let outfile = outputFile "os_facts.pl"
    let procAll : FactWriter<unit> = 
        factWriter {
            let! _ = tell <| comment "os_facts.pl"
            let! _ = 
                tell <| moduleDirective "os_facts" 
                                [ "os_name", 2
                                ; "os_type", 2
                                ; "od_comment", 2
                                ]
            let! _ = mapMz factOsName rows
            let! _ = mapMz factOsType rows
            let! _ = mapMz factOdComment rows
            return () 
            }
    runFactWriter outfile procAll

let main () : unit = 
     readSaiRowRows () |> genSiteFacts
     readOutstationRows () |> genOsFacts
