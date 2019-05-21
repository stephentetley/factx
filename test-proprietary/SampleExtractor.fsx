﻿// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"
open System.IO



#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190322\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Syntax.fs"
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.FactWriter
open FactX.Extra.ExcelProviderHelper

// ********** DATA SETUP **********

type InstallationsTable = 
    ExcelFile< @"G:\work\Projects\events2\er-report\Installations.xlsx",
                SheetName = "Installations",
                ForceString = true >

type InstallationsRow = InstallationsTable.Row

let excelHelper : IExcelProviderHelper<InstallationsTable,InstallationsRow> = 
    { new IExcelProviderHelper<InstallationsTable,InstallationsRow>
        with 
            member this.TableRows table = table.Data 
            member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         


let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)


let address2 (row:InstallationsRow) : Predicate = 
    predicate "address" 
                [ quotedAtom row.InstReference
                ; stringTerm row.``Full Address`` 
                ]

let makeSkeleton (outPath:string) = 
    { OutputPath = outPath
      ModuleName = "addresses"
      Exports = ["address/2"]
      PredicateComment = "address(id:atom, address_text:string)."
      ExcelReader = excelHelper
      RowFact = Some << address2
    }


let main () = 
    let skeleton = makeSkeleton (makeOutputPath "addresses.pl")
    excelSingleSheetToFacts skeleton (new InstallationsTable())
