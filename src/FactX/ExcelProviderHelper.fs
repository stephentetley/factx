// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

module FactX.ExcelProviderHelper



/// F# design guidelines say favour object-interfaces rather than records of functions...
type IExcelProviderHelper<'table,'row> = 
    abstract member GetTableRows : 'table -> seq<'row>
    abstract member IsBlankRow: 'row -> bool

let excelGetRows (helper:IExcelProviderHelper<'table,'row>) (table:'table) : seq<'row> = 
    let allrows = helper.GetTableRows table
    allrows |> Seq.filter (not << helper.IsBlankRow)


let excelGetRowsAsList (helper:IExcelProviderHelper<'table,'row>) (table:'table) : 'row list = 
    excelGetRows helper table |> Seq.toList


// *************************************
// Old ...


// The Excel Type Provider seems to read a trailing null row.
// This dictionary and procedures provide a skeleton to get round this.

type GetExcelRowsDict<'table, 'row> = 
    { GetRows : 'table -> seq<'row>
      NotNullProc : 'row -> bool }

let excelTableGetRowsSeq (dict:GetExcelRowsDict<'table,'row>) (table:'table) : seq<'row> = 
    let allrows = dict.GetRows table
    allrows |> Seq.filter dict.NotNullProc

let excelTableGetRows (dict:GetExcelRowsDict<'table,'row>) (table:'table) : 'row list = 
    let allrows = dict.GetRows table
    allrows |> Seq.filter dict.NotNullProc |> Seq.toList