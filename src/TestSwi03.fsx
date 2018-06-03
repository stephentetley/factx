open System.Threading

#load @"FactX\SwiBridge\ApiStubs.fs"
#load @"FactX\SwiBridge\PrimitiveApi.fs"
#load @"FactX\SwiBridge\Easy.fs"
open FactX.SwiBridge.ApiStubs
open FactX.SwiBridge.PrimitiveApi
open FactX.SwiBridge.Easy



let test01 () = 
    let code = PL_initialise(3, [| "./"; "-q"; "-nosignals" |])
    printfn "code: %i" code
    let fid = plOpenForeignFrame() 
    let father = new Functor("father", 2)
    let mother = new Functor("mother", 2)
    let vX = new Term (())


    Thread.Sleep(3600)
    plDiscardForeignFrame(fid)
    PL_halt 1

        