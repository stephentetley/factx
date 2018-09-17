// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

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
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
#load "..\FactX\FactX\Extra\ValueReader.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.ValueReader


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\assets\facts", filename) 


type UsMiscTable = 
    ExcelFile< @"G:\work\AI2-exports\Ultrasonics_misc_attributes.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >


type UsMiscRow = UsMiscTable.Row



let readUsMiscSpreadsheet () : UsMiscRow list = 
    let helper = 
        { new IExcelProviderHelper<UsMiscTable,UsMiscRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new UsMiscTable())


let genSensorDistances () : unit = 
    let outFile = outputFile "sensor_distances.pl"
    
    let distHelper : IFactHelper<UsMiscRow> = 
        { new IFactHelper<UsMiscRow> with
            member this.Signature = "sensor_distances(pli_code, empty_distance, working_span)."
            member this.ClauseBody (row:UsMiscRow) = 
                runValueReader <| valueReader { 
                    let! uid        = readSymbol row.Reference
                    let! emptyDist  = readDecimal row.``Transducer face to bottom of well (m)``
                    let! span       = readDecimal row.``Working Span (m)``
                    return [uid; emptyDist; span]
                    }
        }
              

    let distFacts : FactSet = readUsMiscSpreadsheet () |> makeFactSet distHelper

    let pmodule : Module = 
        new Module("sensor_distances", "sensor_distances.pl", distFacts)

    pmodule.Save(outFile)