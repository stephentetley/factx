// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

#r "netstandard"


#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190712\lib\netstandard2.0"
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


let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__, @"..\data", filename)



type InstTable = 
    ExcelFile< FileName = @"G:\work\ADB-exports\SitesInsts.xlsx",
               SheetName = "Data_101018!",
               ForceString = true >

type InstRow = InstTable.Row


let readInstSpreadsheet () : InstRow list = 
    let helper = 
        { new IExcelProviderHelper<InstTable,InstRow>
          with member this.TableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new InstTable())

let signature : string = "installation(site_ref, site_name, inst_ref, inst_name, asset_type, status, daz, address1, address2, address3, address4, postcode)."

let installation12 (row:InstRow) : Predicate = 
    predicate "installation" 
                [ quotedAtom row.SiteReference
                ; quotedAtom row.SiteCommonName
                ; quotedAtom row.InstReference 
                ; quotedAtom row.InstCommonName
                ; quotedAtom row.AssetType
                ; quotedAtom  row.AssetStatus
                ; quotedAtom  row.``DA Zone``
                ; stringTerm row.``Postal Address 1``
                ; stringTerm row.``Postal Address 2``
                ; stringTerm row.``Postal Address 3``
                ; stringTerm row.``Postal Address 4``
                ; stringTerm row.``Post Code``
                ] 


let genInstFacts (rows:InstRow list) : unit = 
    let outFile = outputFileName "installation_facts.pl"
    runFactWriter 160 outFile 
        <|  factWriter {
            do! tellComment "installation_facts.pl"
            do! newline
            do! tellDirective (moduleDirective "installation_facts" ["installation/12"])
            do! newline
            do! tellComment signature
            do! mapMz (tellPredicate << installation12) rows
            do! newline
            return ()
        }


let main () = 
    readInstSpreadsheet () |> genInstFacts

