// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190207\lib\netstandard2.0"
#r "SLFormat"


#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\PathString.fs"
#load "..\src\FactX\Extra\LabelledTree.fs"
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
    | Choice2Of2 ans -> printfn "%s" <| ans.ToString()




// Note - to make facts a "filestore" must have a name
// The obvious name is the <path-to-root>.

//let test02 () = 
//    let path1 = getLocalDataFile "dir.txt"
//    match readDirRecurseOutput path1 with
//    | Choice1Of2 err -> failwith err
//    | Choice2Of2 ans -> 
//        let fs:FactBase = fileStore ans in (fs.ToProlog()) |> printfn "%A" 
//        let fs:FactBase = drive ans in (fs.ToProlog()) |> printfn "%A" 

// SWI-Prolog has a pcre module which suggests representing paths
// as lists of strings might be useful.
let pathList (path:FilePath) : Value = 
    path.Split('\\') |> Array.toList |> List.map prologString |> prologList


let writeListing (infile:string) (name:string) (outfile:string) : unit =
    match listingToProlog infile with
    | None -> printfn "Could not interpret the directory listing: '%s'" infile
    | Some facts -> 
        let pmodule : Module = 
            new Module( name = name
                      , comment = name
                      , db = facts )
        pmodule.Save(lineWidth = 160, filePath=outfile)

// We should consider generating SWI Prolog record accessors

let main () = 
    let outfile = outputFile "directories.pl"
    let infile = getLocalDataFile "dir.txt"
    writeListing infile "directories" outfile

let temp01 () = 
    let infile = getLocalDataFile "rtu-listing.txt"
    let outfile = outputFile "rtu.pl"
    writeListing infile "rtu" outfile