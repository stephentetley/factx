module FactX.Utils

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