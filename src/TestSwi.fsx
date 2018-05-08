open System.Diagnostics
open System.Text

#load "FactX\SwiProcess.fs"
open FactX.SwiProcess


// swi-prolog: halt. to quit

let swiPath = @"C:\Program Files\swipl\bin\swipl.exe"


let temp01 () = 
    Process.Start(swiPath, "").WaitForExit()

let temp02 () = 
    runSwiProc swiPath ""

let temp03 () = 
    let prog = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "data/films.pl")
    printfn "'%s'" prog
    runSwiProc2 swiPath prog [| "credit(\"Vendredi Soir\", Y)." ; "halt." |]





