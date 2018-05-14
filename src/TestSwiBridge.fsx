open System
open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop

#load @"FactX\SwiBridge\ApiStubs.fs"
open FactX.SwiBridge.ApiStubs


let test01 () = 
    let code = PL_initialise(3, [| "./"; "-q"; "-nosignals" |])
    printfn "%id" code
    PL_halt 1
