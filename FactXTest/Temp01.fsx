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
#load "..\FactX\FactX\Internal\PrologSyntax.fs"
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
#load "..\FactX\FactX\Extra\String.fs"
open FactX.Internal.FormatCombinators
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.String

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\events2\point-blue\prolog\facts", filename) 

type CsoTable = 
    ExcelFile< FileName = @"G:\work\Projects\events2\point-blue\csos.xlsx",
               SheetName = "Main_Page!",
               ForceString = true >

type CsoRow = CsoTable.Row

/// ExcelProvider can read "data" files not just the file the type was 
/// instantiated with.
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
