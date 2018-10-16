// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"

#I @"..\packages\ExcelProvider.1.0.1\lib\net45"
#r "ExcelProvider.Runtime.dll"

#I @"..\packages\ExcelProvider.1.0.1\typeproviders\fsharp41\net45"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel

#load "..\src\FactX\Internal\PrettyPrint.fs"
#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src\FactX\Extra\PathString.fs"
#load "..\src\FactX\Extra\LabelledTree.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.PathString
open FactX.Extra.LabelledTree


let outputFile (filename:string) : string = 
    System.IO.Path.Combine(@"D:\coding\prolog\spt-misc\prolog\screens\facts", filename)



type AssetTable = 
    ExcelFile< FileName = @"G:\work\ADB-exports\STANDARD_INST.xlsx",
               SheetName = "Sheet1!",
               ForceString = true >

type AssetRow = AssetTable.Row

/// ExcelProvider can read "data" files not just the file the type was 
/// instantiated with.
let readAssetSpreadsheet (filename:string) (sheetname:string) : AssetRow list = 
    let helper = 
        { new IExcelProviderHelper<AssetTable,AssetRow>
          with member this.ReadTableRows table = table.Data 
               member this.IsBlankRow row = match row.GetValue(0) with null -> true | _ -> false }
         
    excelReadRowsAsList helper (new AssetTable(filename,sheetname))

let stfData () = readAssetSpreadsheet @"G:\work\ADB-exports\BE_NO 2 STF.xlsx" "Sheet1"
let stwData () = readAssetSpreadsheet @"G:\work\ADB-exports\BE_NO 2 STW.xlsx" "Sheet1"

type AssetType = 
    | BusinessUnit | System | Function | Installation | ProcessGroup
    | Process | PlantAssembly | PlantItem 
    override v.ToString() = 
        match v with
        | BusinessUnit -> "BUSINESS UNIT"
        | System -> "SYSTEM"
        | Function -> "FUNCTION"
        | Installation -> "INSTALLATION"
        | ProcessGroup -> "PROCESS GROUP"
        | Process -> "PROCESS"
        | PlantAssembly -> "PLANT ASSEMBLY"
        | PlantItem -> "PLANT ITEM"


type AssetStatus = 
    | Operational
    | StatusOther of string






let decodeAssetStatus (statusText:string) : AssetStatus = 
    match statusText with
    | "OPERATIONAL" -> Operational
    | null -> StatusOther "<null>"
    | _ -> StatusOther statusText



let decodeHKey (hkey:string) : option<AssetType> = 
    match hkey with
    | null -> None
    | _ -> 
        match hkey.Length with
        | 2 -> Some BusinessUnit
        | 4 -> Some System
        | 8 -> Some Function 
        | 13 -> Some Installation
        | 20 -> Some ProcessGroup
        | 24 -> Some Process
        | 31 -> Some PlantAssembly
        | 36 -> Some PlantItem
        | _ -> None


let test01 () = 
    stwData () 
        |> List.iter (fun row -> printfn "%A" (decodeHKey row.``Hierarchy Key``))




let findInstallation (rows:AssetRow list) : option<AssetRow> = 
    let isInstallation (row:AssetRow) = 
        match decodeHKey row.``Hierarchy Key`` with
        | Some Installation -> true
        | _ -> false

    List.tryFind isInstallation rows

let test02 () = 
    stwData () |> findInstallation

type NodeLabel = 
    { Uid: string 
      Name: string
      Operational: AssetStatus
      AssetType: option<AssetType>
      GridRef: string
    }


let makeNode (row:AssetRow) : LabelledTree<NodeLabel> = 
    let path : PathString  =  pathString "/" row.``Common Name``
    let label = 
        { Uid = row.Reference
          Name = 
            if path.Length > 2 then 
                path.Last.Output() 
            else path.Subpath(0,2).Output()
                
          Operational = decodeAssetStatus row.AssetStatus
          AssetType = decodeHKey row.``Hierarchy Key``
          GridRef = row.``Loc.Ref.``
          }
    if path.ContainsRegex("^EQUIPMENT: ") then
        Leaf (row.``Common Name``, label)
    else
        Tree (row.``Common Name``, label, [])



let parentName (commonName:string): string = 
    let path : PathString = pathString "/" commonName
    path.SkipRight(1).Output()

let treeHelper : ILabelledTreeBuilder<AssetRow, NodeLabel> = 
    { new ILabelledTreeBuilder<AssetRow, NodeLabel>
      with member this.GetParentName (row:AssetRow) = parentName row.``Common Name``
           member this.MakeNode (row:AssetRow) = makeNode row }

let test03 () =
    buildTopDown treeHelper (stwData ())

type Installation = LabelledTree<NodeLabel>

let assetTypeName (assetType:option<AssetType>) : string = 
    match assetType with
    | None -> "UNKNOWN"
    | Some atype -> atype.ToString()

let assetStatus (status:AssetStatus) : string = 
    match status with
    | Operational -> "OPERATIONAL"
    | StatusOther s -> s

let rec nodeToProlog (node:LabelledTree<NodeLabel>) : Value = 
    match node with
    | Leaf (name,label) -> 
        prologFunctor "equipment" []
    | Tree (name,label,kids) -> 
        match label.AssetType with
        | Some ProcessGroup ->             
            prologFunctor "process_group" [prologList (List.map nodeToProlog kids)]
        | Some Process ->             
            prologFunctor "process" [prologList (List.map nodeToProlog kids)]
        | Some PlantAssembly ->             
            prologFunctor "plant_assembly" [prologList (List.map nodeToProlog kids)]
        | Some PlantItem ->             
            prologFunctor "plant_item" [prologList (List.map nodeToProlog kids)]
        | Some asset -> 
            let symb = prologSymbol <| asset.ToString()
            prologFunctor "generic_asset" [symb; prologList (List.map nodeToProlog kids)]
        | None -> 
            prologFunctor "unknown_asset" (List.map nodeToProlog kids)
            
let installationToProlog (inst:Installation) : FactBase = 
    let rootClause: option<Clause> = 
        match inst with
        | Tree(_, label, kids) -> 
            let kids1 = List.map nodeToProlog kids
            Some <| Clause.cons( signature = "installation(uid, name, status)."
                               , body = [ prologSymbol label.Uid
                                        ; prologSymbol label.Name
                                        ; prologSymbol (assetStatus label.Operational)
                                        ; prologFunctor "kids" [prologList kids1]
                                        ] )
        | Leaf _ -> None
    FactBase.ofOptionList [rootClause]


let test04 () =
    let outFile = outputFile "installation.pl"
    let inst = buildTopDown treeHelper (stwData ())
    let facts = 
        match inst with
        | None -> FactBase.ofList []
        | Some x -> installationToProlog x

    let pmodule : Module = 
        new Module( name = "installation"
                  , comment = "installation.pl"
                  , db = facts )

    pmodule.Save(outFile)
    