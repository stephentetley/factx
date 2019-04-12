// Copyright (c) Stephen Tetley 2019
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
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.FactWriter
open FactX.Extra.ExcelProviderHelper

// ********** DATA SETUP **********

type OsTable = 
    ExcelFile< @"G:\work\Projects\events2\adb\adb-outstations-for-edm2.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type OsRow = OsTable.Row

let readOutstations () : OsRow list = 
    let helper = 
        { new IExcelProviderHelper<OsTable,OsRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new OsTable()) 

let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)


// "outstation(uid:atom, adb_path:string, status:atom)."
let outstation3 (row:OsRow) : Predicate = 
    predicate "outstation" 
                [ quotedAtom row.Reference
                ; stringTerm row.``Common Name``
                ; quotedAtom row.AssetStatus
                ]

let writeListing (rows: OsRow list) (outPath:string) : unit =
    let justfile = FileInfo(outPath).Name
    runFactWriter 160 outPath 
        <|  factWriter {
            do! tellComment justfile
            do! newline
            do! tellDirective (moduleDirective "edm_outstations" ["outstation/3"])
            do! newline
            do! mapMz (tellPredicate << outstation3) rows
            do! newline
            return ()
        }

let main () = 
    let outFile = makeOutputPath "edm_outstations.pl"
    readOutstations () 
        |> fun rows -> writeListing rows outFile
