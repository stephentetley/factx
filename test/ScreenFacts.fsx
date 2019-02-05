// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.1\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src\FactX\Extra\PathString.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.PathString

#load "Proprietary.fs"
open Proprietary

let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\spt-misc\prolog\screens\facts", filename)
    


// ****************************************************************************
// SCREENS

type ScreenTable = 
    ExcelFile< @"G:\work\Projects\events2\screens\ADB-equip=SCREENS-oct2018.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >


type ScreenRow = ScreenTable.Row


// Note - Bad input data - trim fields as we need them.
let readScreensTable () : ScreenRow list = 
    let helper = 
        { new IExcelProviderHelper<ScreenTable,ScreenRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }         
    excelReadRowsAsList helper (new ScreenTable())

 
let extractScreenFacts (rows:ScreenRow list) : FactBase = 
    let makeDistClause (row:ScreenRow) : option<Clause> = 
        Clause.optionCons ( signature = "screen(pli_code, site_name, path, manufacturer, model, screen_type, auto_or_manual, bypass_channel)."
                          , body = [ optPrologSymbol      row.Reference
                                   ; optPrologSymbol    <| siteName row.``Common Name``
                                   ; optPrologSymbol    <| equipmentPath row.``Common Name``
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.Manufacturer
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.Model
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.``Screen Type``
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.``Automatic or Manual``
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.``Bypass Channel``
                                   ])
    rows|> List.map makeDistClause |> FactBase.ofOptionList

 
let extractScreenMeasurements (rows:ScreenRow list) : FactBase = 
    let makeDistClause (row:ScreenRow) : option<Clause> = 
        Clause.optionCons ( signature = "screen_measurements(pli_code, screen_diameter, screen_length, screen_width, aperture_size, flow, flow_units)."
                          , body = [ optPrologSymbol      row.Reference
                                   ; Some << valueOrUnknown << readPrologDecimal <| row.``Screen Diameter mm``
                                   ; Some << valueOrUnknown << readPrologDecimal <| row.``Screen Length m``
                                   ; Some << valueOrUnknown << readPrologDecimal <| row.``Screen Width m``
                                   ; Some << valueOrUnknown << readPrologDecimal <| row.``Screen Aperture Size mm``
                                   ; Some << valueOrUnknown << readPrologDecimal <| row.Flow
                                   ; Some << valueOrUnknown << optPrologSymbol <| row.``Flow Units``
                                   ])
    rows|> List.map makeDistClause |> FactBase.ofOptionList

let genScreenFacts () : unit = 
    let outFile = outputFile "screens.pl"
    let rows = readScreensTable ()
    
    let factBase = rows |> extractScreenFacts
    let measurements = rows |> extractScreenMeasurements

    let pmodule : Module = 
        new Module( name = "screens"
                  , comment = "screens.pl"
                  , dbs = [factBase; measurements] )

    pmodule.Save(outFile)

let main () = 
    genScreenFacts ()

