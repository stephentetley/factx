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
    let arrT : TermT [] = plNewTermRefs 2
    let a1 = arrT.[0]
    let a2 = arrT.[1]
    plPutInteger(a1, 50) |> ignore
    plPutInteger(a2, 51) |> ignore
    let ans1 = plUnifyInteger(a1,50) 
    let ans2 = plUnifyInteger(a1,50) 
    let ans3 = plUnifyInteger(a2,51) 
    printfn "a1=%i, a2=%i, a3=%i" ans1 ans2 ans3
    let err,n = plGetInteger(a1)
    printfn "err,n=%i,%i" err n

    Thread.Sleep(3600)
    plDiscardForeignFrame(fid)
    PL_halt 1


let test02 () = 
    let code = PL_initialise(3, [| "./"; "-q"; "-nosignals" |])
    printfn "code: %i" code
    let fid = plOpenForeignFrame() 

    let a1 = plNewTermRef ()
    plPutInteger(a1, 50) |> ignore

    let err,n = plGetInteger(a1)
    printfn "err,n=%i,%i" err n

    Thread.Sleep(3600)
    plDiscardForeignFrame(fid)
    PL_halt 1
    