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

// *************************************
// Picture facts


type PictureTable = 
    ExcelFile< FileName = @"G:\work\Projects\uquart\rts-data\rts-picture-list.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type PictureRow = PictureTable.Row


let readPictureRows () : PictureRow list = 
    let dict () = 
        { new IExcelProviderHelper<PictureTable,PictureRow>
          with member this.GetTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelGetRowsAsList (dict ()) (new PictureTable())

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

let strLeftOf (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    splits.[0]

let strRightOf (pivot:char) (source:string) : string = 
    let splits : string [] = source.Split(pivot)
    String.concat (pivot.ToString()) splits.[1..]

let getPointName (source:string) : string = 
    (strRightOf '\\' source).Trim()


let getOsName (source:string) : string = 
    (strLeftOf '\\' source).Trim()
    

let factPoint3 (row:PointsRow) : FactOutput<unit> = 
     tell <| fact (simpleAtom "point")  
                    [ quotedAtom (getPointName row.``OS\Point name``)
                    ; quotedAtom (getOsName row.``OS\Point name``)
                    ; quotedAtom (row.``Ctrl pic  Alarm pic``)
                    ]

//let factOsType (row:OutstationRow) : FactOutput<unit> = 
//     tell <| fact (simpleAtom "osType")  
//                    [ quotedAtom    row.``OD name``
//                    ; quotedAtom    row.``OS type``
//                    ]

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
            do! tell <| comment "point(name, osname, picture)."
            do! mapMz factPoint3 rows
            return () 
            }
    runFactOutput outfile procAll

let test01 () = 
    readPoints @"G:\work\Projects\uquart\rts-data\CHERRY_BURTON_STW-rtu-points.csv"
        |> List.iter (printfn "%A")


let getFilesMatching (sourceDirectory:string) (pattern:string) : string list =
    DirectoryInfo(sourceDirectory).GetFiles(searchPattern = pattern) 
        |> Array.map (fun (info:FileInfo)  -> info.FullName)
        |> Array.toList

let main () : unit = 
     readPictureRows () |> genPictureFacts

     let pointFiles = getFilesMatching @"G:\work\Projects\uquart\rts-data" "*-rtu-points.csv"
     let allPoints = 
        List.map  readPoints pointFiles |> List.concat

     allPoints |> genPointFacts
