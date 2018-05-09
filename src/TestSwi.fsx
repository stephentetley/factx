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

let temp03 (cmd:string) = 
    let prog = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "data/actions.pl")
    printfn "'%s'" prog
    runSwiProc2 swiPath (sprintf "--quiet %s" prog) cmd


let temp04 (cmd:string) = 
    let xsbPath = @"C:\programs\XSB\bin\xsb.bat"
    runSwiProc2 xsbPath "" cmd

let temp05 () = 
    let pyPath:string = @"C:\programs\Python27\python.exe"
    runSwiProc2 pyPath "" "print 12 + 1"

