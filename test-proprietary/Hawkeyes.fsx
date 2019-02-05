// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause


#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190205\lib\netstandard2.0"
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
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src\FactX\Extra\String.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.String

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\events2\point-blue\prolog\facts", filename) 

type CsoTable = 
    ExcelFile< FileName = @"G:\work\Projects\events2\point-blue\hawkeyes.xlsx",
               SheetName = "Main_Page!",
               ForceString = true >

type CsoRow = CsoTable.Row


let readCSOSpreadsheet () : CsoRow list = 
    let helper = 
        { new IExcelProviderHelper<CsoTable, CsoRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    new CsoTable() |> excelReadRowsAsList helper

let hawkeyeFacts () = 
    let outFile = outputFileName "hawkeyes.pl"

    let rows = readCSOSpreadsheet ()
    
    let hawkeyeClause (row:CsoRow) : option<Clause> = 
        Clause.optionCons( signature = "hawkeye_remaining(od_name, os_name, od_comment)."
                         , body = [ optPrologSymbol     row.``OD name``
                                  ; optPrologSymbol     row.``OS name``
                                  ; optPrologString     (row.``OD comment``.Trim()) ] )

    let hawkeyes : FactBase  = 
        rows |> List.map hawkeyeClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("hawkeyes", "hawkeyes.pl", hawkeyes)

    pmodule.Save(outFile)

let fileName (path:string) : string = 
    System.IO.FileInfo(path).Name

let siteName (path:string) : option<string> = 
    let temp = leftOfAny [" revisit"; " commissioning"] (fileName path)
    match temp.Replace("_", "/") with
    | "" -> None
    | str -> Some str


let siteFacts () : unit = 
    let outFile = outputFileName "sites.pl"

    let dir = @"G:\work\Projects\events2\point-blue\pb-commissioning-forms"
    let filePaths :string [] = System.IO.Directory.GetFiles(dir)
    
    let siteClause (filePath:string) : option<Clause> = 
        Clause.optionCons( signature = "site(site_name)."
                         , body = [ Option.bind optPrologString (siteName filePath)] )


    let sites : FactBase  = 
        filePaths |> Array.map siteClause |> FactBase.ofOptionArray

    let pmodule : Module = 
        new Module ("sites", "sites.pl", sites)

    pmodule.Save(outFile)
