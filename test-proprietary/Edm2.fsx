// Copyright (c) Stephen Tetley 2019

#r "netstandard"
open System.IO

open System.Text.RegularExpressions

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
#load "..\src-extra\FactX\Extra\Skeletons.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.FactWriter
open FactX.Extra.Skeletons
open FactX.Extra.ExcelProviderHelper


// ********** DATA SETUP **********


type EdmsDocTable = 
    ExcelFile< @"G:\work\Projects\events2\facts\EDMS-export-20190624.xlsx",
                SheetName = "Sheet1!",
                ForceString = true >

type EdmsDocRow = EdmsDocTable.Row

let docTableHelper : IExcelProviderHelper<EdmsDocTable, EdmsDocRow> = 
    { new IExcelProviderHelper<EdmsDocTable, EdmsDocRow>
        with 
            member this.TableRows table = table.Data 
            member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }



let tryReplace (name :string) (pattern : string) : string option = 
    let swap1 (str:string) = "_" + str.Substring(1)
    let rmatch = Regex.Match(name, pattern)
    if rmatch.Success then
        let suffix = swap1 rmatch.Value
        Regex.Replace(name, rmatch.Value, suffix) |> Some
    else
        None

let addUnderscore (name : string) : string = 
    match tryReplace name " NO [0-9] [A-Z]{3}$" with
    | Some ans -> ans
    | None -> 
        match tryReplace name " [0-9] [A-Z]{3}$" with
        | Some ans -> ans
        | None -> 
            match tryReplace name " [A-Z]{3}$" with
            | Some ans -> ans
            | None -> name

            

let edmsExtractName (title : string) : string option = 
    let pattern = "^(?<name>.*?) T0975"
    let rmatch = Regex.Match(input = title, pattern = pattern)
    if rmatch.Success then 
        rmatch.Groups.Item("name").Value |> addUnderscore |> Some
    else
        None

let documentFact (siteName : string) : Predicate = 
    predicate "document" [ stringTerm siteName ]

let docNames () : string list = 
    let table = new EdmsDocTable ()
    excelReadRowsAsList docTableHelper table
        |> List.choose (fun (row:EdmsDocRow) -> row.Title |> edmsExtractName)

let main () = 
    let outPath = @"G:\work\Projects\events2\facts\edms.lp"
    let docs = docNames ()
    runFactWriter 160 outPath 
        <| mapMz (tellPredicate << documentFact) docs 
