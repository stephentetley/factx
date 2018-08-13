open System.Text

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\FactOutput.fs"
#load "FactX\ExcelProviderHelper.fs"
open FactX
open FactX.ExcelProviderHelper

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

    let makeClause (row:InstallationsRow) : Clause = 
        { FactName = "address"  
          Values = [ PQuotedAtom row.InstReference; PString row.``Full Address``] }

    let facts : FactCollection = 
        { FactName = "address"
          Arity = 2
          Signature = "address(refnum, addr)."
          Clauses = readInstallations () |> List.map makeClause } 

    let pmodule : Module = 
        { ModuleName = "addresses"
          GlobalComment = "addresses.pl"
          FactCols = [facts] }

    pmodule.Save(outFile)

let genAssetNames () = 
    let outFile = makeOutputPath "asset_names.pl"

    let makeClause (row:InstallationsRow) : Clause = 
        { FactName = "asset_name"
          Values = [PQuotedAtom row.InstReference; PString row.InstCommonName ] }

    let facts : FactCollection = 
        { FactName = "asset_name"
          Arity = 2
          Signature = "asset_name(refnum, name)."
          Clauses = readInstallations () |> List.map makeClause } 

    let pmodule : Module= 
        { ModuleName = "asset_names"
          GlobalComment = "asset_names.pl"
          FactCols = [facts] }
    
    pmodule.Save(outFile)
    

let main () : unit = 
    genAddresses ()
    genAssetNames ()
    printfn "Done."

