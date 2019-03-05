// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel



#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
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
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new InstTable())

let instClause (row:InstRow) : option<Clause> = 
    Clause.optionCons( signature = "installation(site_ref, site_name, inst_ref, inst_name, asset_type, status, daz, address1, address2, address3, address4, postcode)."
                     , body = [ optPrologSymbol row.SiteReference
                              ; optPrologSymbol row.SiteCommonName
                              ; optPrologSymbol row.InstReference 
                              ; optPrologSymbol row.InstCommonName
                              ; optPrologSymbol row.AssetType
                              ; optPrologSymbol row.AssetStatus
                              ; optPrologSymbol row.``DA Zone``
                              ; optPrologSymbol row.``Postal Address 1``
                              ; optPrologSymbol row.``Postal Address 2``
                              ; optPrologSymbol row.``Postal Address 3``
                              ; optPrologSymbol row.``Postal Address 4``
                              ; optPrologSymbol row.``Post Code``
                              ] )


let genInstFacts (rows:InstRow list) : unit = 
    let outFile = outputFileName "installation_facts.pl"

    let insts : FactBase     = 
        rows |> List.map instClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("installation_facts", "installation_facts.pl", [ insts ])

    pmodule.Save(outFile)

let main () = 
    readInstSpreadsheet () |> genInstFacts

