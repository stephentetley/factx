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
    
    let addressHelper = 
        { new IFactHelper<InstallationsRow> with
            member this.FactName = "address"
            member this.Signature = "address(refnum, full_address)."
            member this.Arity = 2
            member this.ClauseBody row = 
                [ PQuotedAtom    row.InstReference
                ; PString        row.``Full Address`` ] 
        } 

    let addresses : FactSet = readInstallations () |> makeFactSet addressHelper

    let pmodule : Module = 
        makeModule "addresses" "addresses.pl" [addresses]

    pmodule.Save(outFile)


    


let genAssetNames () = 
    let outFile = makeOutputPath "asset_names.pl"

    let namesHelper = 
        { new IFactHelper<InstallationsRow> with
            member this.FactName = "asset_name"
            member this.Signature = "asset_name(refnum, name)."
            member this.Arity = 2
            member this.ClauseBody row = 
                [ PQuotedAtom    row.InstReference
                ; PString        row.InstCommonName ] 
        }

    let assetNames : FactSet = readInstallations () |> makeFactSet namesHelper

    let pmodule : Module= 
        makeModule "asset_names" "asset_names.pl" [assetNames]

    pmodule.Save(outFile)
    

let main () : unit = 
    genAddresses ()
    genAssetNames ()
    printfn "Done."

