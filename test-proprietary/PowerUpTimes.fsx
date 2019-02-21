// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"
#r "System.Xml.Linq.dll"


#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190207\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\FSharp.Data\3.0.0\lib\netstandard2.0"
#r @"FSharp.Data.dll"
open FSharp.Data


open System
open System.Text.RegularExpressions

#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src-extra\FactX\Extra\PathString.fs"
#load "..\src-extra\FactX\Extra\String.fs"
#load "..\src-extra\FactX\Extra\LabelledTree.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.PathString
open FactX.Extra.String
open FactX.Extra.LabelledTree


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"G:\work\Projects\uqpb", filename)



type PowerUpTimesTable = 
    CsvProvider< @"G:\work\Projects\uqpb\rts-test-group-outstations.trim.csv",
                 HasHeaders = true,
                 IgnoreErrors = true >

type PowerUpRow = PowerUpTimesTable.Row

let readPowerUpRow () : PowerUpRow list = 
    (new PowerUpTimesTable()).Rows |> Seq.toList




    
let test01 () = 
    readPowerUpRow () 
        |> List.iter (fun row -> printfn "%A" (row.``Last power up``))


let decodeMonth (source:string) : int = 
    match source.ToUpper() with
    | "JAN" -> 1
    | "FEB" -> 2
    | "MAR" -> 3
    | "APR" -> 4
    | "MAY" -> 5
    | "JUN" -> 6
    | "JUL" -> 7
    | "AUG" -> 8
    | "SEP" -> 9
    | "OCT" -> 10
    | "NOV" -> 11
    | "DEC" -> 12
    | _ -> 0



/// Format is either:
/// "??:?? ??-???-??"
/// "10:20 07-May-15"
let parseDate (source:string) : option<DateTime> = 
    let pattern = "(?<hour>\d{2}):(?<minute>\d{2}) (?<day>\d{2})-(?<month>\w*)-(?<year>\d{2})"
    let groups = Regex.Match(source, pattern).Groups
    try 
        let minute = int <| groups.Item("minute").Value
        let hour = int <| groups.Item("hour").Value
        let day = int <| groups.Item("day").Value
        let month = decodeMonth <| groups.Item("month").Value
        let year = int <| groups.Item("year").Value
        let dt = new DateTime( year = 2000 + year
                             , month = month
                             , day = day
                             , hour = hour
                             , minute = minute
                             , second = 0 )
        Some dt
    with
    | ex -> None


let formatDate (odate:option<DateTime>) : option<Value> = 
    Option.map prologDateTime odate


let main () = 
    let outFile = outputFile "power_up_times.pl"

    let rows = readPowerUpRow ()
    
    let powerUpClause (row:PowerUpRow) : option<Clause> = 
        Clause.optionCons( signature = "outstation(os_name, last_power_up, last_polled)."
                         , body = [ optPrologSymbol row.``OS name``
                                  ; formatDate  <| parseDate    row.``Last power up``
                                  ; formatDate  <| parseDate    row.``Last polled`` 
                                  ] )

    let facts : FactBase  = 
        rows |> List.map powerUpClause |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module ("power_up_times", "power_up_times.pl", facts)

    pmodule.Save(outFile)

            