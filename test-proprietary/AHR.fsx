// Copyright (c) Stephen Tetley 2019

#r "netstandard"
open System.IO


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
open FactX.Extra.Skeletons
open FactX.Extra.ExcelProviderHelper


// ********** DATA SETUP **********

type MappingTable = 
    ExcelFile< @"G:\work\Projects\asset_sync\AI2_FLOC_Asset_Hierarchy_Rules_V3_FRAGMENT.xlsx",
                SheetName = "Mapping!",
                ForceString = true >

type MappingRow = MappingTable.Row

let excelHelper : IExcelProviderHelper<MappingTable, MappingRow> = 
    { new IExcelProviderHelper<MappingTable, MappingRow>
        with 
            member this.TableRows table = table.Data 
            member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
let checkInput (input:string) : string = 
    match input with
    | null | "NULL" -> ""
    | _ -> input.Trim()


let codeMapping (row:MappingRow) : Predicate = 
    predicate "code_mapping" 
                [ quotedAtom <| checkInput row.InstAssetTypeCode
                ; quotedAtom <| checkInput row.PrcgAssetTypeDescription
                ; quotedAtom <| checkInput row.PrcAssetTypeDescription
                ; quotedAtom <| checkInput row.``L2 FLOC Code/Object Code``
                ; quotedAtom <| checkInput row.``L3 FLOC Code/Object Code``
                ; quotedAtom <| checkInput row.``L4 FLOC Code/Object Code``
                ]

let codeMappingSkeleton (table:MappingTable) : ModuleSkeleton = 
    let codePredicate = 
        { PredicateName = "code_mapping/6"
          Comment = "code_mapping(inst_type:atom, group:atom, process:atom, function_code:atom, group_code:atom, process_code:atom)."
          WriteFacts = excelProviderWriteFacts excelHelper (Some << codeMapping) table
        }
    { OutputPath = @"G:\work\Projects\asset_sync\output\code_mapping.pl"
      ModuleName = "code_mapping"
      PredicateSkeletons = [ codePredicate ]
    }

/// Allowed processes and process groups

let prcg (name:string) : Predicate = 
    predicate "allowed_process_group" 
                [ quotedAtom <| checkInput name
                ]

let prc (name:string) : Predicate = 
    predicate "allowed_process" 
                [ quotedAtom <| checkInput name
                ]



let processesSkeleton (table:MappingTable) : ModuleSkeleton = 
    let prcgSkeleton = 
        let elements : string list = 
            excelReadRowsAsList excelHelper table
                |> List.map (fun (row:MappingRow) -> row.PrcgAssetTypeDescription)
                |> List.sort
                |> List.distinct
        { PredicateName = "allowed_process_group/1"
          Comment = "allowed_process_group(name:atom)."
          WriteFacts = seqWriteFacts (Some << prcg) elements
        }

    let prcSkeleton = 
        let elements : string list = 
            excelReadRowsAsList excelHelper table
                |> List.map (fun (row:MappingRow) -> row.PrcAssetTypeDescription)
                |> List.sort
                |> List.distinct
        { PredicateName = "allowed_process/1"
          Comment = "allowed_process(name:atom)."
          WriteFacts = seqWriteFacts (Some << prc) elements
        }
    { OutputPath = @"G:\work\Projects\asset_sync\output\process_rules.pl"
      ModuleName = "process_rules"
      PredicateSkeletons = [ prcgSkeleton; prcSkeleton ]
    }

/// level2: code -> description

let descrLookupPredicate (predicateName:string) 
                         (code:string) 
                         (descr:string) : Predicate option = 
    match code, descr with
    | null, _ -> None
    | _, null -> None
    | _,_ -> 
        Some <| predicate predicateName
                    [ quotedAtom <| checkInput code
                    ; quotedAtom <| checkInput descr
                    ]





let level2DescrSkeleton (table:MappingTable) : PredicateSkeleton =
    let makePred (code:string, functionName:string) : Predicate option = 
        descrLookupPredicate "level2_function_description"  code functionName

    let getPair (row:MappingRow) : string * string = 
        (row.``L2 FLOC Code/Object Code``, row.``Function (L2 FLOC Description)``)
    let elements : (string * string) list = 
        excelReadRowsAsList excelHelper table
            |> List.map getPair
            |> List.sort
            |> List.distinctBy (fun (a,b) -> a+b)
    { PredicateName = "level2_function_description/2"
      Comment = "level2_function_description(code:atom, decription:atom)."
      WriteFacts = seqWriteFacts makePred elements
    }

/// level3: code -> description


let level3DescrSkeleton (table:MappingTable) : PredicateSkeleton =

    let makePred (code:string, procgName:string) : Predicate option = 
        descrLookupPredicate "level3_process_group_description" code procgName

    let getPair (row:MappingRow) : string * string = 
        (row.``L3 FLOC Code/Object Code``, row.``Process Group (L3 FLOC Description)``)
    let elements : (string * string) list = 
        excelReadRowsAsList excelHelper table
            |> List.map getPair
            |> List.sort
            |> List.distinctBy (fun (a,b) -> a+b)
    { PredicateName = "level3_process_group_description/2"
      Comment = "level3_process_group_description(code:atom, decription:atom)."
      WriteFacts = seqWriteFacts makePred elements
    }


/// level4: code -> description



let level4DescrSkeleton (table:MappingTable) : PredicateSkeleton =
    let makePred (code:string, procgName:string) : Predicate option = 
        descrLookupPredicate "level4_process_description" code procgName

    let getPair (row:MappingRow) : string * string = 
        (row.``L4 FLOC Code/Object Code``, row.``Process (L4 FLOC Description)``)
    let elements : (string * string) list = 
        excelReadRowsAsList excelHelper table
            |> List.map getPair
            |> List.sort
            |> List.distinctBy (fun (a,b) -> a+b)
    { PredicateName = "level4_process_description/2"
      Comment = "level4_process_description(code:atom, decription:atom)."
      WriteFacts = seqWriteFacts makePred elements
    }

let descriptionLookupsSkeleton (table:MappingTable) = 
    { OutputPath = @"G:\work\Projects\asset_sync\output\description_lookups.pl"
      ModuleName = "description_lookups"
      PredicateSkeletons = 
        [ level2DescrSkeleton table 
        ; level3DescrSkeleton table
        ; level4DescrSkeleton table
        ]
    }
let main () = 
    let source = new MappingTable()
    generateModule (codeMappingSkeleton source)
    generateModule (processesSkeleton source)
    generateModule (descriptionLookupsSkeleton source)
