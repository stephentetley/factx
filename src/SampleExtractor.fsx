#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#load "FactX\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
#load "FactX\ExcelProviderHelper.fs"
open FactX.FormatCombinators
open FactX.FactOutput
open FactX.ExcelProviderHelper

// ********** DATA SETUP **********

type InstallationsTable = 
    ExcelFile< @"G:\work\Projects\events2\er-report\Installations.xlsx",
                SheetName = "Installations",
                ForceString = true >

type InstallationsRow = InstallationsTable.Row

let getInstallations () : InstallationsRow list = 
    let dict : GetExcelRowsDict<InstallationsTable, InstallationsRow> = 
        { GetRows     = fun imports -> imports.Data 
          NotNullProc = fun row -> match row.GetValue(0) with null -> false | _ -> true }
    excelTableGetRows dict (new InstallationsTable())


// ** Generate Prolog facts.
let GenAddresses () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..","data/addresses.pl")
    let rows = getInstallations ()
    let proc1 (row:InstallationsRow) : FactOutput<unit> = 
        tellDoc 
            <| fact (simpleAtom "address")  [quotedAtom row.InstReference; prologString row.``Full Address``]
    let procAll : FactOutput<unit> = 
        factOutput {
            let! _ = tellComment "addresses.pl"
            let! _ = tellComment "At prompt type ``make.`` to reload"
            let! _ = forMz rows proc1
            return () 
            }
    runFactOutput outFile procAll

let GenAssetNames () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..","data/assetnames.pl")
    let rows = getInstallations ()
    let proc1 (row:InstallationsRow) : FactOutput<unit> = 
        tellDoc 
            <| fact (simpleAtom "assetName")  [quotedAtom row.InstReference; prologString row.InstCommonName]
    let procAll : FactOutput<unit> = 
        factOutput {
            let! _ = tellComment "assetnames.pl"
            let! _ = tellComment "At prompt type ``make.`` to reload"
            let! _ = forMz rows proc1
            return () 
            }
    runFactOutput outFile procAll

let main () : unit = 
    GenAddresses ()
    GenAssetNames ()
    printfn "Done."
