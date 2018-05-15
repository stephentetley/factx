open System
open System.Runtime.InteropServices
open System.Threading
open Microsoft.FSharp.NativeInterop

#load @"FactX\SwiBridge\ApiStubs.fs"
open FactX.SwiBridge.ApiStubs


let test01 () = 
    let code = PL_initialise(3, [| "./"; "-q"; "-nosignals" |])
    printfn "code: %i" code
    let fid = PL_open_foreign_frame() 
    let term1 = PL_new_term_ref()
    let ans1 = PL_chars_to_term("write(\"hello world!\").", term1)
    printfn "ans1: %i" ans1
    let ans2 = PL_call(term1, IntPtr.Zero)
    printfn "ans2: %i" ans2
    Thread.Sleep(3600)
    PL_discard_foreign_frame(fid)
    PL_halt 1
