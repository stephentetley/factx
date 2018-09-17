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
open System.Security.AccessControl


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
    
    let distHelper : IFactHelper<UsMiscRow> = 
        { new IFactHelper<UsMiscRow> with
            member this.Signature = "us_sensor_distances(pli_code, empty_distance, working_span)."
            member this.ClauseBody (row:UsMiscRow) = 
                runValueReader <| valueReader { 
                    let! uid        = readSymbol row.Reference
                    let! emptyDist  = readDecimal row.``Transducer face to bottom of well (m)``
                    let! span       = readDecimal row.``Working Span (m)``
                    return [uid; emptyDist; span]
                    }
        }
             
    let distFacts : FactSet = readUsMiscSpreadsheet () |> makeFactSet distHelper
    
    let modelHelper : IFactHelper<UsMiscRow> = 
        { new IFactHelper<UsMiscRow> with
            member this.Signature = "us_model(pli_code, manufacturer, model)."
            member this.ClauseBody (row:UsMiscRow) = 
                runValueReader <| valueReader { 
                    let! uid        = readSymbol row.Reference
                    let! emptyDist  = readString row.Manufacturer
                    let! span       = readString row.Model
                    return [uid; emptyDist; span]
                    }
        }

    let modelFacts : FactSet = readUsMiscSpreadsheet () |> makeFactSet modelHelper

    let pmodule : Module = 
        new Module( name = "us_sensor_facts"
                  , db = [modelFacts; distFacts] )

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

type FixedRelay = 
    { Uid: string
      Number: int
      Function: string }

type ActiveRelay = 
    { Uid: string
      Number: int
      Function: string
      OnSetpoint: decimal
      OffSetpoint: decimal }

/// Use ValueReader
let decodeRelay (uid:string) (number:int) (funName:string) 
            (ons:string) (offs:string) : Option<Choice<FixedRelay,ActiveRelay>> = 
    let parseActive = 
        valueReader {
            let! uid1    = readStringRaw uid
            let! name   = readStringRaw funName
            let! on1     = readDecimalRaw ons
            let! off1    = readDecimalRaw offs
            return { Uid = uid1
                     Number = number
                     Function = name
                     OnSetpoint = on1
                     OffSetpoint = off1 }
        }
    let parseFixed = 
        valueReader {
            let! uid1    = readStringRaw uid 
            let! name    = readStringRaw funName  
            return { Uid = uid1
                     Number = number
                     Function = name }
        }
    let parser = (parseActive |>> Choice2Of2) <||> (parseFixed |>> Choice1Of2)
    runValueReader parser

let parseFixed1 (uid:string) (number:int) (funName:string) : ValueReader<FixedRelay> = 
    valueReader {
        let! uid1    = readStringRaw uid    
        return { Uid = uid1
                 Number = number
                 Function = funName }
    }  


let parseSilly (uid:string) (number:int) (funName:string) : ValueReader<string> = 
    valueReader {
        let! uid1    = readStringRaw uid    
        return (uid1 + "!!!")
    }  

let temp () = 
    let xs = [Choice1Of2 1; Choice2Of2 "name"]
    List.choose (fun x -> match x with | Choice1Of2 y -> Some y | _ -> None) xs

let temp2 () = 
    runValueReader <| valueReader { 
        return ("Raw" + "!!!") 
        }

// TODO this would be much easier if we could add to FactSets
let getRelays13 (rows:Relay13Row list) : FixedRelay list * ActiveRelay list = 
    let cons (opt:Option<'a>) (ac:'a list) : 'a list = 
        match opt with
        | None -> ac
        | Some a -> a::ac

    let rec work ac xs =
        match xs with
        | [] -> List.rev ac
        | (z : Relay13Row) :: zs -> 
            let r1 = decodeRelay (z.Reference) 1 (z.``Relay 1 Function``) 
                                 (z.``Relay 1 on Level (m)``) (z.``Relay 1 off Level (m)``)
            let r2 = decodeRelay (z.Reference) 2 (z.``Relay 2 Function``) 
                                 (z.``Relay 2 on Level (m)``) (z.``Relay 2 off Level (m)``)    
            let r3 = decodeRelay (z.Reference) 3 (z.``Relay 3 Function``) 
                                 (z.``Relay 3 on Level (m)``) (z.``Relay 3 off Level (m)``)    
            work (cons r3 (cons r1 ac)) zs

    let answers = work [] rows
    let fixeds = List.choose (fun x -> match x with | Choice1Of2 y -> Some y | _ -> None) answers
    let actives = List.choose (fun x -> match x with | Choice2Of2 y -> Some y | _ -> None) answers
    fixeds, actives

let genRelayFacts () : unit = 
    let outFile = outputFile "us_relay_facts.pl"
    
    let fixedRelayHelper : IFactHelper<FixedRelay> = 
        { new IFactHelper<FixedRelay> with
            member this.Signature = "us_fixed_relay(pli_code, relay_number, relay_function)."
            member this.ClauseBody (relay:FixedRelay) = 
                Some <| [ PQuotedAtom relay.Uid
                        ; PInt relay.Number
                        ; PString relay.Function ]
                    
        }

    let activeRelayHelper : IFactHelper<ActiveRelay> = 
        { new IFactHelper<ActiveRelay> with
            member this.Signature = "us_active_relay(pli_code, manufacturer, model, on_setpoint, off_setpoint)."
            member this.ClauseBody (relay:ActiveRelay) = 
                Some <| [ PQuotedAtom relay.Uid
                        ; PInt relay.Number
                        ; PString relay.Function 
                        ; PDecimal relay.OnSetpoint
                        ; PDecimal relay.OffSetpoint ]
        }
     
    let fixeds1, actives1 = readRelay13Spreadsheet () |> getRelays13

    let fixedFacts : FactSet = fixeds1 |> makeFactSet fixedRelayHelper
    let activeFacts : FactSet = actives1 |> makeFactSet activeRelayHelper

    let pmodule : Module = 
        new Module( name = "us_relay_facts"
                  , db = [fixedFacts; activeFacts] )

    pmodule.Save(outFile)
