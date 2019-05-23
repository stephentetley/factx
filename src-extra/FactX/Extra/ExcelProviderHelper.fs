// Copyright (c) Stephen Tetley 2018, 2019
// License: BSD 3 Clause

namespace FactX.Extra.ExcelProviderHelper


[<AutoOpen>]
module ExcelProviderHelper = 

    open System.IO

    open FactX
    open FactX.FactWriter
    open FactX.Extra.Skeletons

    /// F# design guidelines say favour object-interfaces rather than records of functions...
    type IExcelProviderHelper<'table,'row> = 
        abstract member TableRows : 'table -> seq<'row>
        abstract member IsBlankRow: 'row -> bool

    let excelReadRows (helper:IExcelProviderHelper<'table,'row>) (table:'table) : seq<'row> = 
        let allrows = helper.TableRows table
        allrows |> Seq.filter (not << helper.IsBlankRow)


    let excelReadRowsAsList (helper:IExcelProviderHelper<'table,'row>) (table:'table) : 'row list = 
        excelReadRows helper table |> Seq.toList

    
    let excelProviderWriteFacts (helper:IExcelProviderHelper<'table,'row>) 
                                (rowFact: 'row -> Predicate option) 
                                (table:'table) : FactWriter<unit> =
        let rows = excelReadRowsAsList helper table 
        mapMz (optTellPredicate << rowFact) rows

