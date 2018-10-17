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



#load "..\src\FactX\Internal\PrettyPrint.fs"
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
    System.IO.Path.Combine(@"D:\coding\prolog\spt-misc\prolog\calibration\facts", filename) 


// ****************************************************************************
// RELAYS

type Relay13Table = 
    ExcelFile< @"G:\work\ADB-exports\Ultrasonics_relays_1_3.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >


type Relay13Row = Relay13Table.Row

let readRelay13Spreadsheet () : Relay13Row list = 
    let helper = 
        { new IExcelProviderHelper<Relay13Table,Relay13Row>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }         
    excelReadRowsAsList helper (new Relay13Table())


type Relay46Table = 
    ExcelFile< @"G:\work\ADB-exports\Ultrasonics_relays_4_6.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >


type Relay46Row = Relay46Table.Row

let readRelay46Spreadsheet () : Relay46Row list = 
    let helper = 
        { new IExcelProviderHelper<Relay46Table,Relay46Row>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
    excelReadRowsAsList helper (new Relay46Table())



let decodeRelay (uid:string) (number:int) (funName:string) 
            (ons:string) (offs:string) : Option<Clause> = 
    let activeRelay = 
        Clause.optionCons ( 
            signature = "active_relay(pli_code, relay_number, relay_function, on_setpoint, off_setpoint)."
            , body =    [ optPrologSymbol uid
                        ; Some (prologInt number)
                        ; optPrologSymbol funName
                        ; readPrologDecimal ons
                        ; readPrologDecimal offs] )

    let fixedRelay = 
        Clause.optionCons ( 
            signature = "fixed_relay(pli_code, relay_number, relay_function)."
            , body =    [ optPrologSymbol uid
                        ; Some (prologInt number)
                        ; optPrologSymbol funName ] )

    match activeRelay with
    | Some _ -> activeRelay
    | None -> fixedRelay
    



let processRelay13Row (row:Relay13Row) : option<Clause> list  = 
    let r1 = decodeRelay (row.Reference) 1 (row.``Relay 1 Function``) 
                            (row.``Relay 1 on Level (m)``) 
                            (row.``Relay 1 off Level (m)``)
    let r2 = decodeRelay (row.Reference) 2 (row.``Relay 2 Function``) 
                            (row.``Relay 2 on Level (m)``) 
                            (row.``Relay 2 off Level (m)``)    
    let r3 = decodeRelay (row.Reference) 3 (row.``Relay 3 Function``) 
                            (row.``Relay 3 on Level (m)``) (row.``Relay 3 off Level (m)``)    
    [ r1; r2; r3 ]


let processRelay46Row (row:Relay46Row) : option<Clause> list  = 
    let r1 = decodeRelay (row.Reference) 4 (row.``Relay 4 Function``) 
                            (row.``Relay 4 on Level (m)``) 
                            (row.``Relay 4 off Level (m)``)
    let r2 = decodeRelay (row.Reference) 5 (row.``Relay 5 Function``) 
                            (row.``Relay 5 on Level (m)``) 
                            (row.``Relay 5 off Level (m)``)    
    let r3 = decodeRelay (row.Reference) 6 (row.``Relay 6 Function``) 
                            (row.``Relay 6 on Level (m)``) (row.``Relay 6 off Level (m)``)    
    [ r1; r2; r3 ]


let genRelayFacts () : unit = 
    let outFile = outputFile "relays.pl"
    
    let relays13 : FactBase = 
        readRelay13Spreadsheet () 
            |> List.map processRelay13Row
            |> List.concat 
            |> FactBase.ofOptionList

    let relays46 : FactBase = 
        readRelay46Spreadsheet () 
            |> List.map processRelay46Row
            |> List.concat 
            |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module( name = "relays"
                  , comment = "relays.pl"
                  , dbs = [relays13; relays46] )

    pmodule.Save(lineWidth = 120, filePath = outFile)


// ****************************************************************************
// SENSORS

type UsMiscTable = 
    ExcelFile< @"G:\work\ADB-exports\Ultrasonics_misc_attributes.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type UsMiscRow = UsMiscTable.Row

let readUsMiscSpreadsheet () : UsMiscRow list = 
    let helper = 
        { new IExcelProviderHelper<UsMiscTable,UsMiscRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = 
                        match row.GetValue(0) with null -> true | _ -> false }
    excelReadRowsAsList helper (new UsMiscTable())
 
 
let extractDistFacts (rows:UsMiscRow list) : FactBase = 
    let makeDistClause (row:UsMiscRow) : option<Clause> = 
        Clause.optionCons ( signature = "sensor_measurements(pli_code, empty_distance, working_span)."
                          , body = [ optPrologSymbol      row.Reference
                                   ; readPrologDecimal    row.``Transducer face to bottom of well (m)``
                                   ; readPrologDecimal    row.``Working Span (m)`` 
                                   ])
    rows|> List.map makeDistClause |> FactBase.ofOptionList




let genSensorFacts () : unit = 
    let outFile = outputFile "sensors.pl"
    let rows = readUsMiscSpreadsheet () 

    let distFacts = extractDistFacts rows

    let pmodule : Module = 
        new Module( name = "sensors"
                  , comment = "sensors.pl"
                  , db = distFacts )

    pmodule.Save(lineWidth = 120, filePath = outFile)

// ****************************************************************************
// LEVEL MONITORS


let locale (commonName:string) : string = 
    let path : PathString = pathString "/" commonName
    if path.Length >= 4 then 
        path.Skip(2).SkipRight(1).Clone(":").Output()
    else 
        null

let extractMonitorLocation (rows:UsMiscRow list) : FactBase = 
    let makeLocationClause (row:UsMiscRow) : option<Clause>  = 
        Clause.optionCons ( signature = "monitor_location(site, subpath, intrument_code)."
                          , body = [ optPrologSymbol    (siteName   row.``Common Name``)
                                   ; optPrologSymbol    (locale     row.``Common Name``) 
                                   ; optPrologSymbol    row.Reference
                                   ]) 
    rows |> List.map makeLocationClause |> FactBase.ofOptionList


    
let extractMonitorModel (rows:UsMiscRow list) : FactBase = 
    let makeModelClause (row:UsMiscRow) : option<Clause>  = 
        Clause.optionCons ( signature = "monitor_model(pli_code, manufacturer, model)."
                          , body = [ optPrologSymbol    row.Reference
                                   ; optPrologString    row.Manufacturer
                                   ; optPrologString    row.Model ]) 
    rows |> List.map makeModelClause |> FactBase.ofOptionList

let genControllerFacts () = 
    let outFile = outputFile "level_monitors.pl"
    let rows = readUsMiscSpreadsheet () 

    let locations : FactBase = extractMonitorLocation rows
    let models : FactBase = extractMonitorModel rows

    let pmodule : Module = 
        new Module( name = "level_monitors"
                  , comment = "level_monitors.pl"
                  , dbs = [ locations; models ] )

    pmodule.Save(lineWidth = 120, filePath = outFile)

let main () : unit = 
    genRelayFacts ()
    genSensorFacts () 
    genControllerFacts ()


