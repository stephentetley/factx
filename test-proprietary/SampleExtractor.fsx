// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"
open System.IO



#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
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

let readInstallations () : InstallationsRow list = 
    let helper = 
        { new IExcelProviderHelper<InstallationsTable,InstallationsRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new InstallationsTable()) 

let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)



let address2 (row:InstallationsRow) : Predicate = 
    predicate "address" [ quotedAtom row.InstReference; stringTerm row.``Full Address`` ]

let writeListing (rows: InstallationsRow list) (outPath:string) : unit =
    let justfile = FileInfo(outPath).Name
    runFactWriter 160 outPath 
        <|  factWriter {
            do! tellComment justfile
            do! newline
            do! tellDirective (moduleDirective "addresses" ["address/2"])
            do! newline
            do! tellComment "address(id:atom, address_text:string)."
            do! mapMz (tellPredicate << address2) rows
            do! newline
            return ()
        }

let main () = 
    let outFile = makeOutputPath "addresses.pl"
    readInstallations () 
        |> fun rows -> writeListing rows outFile
