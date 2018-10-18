// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#load "..\src\FactX\Internal\PrettyPrint.fs"
#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\DirectoryListing.fs"
open FactX
open FactX.Extra.DirectoryListing
open System.IO


let getLocalDataFile (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"../data", fileName)

let outputFile (fileName:string) : string = 
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
let pathList (path:FilePath) : Value = 
    path.Split('\\') |> Array.toList |> List.map prologString |> prologList




let main () =
    let outFile = outputFile "directories.pl"
    let path1 = getLocalDataFile "dir.txt"

    match buildFactBase path1 with
    | None -> printfn "Could not decipher file '%s'" path1
    | Some facts -> 
        let pmodule : Module = 
            new Module( name = "directories"
                      , comment = "directories.pl"
                      , db = facts )

        pmodule.Save(lineWidth = 160, filePath=outFile)
