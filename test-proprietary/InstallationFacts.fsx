﻿// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

#r "netstandard"

#I @"C:\Users\stephen\.nuget\packages\FParsec\1.0.4-rc3\lib\netstandard1.6"
#r "FParsec"
#r "FParsecCS"

#I @"C:\Users\stephen\.nuget\packages\slformat\1.0.2-alpha-20190207\lib\netstandard2.0"
#r "SLFormat"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\lib\netstandard2.0"
#r "ExcelProvider.Runtime.dll"

#I @"C:\Users\stephen\.nuget\packages\ExcelProvider\1.0.1\typeproviders\fsharp41\netstandard2.0"
#r "ExcelDataReader.DataSet.dll"
#r "ExcelDataReader.dll"
#r "ExcelProvider.DesignTime.dll"
open FSharp.Interop.Excel



#load "..\src\FactX\Internal\PrintProlog.fs"
#load "..\src\FactX\Internal\PrologSyntax.fs"
#load "..\src\FactX\FactOutput.fs"
#load "..\src-extra\FactX\Extra\ExcelProviderHelper.fs"
#load "..\src-extra\FactX\Extra\PathString.fs"
#load "..\src-extra\FactX\Extra\String.fs"
#load "..\src-extra\FactX\Extra\LabelledTree.fs"
open FactX
open FactX.Extra.ExcelProviderHelper
open FactX.Extra.PathString
open FactX.Extra.String
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
    | _     when hkey.Contains("TODO") -> None
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
    let commonName = row.``Common Name``.Replace("EQPT:","EQUIPMENT:")
    let path : PathString  =  pathString "/" commonName
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
        Leaf (commonName, label)
    else
        Tree (commonName, label, [])



let parentName (commonName:string): string = 
    let path : PathString = pathString "/" commonName
    path.SkipRight(1).Output()

let treeHelper : ILabelledTreeBuilder<AssetRow, NodeLabel> = 
    { new ILabelledTreeBuilder<AssetRow, NodeLabel>
      with member this.GetParentName (row:AssetRow) = parentName row.``Common Name``
           member this.MakeNode (row:AssetRow) = makeNode row }

           
// Root is always first
// TODO this may be too strong a condition.
let getRoot (rows:'row list) : 'row option = 
    match rows with
    | x :: _ -> Some x
    | _ -> None

let test03 () =
    buildTopDown treeHelper getRoot (stwData ())

type Installation = LabelledTree<NodeLabel>

let assetTypeName (assetType:option<AssetType>) : string = 
    match assetType with
    | None -> "UNKNOWN"
    | Some atype -> atype.ToString()

let assetStatus (status:AssetStatus) : string = 
    match status with
    | Operational -> "OPERATIONAL"
    | StatusOther s -> s


let nodeToProlog (tree:LabelledTree<NodeLabel>) : Value = 
    let rec work (node:LabelledTree<NodeLabel>) (cont : Value -> 'a) = 
        match node with
        | Leaf (_,label) -> 
            let nameP = prologSymbol << rightOf "EQUIPMENT: " <| label.Name
            cont (prologFunctor "equipment" [prologSymbol label.Uid; nameP])
        | Tree (_,label,kids) -> 
            let nameP = prologSymbol label.Name
            match label.AssetType with
            | Some atype -> 
                match atype with
                | ProcessGroup -> 
                    workList kids (fun ks -> cont (prologFunctor "process_group" [nameP; prologList ks])) 
                | Process -> 
                    workList kids (fun ks -> cont (prologFunctor "process" [nameP; prologList ks])) 
                | PlantAssembly -> 
                    workList kids (fun ks -> cont (prologFunctor "plant_assembly" [nameP; prologList ks])) 
                | PlantItem -> 
                    workList kids (fun ks -> cont (prologFunctor "plant_item" [nameP; prologList ks])) 
                | other -> 
                    let symb = prologFunctor "type" [ prologSymbol (other.ToString())]
                    workList kids (fun ks -> cont (prologFunctor "generic_asset" [nameP; symb; prologList ks])) 
            | None -> 
                workList kids (fun ks -> cont (prologFunctor "unknown_asset" [nameP; prologList ks]))

    and workList (nodes:LabelledTree<NodeLabel> list) (cont : Value list -> 'a) = 
        match nodes with
        | [] -> cont []
        | z :: zs -> work z      (fun x -> 
                     workList zs (fun xs -> 
                     cont (x :: xs)))
    work tree (fun a -> a)

    

            
let installationToProlog (inst:Installation) : FactBase = 
    let rootClause: option<Clause> = 
        match inst with
        | Tree(_, label, kids) -> 
            let kids1 = List.map nodeToProlog kids
            Some <| Clause.cons( signature = "installation(uid, name, status, kids)."
                               , body = [ prologSymbol label.Uid
                                        ; prologSymbol label.Name
                                        ; prologSymbol (assetStatus label.Operational)
                                        ; prologFunctor "kids" [prologList kids1]
                                        ] )
        | Leaf _ -> None
    FactBase.ofOptionList [rootClause]


let main () =
    let outFile = outputFile "installations.pl"
    let inst1 = buildTopDown treeHelper getRoot (stwData ())
    let inst2 = buildTopDown treeHelper getRoot (stfData ())

    let makeFacts (inst:option<Installation>) : FactBase = 
        match inst with
        | None -> FactBase.ofList []
        | Some x -> installationToProlog x

    let pmodule : Module = 
        new Module( name = "installations"
                  , comment = "installations.pl"
                  , dbs = [makeFacts inst1; makeFacts inst2] )

    pmodule.Save(lineWidth = 160, filePath = outFile)
    