// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.ExcelProviderHelper


[<AutoOpen>]
module ExcelProviderHelper = 

    open System.IO

    open FactX
    open FactX.FactWriter


    /// F# design guidelines say favour object-interfaces rather than records of functions...
    type IExcelProviderHelper<'table,'row> = 
        abstract member ReadTableRows : 'table -> seq<'row>
        abstract member IsBlankRow: 'row -> bool

    let excelReadRows (helper:IExcelProviderHelper<'table,'row>) (table:'table) : seq<'row> = 
        let allrows = helper.ReadTableRows table
        allrows |> Seq.filter (not << helper.IsBlankRow)


    let excelReadRowsAsList (helper:IExcelProviderHelper<'table,'row>) (table:'table) : 'row list = 
        excelReadRows helper table |> Seq.toList

    
    /// Skeleton

    type ExcelProvider1to1Skeleton<'table,'row> = 
        { OutputPath: string
          ModuleName: string
          Exports: string list
          PredicateComment: string
          ExcelReader: IExcelProviderHelper<'table,'row>
          RowFact: 'row -> Predicate option
        }

    let excelTableToFacts1to1 (skeleton:ExcelProvider1to1Skeleton<'table, 'row>) (table: 'table): unit =
        let justfile = FileInfo(skeleton.OutputPath).Name
        let rows = excelReadRowsAsList skeleton.ExcelReader table 
        runFactWriter 160 skeleton.OutputPath 
            <|  factWriter {
                do! tellComment justfile
                do! newline
                do! tellDirective (moduleDirective skeleton.ModuleName skeleton.Exports)
                do! newline
                do! tellComment skeleton.PredicateComment
                do! mapMz (optTellPredicate << skeleton.RowFact) rows
                do! newline
                return ()
            }
