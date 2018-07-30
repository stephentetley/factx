open System.Text

#I @"..\packages\FSharp.Data.3.0.0-beta3\lib\net45"
#r @"FSharp.Data.dll"
open FSharp.Data

#I @"..\packages\ExcelProvider.0.8.2\lib"
#r "ExcelProvider.dll"
open FSharp.ExcelProvider

#load "FactX\Internal\FormatCombinators.fs"
#load "FactX\Internal\FactWriter.fs"
#load "FactX\Utils\ExcelProviderHelper.fs"
#load "FactX\FactOutput.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal.FactWriter
open FactX.Utils.ExcelProviderHelper
open FactX.FactOutput

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


// ** Generate Prolog facts.
let GenAddresses () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..","data/addresses.pl")
    let rows = readInstallations ()
    let proc1 (row:InstallationsRow) : FactWriter<unit> = 
        tell <| prologFact (simpleAtom "address")  
                            [ quotedAtom row.InstReference
                            ; prologString row.``Full Address``]
    let procAll : FactWriter<unit> = 
        factWriter {
            let! _ = tell <| prologComment "addresses.pl"
            let! _ = tell <| prologComment "At prompt type 'make.' to reload"
            let! _ = forMz rows proc1
            return () 
            }
    runFactWriter outFile procAll

let GenAssetNames () = 
    let outFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"..","data/asset_names.pl")
    let makeFact (row:InstallationsRow) : Fact = 
        { FactName = "asset_name"
          FactValues = [PQuotedAtom row.InstReference; PString row.InstCommonName ] }

    let facts : FactCollection = 
        { Name = "asset_name"
          Arity = 2
          Signature = "asset_name(refnum, name)."
          Facts = readInstallations () |> List.map makeFact } 

    let pmodule = 
        { ModuleName = "asset_names"
          GlobalComment = "asset_names.pl"
          FactCols = [facts] }
    
    pmodule.Save(outFile)
    

let main () : unit = 
    GenAddresses ()
    GenAssetNames ()
    printfn "Done."

// StringBuilder is capable of building a huge string...
let temp01 () = 
    let sb = new StringBuilder ()
    let table = (new InstallationsTable()).Data
    table |> Seq.iter (fun (row:InstallationsRow) -> sb.AppendLine(row.ToString()) |> ignore)
    sb.ToString ()

