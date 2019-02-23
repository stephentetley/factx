// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"


#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190222\lib\netstandard2.0"
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
    System.IO.Path.Combine(@"G:\work\Projects\uqpb\prolog\facts", filename) 


type LmpTable = 
    ExcelFile< FileName = @"G:\work\Projects\uqpb\ADB-LMP-exports.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type LmpRow = LmpTable.Row


let readLmpSpreadsheet () : LmpRow list = 
    let helper = 
        { new IExcelProviderHelper<LmpTable, LmpRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
    new LmpTable() |> excelReadRowsAsList helper


let lmpFacts () = 
    let outFile = outputFileName "adb_lmps.pl"
   
    let lmpClause (row:LmpRow) : option<Clause> = 
        Clause.optionCons( signature = "level_monitor_point(uid, lmp_name, stc25_ref)."
                         , body = [ optPrologSymbol     row.Reference
                                  ; optPrologSymbol     row.``Common Name``
                                  ; optPrologSymbol     row.``STC25 Ref`` ] )

    let facts : FactBase  = 
        readLmpSpreadsheet () |> List.map lmpClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("adb_lmps", "adb_lmps.pl", facts)

    pmodule.Save(outFile)