// Copyright (c) Stephen Tetley 2019

#r "netstandard"
open System.IO

open System.Text.RegularExpressions


#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190721\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\rewriteyourstrings\1.0.0-alpha-20190628\lib\netstandard2.0"
#r "RewriteYourStrings"
open RewriteYourStrings.RewriteMonad
open RewriteYourStrings.Query
open RewriteYourStrings.Transform

#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Syntax.fs"
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src\FactX\Skeletons.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.FactWriter
open FactX.Skeletons
open FactX.Extra.ExcelProviderHelper



// ********** Extras for rewrite-your-strings **********

let execRewrite (action : Rewrite)
                (input : string) : StringRewriter<string> = 
    rewrite { 
        let! start = getInput ()
        do! setInput input
        let! _ = action
        let! ans = getInput ()
        do! setInput start
        return ans
    }

let runOptional (ma : StringRewriter<'a>) 
                (input : string) : string option = 
    match runRewrite ma input with
    | Error _ -> None
    | Ok ans -> Some ans

let primitiveRewrite (operation: RegexOptions -> string -> string option) : Rewrite =
    rewrite { 
        let! source = getInput ()
        let! regexOpts = askOptions ()
        match operation regexOpts source with
        | None -> return! rewriteError "primitiveRewrite"
        | Some ans -> return! setInput ans
    }


let namedRegexMatch (pattern : string) (name : string) : Rewrite = 
    primitiveRewrite <| fun opts input -> 
        let rmatch = Regex.Match(input = input, pattern = pattern)
        if rmatch.Success then 
            rmatch.Groups.Item(name).Value |> Some
        else
            None


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





let addUnderscore : Rewrite = 
    let rewriteRhs (pattern : string) = 
        rewrite { 
            let! rhs1 = matchValue pattern
            let! rhs2 = execRewrite (replacePrefix "_") rhs1
            return! modify (fun str -> str.Replace(oldValue = rhs1, newValue = rhs2))
        }
    choice  [ rewriteRhs " NO [0-9] [A-Z]{3}$" 
            ; rewriteRhs " [0-9] [A-Z]{3}$"
            ; rewriteRhs " [A-Z]{3}$" 
            ]



    
let edmsExtractName (title : string) : string option = 
    let extractName  : Rewrite = 
        rewrite { 
            do! namedRegexMatch "^(?<name>.*?) T0975" "name" 
            do! addUnderscore
            return ()
        }
    runOptional extractName title


let documentFact (siteName : string) : Predicate = 
    predicate "edms_document" [ stringTerm siteName ]

let docNames () : string list = 
    let table = new EdmsDocTable ()
    excelReadRowsAsList docTableHelper table
        |> List.choose (fun (row:EdmsDocRow) -> row.Title |> edmsExtractName)

let main () = 
    let outPath = @"G:\work\Projects\events2\facts\edms.lp"
    let docs = docNames ()
    runFactWriter 160 outPath 
        <| mapMz (tellPredicate << documentFact) docs 



