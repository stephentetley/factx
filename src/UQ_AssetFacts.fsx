// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

open System.IO 


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

#load @"PropUtils.fs"
open PropUtils


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\asset\facts", filename) 


type AssetTable = 
    ExcelFile< @"G:\work\Projects\uquart\site-data\AssetDB\adb-site-sample.xlsx",
               ForceString = true >

type AssetRow = AssetTable.Row

let readAssetSpeadsheet (sourcePath:string) : AssetRow list = 
    let helper = 
        { new IExcelProviderHelper<AssetTable,AssetRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new AssetTable(sourcePath))



