open System.Text

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data
#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"


#I @"..\packages\ExcelProvider.1.0.1\typeproviders\fsharp41\net45"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel


#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\FactOutput.fs"
#load "..\FactX\FactX\Extra\ExcelProviderHelper.fs"
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
         
    excelReadRowsAsList helper (new InstallationsTable()) // |> List.take 100

let makeOutputPath (fileName:string) : string = 
    System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..", "data", fileName)




// ** Generate Prolog facts.
let genAddresses () = 
    let outFile = makeOutputPath "addresses.pl"
    
    let addressHelper = 
        { new IFactHelper<InstallationsRow> with
            member this.Signature = "address(refnum, full_address)."
            member this.ClauseBody row = 
                let addr = 
                    match row.``Full Address`` with
                    | null -> ""
                    | s -> s                            
                [ PQuotedAtom    row.InstReference
                ; PString        addr ] 
        } 

    let addresses : FactSet = readInstallations () |> makeFactSet addressHelper

    let pmodule : Module = 
        new Module("addresses", "addresses.pl", addresses)

    pmodule.Save(outFile)


    


let genAssetNames () = 
    let outFile = makeOutputPath "asset_names.pl"

    let namesHelper = 
        { new IFactHelper<InstallationsRow> with
            member this.Signature = "asset_name(refnum, name)."
            member this.ClauseBody row = 
                [ PQuotedAtom    row.InstReference
                ; PString        row.InstCommonName ] 
        }

    let assetNames : FactSet = readInstallations () |> makeFactSet namesHelper

    let pmodule : Module= 
        new Module("asset_names", "asset_names.pl", assetNames)

    pmodule.Save(outFile)
    

let main () : unit = 
    genAddresses ()
    genAssetNames ()
    printfn "Done."

