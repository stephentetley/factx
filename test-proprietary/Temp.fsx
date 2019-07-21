// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

#r "netstandard"
open System.IO



#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190721\lib\netstandard2.0"
#r "SLFormat"

#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Syntax.fs"
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src\FactX\Skeletons.fs"
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
          with member this.TableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new OsTable()) 


type OsNames = 
    ExcelFile< @"G:\work\Projects\events2\adb\adb-outstations-names-for-edm2.xlsx",
                SheetName = "Sheet1",
                ForceString = true >

type OsNameRow = OsNames.Row

let readOsNames () : OsNameRow list = 
    let helper = 
        { new IExcelProviderHelper<OsNames,OsNameRow>
          with member this.TableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new OsNames()) 



let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)


let ntrim (source:string) : string = 
    match source with
    | null -> ""
    | _ -> source.Trim()



// "outstation(uid:atom, adb_path:string, status:atom)."
let outstation3 (row:OsRow) : Predicate = 
    predicate "outstation" 
                [ quotedAtom row.Reference
                ; stringTerm row.``Common Name``
                ; quotedAtom row.AssetStatus
                ]


// "outstation(os_name:atom, uid:atom, common_name:string)."
let outstationName3 (row:OsNameRow) : Predicate option = 
    match ntrim row.``RTS Outstation Name`` with
    | null | "" -> None
    | rtsname -> 
        predicate "outstation_name" 
                        [ quotedAtom rtsname
                        ; quotedAtom row.Reference
                        ; stringTerm (ntrim row.``Common Name``)
                        ] |> Some


let main () = 
    let outPath = makeOutputPath "edm_outstations.pl"
    let osList = readOutstations () 
    let namesList = readOsNames ()             
    runFactWriter 160 outPath 
        <|  factWriter {
            do! tellComment "edm_outstations.pl"
            do! newline
            do! tellDirective (moduleDirective "edm_outstations" ["outstation/3" ; "outstation_name/3"])
            do! newline
            do! mapMz (tellPredicate << outstation3) osList
            do! newline
            do! mapMz (optTellPredicate << outstationName3) namesList
            return ()
        }

