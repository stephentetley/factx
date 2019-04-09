// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause


#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190304\lib\netstandard2.0"
#r "SLFormat"
open SLFormat.Pretty


#load "..\src\FactX\Internal\Common.fs"
#load "..\src\FactX\Internal\Syntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\FactWriter.fs"
open FactX.Internal.Syntax
open FactX.FactWriter

let outputFileName (filename:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__, "../data/", filename) 


let demo01 () = 
    let outPath = outputFileName "dummy_writer.pl"
    runFactWriter 160 outPath 
        <|  factWriter {
            do! comment "dummy_writer.pl"
            do! moduleDirective "directories" ["listing/1"]
            return ()
        }
