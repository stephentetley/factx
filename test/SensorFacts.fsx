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



#load "..\src\FactX\Internal\FormatCombinators.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.Extra.ExcelProviderHelper



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


let genSensorFacts () : unit = 
    let outFile = outputFile "us_sensor_facts.pl"
    

    let makeDistClause (row:UsMiscRow) : option<Clause> = 
        Clause.optionCons ( signature = "us_sensor_distances(pli_code, empty_distance, working_span)."
                          , body = [ optPrologSymbol      row.Reference
                                   ; readPrologDecimal    row.``Transducer face to bottom of well (m)``
                                   ; readPrologDecimal    row.``Working Span (m)`` ])

             
    let distFacts : FactBase = 
        readUsMiscSpreadsheet () 
            |> List.map makeDistClause 
            |> FactBase.ofOptionList


    let makeModelClause (row:UsMiscRow) : option<Clause>  = 
        Clause.optionCons ( signature = "us_model(pli_code, manufacturer, model)."
                          , body = [ optPrologSymbol    row.Reference
                                   ; optPrologString row.Manufacturer
                                   ; optPrologString row.Model ]) 


    let modelFacts : FactBase = 
        readUsMiscSpreadsheet () 
            |> List.map makeModelClause 
            |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module( name = "us_sensor_facts"
                  , dbs = [ modelFacts; distFacts] )

    pmodule.Save(outFile)


type Relay13Table = 
    ExcelFile< @"G:\work\AI2-exports\Ultrasonics_relays_1_3.xlsx",
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
    ExcelFile< @"G:\work\AI2-exports\Ultrasonics_relays_4_6.xlsx",
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
            signature = "us_active_relay(pli_code, relay_number, relay_function, on_setpoint, off_setpoint)."
            , body =    [ optPrologSymbol uid
                        ; Some (prologInt number)
                        ; optPrologString funName
                        ; readPrologDecimal ons
                        ; readPrologDecimal offs] )

    let fixedRelay = 
        Clause.optionCons ( 
            signature = "us_fixed_relay(pli_code, relay_number, relay_function)."
            , body =    [ optPrologSymbol uid
                        ; Some (prologInt number)
                        ; optPrologString funName ] )

    match activeRelay with
    | Some _ -> activeRelay
    | None -> fixedRelay
    




// TODO this would likely be simpler if we could add to FactSets
let getRelays13 (row:Relay13Row) : option<Clause> list  = 
    let r1 = decodeRelay (row.Reference) 1 (row.``Relay 1 Function``) 
                            (row.``Relay 1 on Level (m)``) 
                            (row.``Relay 1 off Level (m)``)
    let r2 = decodeRelay (row.Reference) 2 (row.``Relay 2 Function``) 
                            (row.``Relay 2 on Level (m)``) 
                            (row.``Relay 2 off Level (m)``)    
    let r3 = decodeRelay (row.Reference) 3 (row.``Relay 3 Function``) 
                            (row.``Relay 3 on Level (m)``) (row.``Relay 3 off Level (m)``)    
    [ r1; r2; r3 ]


let getRelays46 (row:Relay46Row) : option<Clause> list  = 
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
    let outFile = outputFile "us_relay_facts.pl"
    
    let relays13 : FactBase = 
        readRelay13Spreadsheet () 
            |> List.map getRelays13
            |> List.concat 
            |> FactBase.ofOptionList

    let relays46 : FactBase = 
        readRelay46Spreadsheet () 
            |> List.map getRelays46
            |> List.concat 
            |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module( name = "us_relay_facts"
                  , dbs = [relays13; relays46] )

    pmodule.Save(outFile)

let main () : unit = 
    genRelayFacts ()
    genSensorFacts () 

