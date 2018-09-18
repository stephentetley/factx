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

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\OldFactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
#load "..\FactX\FactX\Extra\ValueReader.fs"
open FactX
open FactX.Extra.ExcelProviderHelper

type InstTable = 
    ExcelFile< FileName = @"G:\work\AI2-exports\STANDARD_INST.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type InstRow = InstTable.Row

/// ExcelProvider can read "data" files not just the file the type was 
/// instantiated with.
let readInstSpreadsheet (filename:string) (sheetname:string) : InstRow list = 
    let helper = 
        { new IExcelProviderHelper<InstTable,InstRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new InstTable(filename,sheetname))

let table1 () = readInstSpreadsheet @"G:\work\AI2-exports\BE_NO 2 STF.xlsx" "Sheet1"
let table2 () = readInstSpreadsheet @"G:\work\AI2-exports\BE_NO 2 STW.xlsx" "Sheet1"

type AssetType = 
    | BusinessUnit | System | Function | Installation | ProcessGroup
    | Process | PlantAssembly | PlantItem

let decodeHKey (hkey:string) : option<AssetType> = 
    match hkey with
    | null -> None
    | _ -> 
        match hkey.Length with
        | 2 -> Some BusinessUnit
        | 4 -> Some System
        | 8 -> Some Function 
        | 13 -> Some Installation
        | 20 -> Some ProcessGroup
        | 24 -> Some Process
        | 31 -> Some PlantAssembly
        | 36 -> Some PlantItem
        | _ -> None


let test01 () = 
    table1 () 
        |> List.iter (fun row -> printfn "%A" (decodeHKey row.``Hierarchy Key``))
