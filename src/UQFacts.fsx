// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

open System.IO 


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

#load @"PropRtu.fs"
open PropRtu

// *************************************
// Picture facts


type PictureTable = 
    ExcelFile< FileName = @"G:\work\Projects\uquart\rts-data\rts-picture-list.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type PictureRow = PictureTable.Row


let readPictureRows () : PictureRow list = 
    let helper = 
        { new IExcelProviderHelper<PictureTable,PictureRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new PictureTable())

let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\uquart\facts", filename) 



let factPictureName (row:PictureRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "pictureName")  
                    [ quotedAtom row.``Picture ID``
                    ; prologString row.Name
                    ]



let genPictureFacts (rows:PictureRow list) : unit = 
    let outfile = outputFile "picture_facts.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "picture_facts.pl"
            do! tell <| moduleDirective "picture_facts" 
                            [ "pictureName", 2
                            ]
            do! mapMz factPictureName rows
            return () 
        }
    runFactOutput outfile procAll

    
// *************************************
// Points facts

    
type PointsTable = 
    CsvProvider< Sample = @"G:\work\Projects\uquart\rts-data\points-sample.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type PointsRow = PointsTable.Row

let readPoints (sourcePath:string) : PointsRow list = 
    let sheet = PointsTable.Load(uri=sourcePath)
    sheet.Rows |> Seq.toList



let factPoint3 (row:PointsRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "point")  
                    [ quotedAtom (getOsName row.``OS\Point name``)
                    ; quotedAtom (getPointName row.``OS\Point name``)
                    ; quotedAtom (row.``Ctrl pic  Alarm pic``)
                    ]



//let factOdComment (row:OutstationRow) : FactOutput<unit> = 
//     tell <| fact (simpleAtom "odComment")  
//                    [ quotedAtom    row.``OD name``
//                    ; prologString  row.``OD comment``
//                    ]


let genPointFacts (rows:PointsRow list) : unit = 
    let outfile = outputFile "point_facts.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "point_facts.pl"
            do! tell <| moduleDirective "point_facts" 
                        [ "point", 3
                        ]
            do! tell <| comment "point(osname, name, picture)."
            do! mapMz factPoint3 rows
            return () 
            }
    runFactOutput outfile procAll




let getFilesMatching (sourceDirectory:string) (pattern:string) : string list =
    DirectoryInfo(sourceDirectory).GetFiles(searchPattern = pattern) 
        |> Array.map (fun (info:FileInfo)  -> info.FullName)
        |> Array.toList

/// Name incluse Outstation:
/// "THORNTON_DALE_STW   \INLET_BRUSH_SCREEN_R" => "THORNTON_DALE_STW   \INLET_BRUSH_SCREEN"

type PumpPoints = Map<string,string list>

let getPumpPoints (rows:PointsRow list) : PumpPoints = 
    let oper (ac:PumpPoints) (row:PointsRow) : PumpPoints = 
        if isPump row.``OS\Point name`` then
            let name = uptoSuffix '_' row.``OS\Point name``
            // let n1 = getPointName row.``OS\Point name``
            let suffix = suffixOf '_' row.``OS\Point name``
            match Map.tryFind name ac with
            | Some xs -> Map.add name (suffix::xs) ac
            | None -> Map.add name [suffix] ac
        else ac
    List.fold oper Map.empty rows

let factPumpPoints (qualName:string, points:string list)  : FactOutput<unit> = 
     tell <| fact (simpleAtom "pump")  
                    [ quotedAtom    <| getOsName qualName
                    ; quotedAtom    <| getPointName qualName
                    ; prologList    <| List.map quotedAtom points
                    ]

let genPumpFacts (pumpPoints:PumpPoints) : unit = 
    let outfile = outputFile "pump_facts.pl"
    let pumps = Map.toList pumpPoints
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "pump_facts.pl"
            do! tell <| moduleDirective "pump_facts" 
                        [ "pump", 3
                        ]
            do! tell <| comment "point(name, osname, points)."
            do! mapMz factPumpPoints pumps
            return () 
            }
    runFactOutput outfile procAll




let main () : unit = 
     readPictureRows () |> genPictureFacts

     let pointFiles = getFilesMatching @"G:\work\Projects\uquart\rts-data" "*-rtu-points.csv"
     let allPoints = 
        List.map  readPoints pointFiles |> List.concat

     allPoints |> genPointFacts
     // Pumps pump/3
     allPoints |> getPumpPoints |> genPumpFacts



let test01 () = 
    printfn "%s" <| suffixOf    '_' @"INLET_BRUSH_SCREEN_R"
    printfn "%s" <| uptoSuffix  '_' @"INLET_BRUSH_SCREEN_R"
    printfn "%A" <| isPRF "INLET_BRUSH_SCREEN_R"
    printfn "%A" <| isPRF "INLET_BRUSH_SCREEN_S"
    printfn "%A" <| isPump "PUMP_1_R"
    printfn "%A" <| isPump "INLET_BRUSH_SCREEN_S"
    printfn "%A" <| isScreen "INLET_BRUSH_SCREEN_S"
    printfn "%A" <| isScreen "PUMP_1_R"
    