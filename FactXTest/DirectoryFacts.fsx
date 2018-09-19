// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\Internal\PrologSyntax.fs"
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\DirectoryListing.fs"
open FactX
open FactX.Internal
open FactX.Extra.DirectoryListing


let getLocalDataFile (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"../data", fileName)

let test01 () = 
    let path1 = getLocalDataFile "dir.txt"
    match readDirRecurseOutput path1 with
    | Choice1Of2 err -> failwith err
    | Choice2Of2 ans -> printfn "%s" <| display ans




// Note - to make facts a "filestore" must have a name
// The obvious name is the <path-to-root>.

let test02 () = 
    let path1 = getLocalDataFile "dir.txt"
    match readDirRecurseOutput path1 with
    | Choice1Of2 err -> failwith err
    | Choice2Of2 ans -> 
        let fs:FactBase = fileStore ans in (fs.ToProlog()) |> printfn "%A" 
        let fs:FactBase = drive ans in (fs.ToProlog()) |> printfn "%A" 

// SWI-Prolog has a pcre module which suggests representing paths
// as lists of strings might be useful.
let pathList (path:FilePath) : PrologSyntax.Value = 
    path.Split('\\') |> Array.toList |> List.map PrologSyntax.PString |> PrologSyntax.PList

/// If we encode LastWriteTime use something that can be parsed with
/// parse_time (SWI Prolog).
