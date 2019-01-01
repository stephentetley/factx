// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause



#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"


#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#load "..\src\FactX\Internal\PrettyPrint.fs"
#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
open FactX
open FactX.Extra.ExcelProviderHelper

// ********** DATA SETUP **********

type InstallationsTable = 
    ExcelFile< @"G:\work\Projects\events2\er-report\Installations.xlsx",
                SheetName = "Installations",
                ForceString = true >

type InstallationsRow = InstallationsTable.Row

let readInstallations () : InstallationsRow list = 
    let helper = 
        { new IExcelProviderHelper<InstallationsTable,InstallationsRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new InstallationsTable()) 

let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)




// ** Generate Prolog facts.
let genAddresses () = 
    let outFile = makeOutputPath "addresses.pl"

    let makeClause (row:InstallationsRow) : Option<Clause>= 
        Clause.optionCons ( signature = "address(refnum, full_address)."
                          , body = [ optPrologSymbol    row.InstReference
                                   ; optPrologString    row.``Full Address`` ] )


    let addresses : FactBase = 
        readInstallations () |> List.map makeClause  |> FactBase.ofOptionList

    let pmodule : Module = 
        new Module("addresses", "addresses.pl", addresses)

    pmodule.Save(outFile)


let genAssetNames () = 
    let outFile = makeOutputPath "asset_names.pl"

    let makeClause (row:InstallationsRow) : option<Clause> = 
        Clause.optionCons ( signature = "asset_name(refnum, name)."
                          , body = [ optPrologSymbol    row.InstReference
                                   ; optPrologString    row.InstCommonName ] )

    let assetNames : FactBase = 
        readInstallations () |> List.map makeClause |> FactBase.ofOptionList

    let pmodule : Module= 
        new Module("asset_names", "asset_names.pl", assetNames)

    pmodule.Save(outFile)
    

let main () : unit = 
    genAddresses ()
    genAssetNames ()
    printfn "Done."

