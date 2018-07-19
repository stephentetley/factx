// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#load "FactX\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
#load "FactX\ExcelProviderHelper.fs"
open FactX.FormatCombinators
open FactX.FactOutput
open FactX.ExcelProviderHelper

// *************************************
// SAI facts


type SaiTable = 
    ExcelFile< FileName = @"G:\work\common_data\SAINumbers.xlsx",
                SheetName = "SITES",
                ForceString = true >

type SaiRow = SaiTable.Row


let getSaiRowRows () : SaiRow list = 
    let dict () = 
        { new IExcelProviderHelper<SaiTable,SaiRow>
          with member this.GetTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelGetRowsAsList (dict ()) (new SaiTable())

let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\common_data\prolog", filename) 



let factSiteName (row:SaiRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "siteName")  
                    [ quotedAtom row.InstReference
                    ; prologString row.InstCommonName
                    ]

let factAssetType (row:SaiRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "assetType")  
                    [ quotedAtom row.InstReference
                    ; quotedAtom row.AssetType
                    ]

let factAssetStatus (row:SaiRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "assetStatus")  
                    [ quotedAtom row.InstReference
                    ; quotedAtom row.AssetStatus
                    ]


let genSiteRelations (rows:SaiRow list) : unit = 
    let outfile = outputFile "sai_relations.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            let! _ = tell <| comment "sai_relations.pl"
            let! _ = tell <| moduleDirective "sai_relations" 
                        [ "siteName", 2
                        ; "assetType", 2
                        ; "assetStatus", 2
                        ]
            let! _ = mapMz factSiteName rows
            let! _ = mapMz factAssetType rows
            let! _ = mapMz factAssetStatus rows
            return () 
            }
    runFactOutput outfile procAll

    
// *************************************
// Oustation facts

    
type OustationTable = 
    CsvProvider< "G:\work\common_data\outstations.2018-07-06.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type OutstationRow = OustationTable.Row

let getOutstationRows () : OutstationRow list = 
    (new OustationTable()).Rows |> Seq.toList


let factOsName (row:OutstationRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "osName")  
                    [ quotedAtom row.``OD name``
                    ; quotedAtom row.``OS name``
                    ]

let factOsType (row:OutstationRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "osType")  
                    [ quotedAtom    row.``OD name``
                    ; quotedAtom    row.``OS type``
                    ]

let factOdComment (row:OutstationRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "odComment")  
                    [ quotedAtom    row.``OD name``
                    ; string        row.``OD comment``
                    ]


let genOsRelations (rows:OutstationRow list) : unit = 
    let outfile = outputFile "os_relations.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            let! _ = tell <| comment "os_relations.pl"
            let! _ = tell <| moduleDirective "os_relations" 
                        [ "osName", 2
                        ; "osType", 2
                        ; "odComment", 2
                        ]
            let! _ = mapMz factOsName rows
            let! _ = mapMz factOsType rows
            let! _ = mapMz factOdComment rows
            return () 
            }
    runFactOutput outfile procAll

let main () : unit = 
     getSaiRowRows () |> genSiteRelations
     getOutstationRows () |> genOsRelations
