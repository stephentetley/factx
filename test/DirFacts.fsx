// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"


#load "..\src\Old\FactX\Internal\PrintProlog.fs"
#load "..\src\Old\FactX\Internal\PrologSyntax.fs"
#load "..\src\Old\FactX\FactOutput.fs"
#load "..\src-extra\FactX\Extra\PathString.fs"
#load "..\src-extra\FactX\Extra\LabelledTree.fs"
#load "..\src-extra\FactX\Extra\DirectoryListing.fs"
open Old.FactX
open FactX.Extra.DirectoryListing
open System.IO
open System.Drawing
open System.Drawing


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

let main (localFile:string) = 
    let infile = getLocalDataFile localFile
    let name1 = Path.GetFileName infile |> fun x -> Path.ChangeExtension(x,"pl")
    let outfile = outputFile name1
    writeListing infile "directories" outfile

let dateString (odate:System.DateTime option) : string = 
    match odate with
    | None -> ""
    | Some date -> date.ToString("dd/MM/yyyy")



let printFileRow (path:string) (row:Row) : unit =
    match row with
    | FileRow(_,_,_,_) -> 
        printfn "%s,%s" row.Name (dateString row.Properties.ModificationTime)
    | FolderRow(_,_,_) -> ()

let printBlock (block:Block) : unit = 
    List.iter (printFileRow block.Path) block.Rows


let tempCsv (localFile:string) = 
    let infile = getLocalDataFile localFile
    match readDirRecurseOutput infile with
    | Choice1Of2 msg -> failwith msg
    | Choice2Of2 blocks -> List.iter printBlock blocks




