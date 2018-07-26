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
    System.IO.Path.Combine(@"G:\work\Projects\uquart\prolog\facts", filename) 



let factPictureName2 (row:PictureRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "rts_picture_name")  
                    [ quotedAtom row.``Picture ID``
                    ; prologString row.Name
                    ]



let genPicNameFacts (rows:PictureRow list) : unit = 
    let outfile = outputFile "rts_picture_names.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "rts_picture_names.pl"
            do! tell <| moduleDirective "rts_picture_names" 
                            [ "rts_picture_name", 2
                            ]
            do! mapMz factPictureName2 rows
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
     tell <| fact (simpleAtom "rts_picture")  
                    [ quotedAtom (row.``Ctrl pic  Alarm pic``)
                    ; quotedAtom (getOsName row.``OS\Point name``)
                    ; quotedAtom (getPointName row.``OS\Point name``)
                    ]



let genPictureChildrenFacts (rows:PointsRow list) : unit = 
    let outfile = outputFile "rts_picture_facts.pl"
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "rts_picture_facts.pl"
            do! tell <| moduleDirective "rts_picture_facts" 
                        [ "rts_picture", 3
                        ]
            do! tell <| comment "rts_picture_points(picture, os_name, point_name)."
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

let factPumpPoints (qualName:string, pointCodes:string list)  : FactOutput<unit> = 
     tell <| fact (simpleAtom "rts_pump")  
                    [ quotedAtom    <| getOsName qualName
                    ; quotedAtom    <| getPointName qualName
                    ; prologList    <| List.map quotedAtom pointCodes
                    ]

let genPumpFacts (pumpPoints:PumpPoints) : unit = 
    let outfile = outputFile "rts_pump_facts.pl"
    let pumps = Map.toList pumpPoints
    let procAll : FactOutput<unit> = 
        factOutput {
            do! tell <| comment "rts_pump_facts.pl"
            do! tell <| moduleDirective "rts_pump_facts" 
                        [ "rts_pump", 3
                        ]
            do! tell <| comment "rts_pump(osname, pump_name, point_codes)."
            do! mapMz factPumpPoints pumps
            return () 
            }
    runFactOutput outfile procAll




let main () : unit = 
     readPictureRows () |> genPicNameFacts

     let allPointsFiles = getFilesMatching @"G:\work\Projects\uquart\rts-data" "*-rtu-points.csv"
     let allPoints = 
        List.map readPoints allPointsFiles |> List.concat

     allPoints |> genPictureChildrenFacts
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
    