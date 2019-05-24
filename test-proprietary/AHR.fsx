// Copyright (c) Stephen Tetley 2019

#r "netstandard"
open System.IO

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
    ExcelFile< @"G:\work\Projects\asset_sync\Mapping_Rules.xlsx",
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

let descrMapping (row:MappingRow) : Predicate = 
    predicate "descr_mapping" 
                [ quotedAtom <| checkInput row.InstAssetTypeCode
                ; quotedAtom <| checkInput row.PrcgAssetTypeDescription
                ; quotedAtom <| checkInput row.PrcAssetTypeDescription
                ; quotedAtom <| checkInput row.``Function (L2 FLOC Description)``
                ; quotedAtom <| checkInput row.``Process Group (L3 FLOC Description)``
                ; quotedAtom <| checkInput row.``Process (L4 FLOC Description)``
                ]

let descrMappingSkeleton (table:MappingTable) : ModuleSkeleton = 
    let descrPredicate = 
        { PredicateName = "descr_mapping/6"
          Comment = "descr_mapping(inst_type:atom, group:atom, process:atom, function_descr:atom, group_descr:atom, process_descr:atom)."
          WriteFacts = excelProviderWriteFacts excelHelper (Some << descrMapping) table
        }
    { OutputPath = @"G:\work\Projects\asset_sync\output\descr_mapping.pl"
      ModuleName = "descr_mapping"
      PredicateSkeletons = [ descrPredicate ]
    }

let main () = 
    let source = new MappingTable()
    generateModule (codeMappingSkeleton source)
    generateModule (descrMappingSkeleton source)

let prcg (name:string) : Predicate = 
    predicate "process_group" 
                [ quotedAtom <| checkInput name
                ]

let prc (name:string) : Predicate = 
    predicate "process" 
                [ quotedAtom <| checkInput name
                ]



let processesSkeleton (table:MappingTable) : ModuleSkeleton = 
    let prcgPredicate = 
        let elements : string list = 
            excelReadRowsAsList excelHelper table
                |> List.map (fun (row:MappingRow) -> row.PrcgAssetTypeDescription)
                |> List.sort
                |> List.distinct
        { PredicateName = "process_group/1"
          Comment = "process_group(process_group_name:atom)."
          WriteFacts = seqWriteFacts (Some << prcg) elements
        }

    let prcPredicate = 
        let elements : string list = 
            excelReadRowsAsList excelHelper table
                |> List.map (fun (row:MappingRow) -> row.PrcAssetTypeDescription)
                |> List.sort
                |> List.distinct
        { PredicateName = "process/1"
          Comment = "process(process_name:atom)."
          WriteFacts = seqWriteFacts (Some << prc) elements
        }
    { OutputPath = @"G:\work\Projects\asset_sync\output\processes.pl"
      ModuleName = "processes"
      PredicateSkeletons = [ prcgPredicate; prcPredicate ]
    }


let genProcesses () = 
    let source = new MappingTable()
    generateModule (processesSkeleton source)
