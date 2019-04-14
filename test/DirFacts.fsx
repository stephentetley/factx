// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"


#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Syntax.fs"
#load "..\src\FactX\Pretty.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
#load "..\src-extra\FactX\Extra\PathString.fs"
#load "..\src-extra\FactX\Extra\LabelledTree.fs"
#load "..\src-extra\FactX\Extra\DirectoryListing.fs"
open FactX
open FactX.FactWriter

open FactX.Extra.DirectoryListing
open System.IO

let getLocalDataFile (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"../data", fileName)


let test01 () = 
    let path1 = getLocalDataFile "dir.txt"
    match readDirRecurseOutput path1 with
    | Choice1Of2 err -> failwith err
    | Choice2Of2 ans -> printfn "%s" <| ans.ToString()




/// This should go in the DirectoryListing module...
let writeListing (inputPath:string) (moduleName:string)  : unit =
    match listingToProlog inputPath with
    | None -> printfn "Could not interpret the directory listing: '%s'" inputPath
    | Some listing -> 
        let justFile = sprintf "%s.pl" moduleName
        let outPath = Path.Combine( Path.GetDirectoryName(inputPath), justFile)
        runFactWriter 160 outPath 
            <|  factWriter {
                do! tellComment justFile
                do! timestamp
                do! newlines 3
                do! tellDirective (moduleDirective moduleName ["listing/1"])
                do! newline
                do! tellPredicate (predicate "listing" [listing])
                do! newline
                return ()
            }


// We should consider generating SWI Prolog record accessors

let main (inputPath:string) = 
    writeListing inputPath "directories"

let demo01 () = 
    let inputPath = getLocalDataFile "dir.txt"
    writeListing inputPath "directories"




