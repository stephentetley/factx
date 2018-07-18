#load @"FactX\SwiBridge\ApiStubs.fs"
#load @"FactX\SwiBridge\PrimitiveApi.fs"
open FactX.SwiBridge.PrimitiveApi

// SWI Prolog Manual page 375

let main (args: string list) : int = 
    if not (plIinitialise args) then
        plHalt(1)
    else 
        plHalt(if plToplevel () then 0 else 1)


let test01 () : int = main ["./"; "-q"; "-nosignals" ]