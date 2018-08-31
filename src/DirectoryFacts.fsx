// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#load "FactX\Extra\DirectoryListing.fs"
open FactX.Extra.DirectoryListing


let getLocalDataFile (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"../data", fileName)

let test01 () = 
    let path1 = getLocalDataFile "dir.txt"
    match readDirRecurseOutput path1 with
    | Choice1Of2 err -> failwith err
    | Choice2Of2 ans -> printfn "%s" <| display ans