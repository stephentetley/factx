module FactX.FactWriter

open System
open System.IO



type FactOutput<'a> = 
    FactOutput of (StreamWriter -> 'a)

/// monad code to fill out...

type TermWriter = private TermWriter of string

let inline private getTW (tw:TermWriter) : string = 
    match tw with
    | TermWriter value -> value

let tellFact (head:TermWriter) (body: TermWriter list) : FactOutput<unit> =
    FactOutput <| fun handle ->
        let args = String.concat ", " <| List.map getTW body
        let line = printfn "%s(%s)." (getTW head) args
        handle.WriteLine line

let tellBool (value:bool) : TermWriter = TermWriter <| if value then "true" else "false"