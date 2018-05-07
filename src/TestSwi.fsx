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



