// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.DirectoryListing


open System
open System.IO

open FParsec


open FactX
open FactX.Extra.LabelledTree

[<AutoOpen>]
module DirectoryListing = 


    type Name = string
    type FilePath = string
    type Size = int64


    // Mode is not currently interpreted
    type Properties = 
        { Mode : string option
          ModificationTime : DateTime option
        }
 
    type Row = 
        | FolderRow of Name * Properties * FilePath
        | FileRow of Name * Properties * Size * FilePath
        member x.Name = 
            match x with
            | FolderRow(name,_,_) -> name
            | FileRow(name,_,_,_) -> name

        member x.Path = 
            match x with
            | FolderRow(_,_,path) -> path
            | FileRow(_,_,_,path) -> path

    type Block = 
        { Path: FilePath 
          Rows: Row list }
        




    let makeDateTime (year:int) (month:int) (day:int) (hour:int) (minute:int) (second:int) : DateTime = 
        new DateTime(year, month, day, hour, minute, second)


    
    // *************************************
    // PARSER

    // Parsing output of "dir" or "dir -Recurse" (Windows)

    // Utility combinators
    let private ws : Parser<string,unit> = manyChars (pchar ' ' <|> pchar '\t')
    let private ws1 : Parser<string,unit> = many1Chars (pchar ' ' <|> pchar '\t')

    let private symbol (p:Parser<'a,unit>)      : Parser<'a,unit> = p .>> ws

    let private keyword (s:string) : Parser<string,unit> = pstring s .>> ws
    let private keyword1 (s:string) : Parser<string,unit> = pstring s .>> ws1


    let private emptyLine : Parser<unit,unit> = newline >>. preturn ()

    // Names may span multiple lines
    let private pName : Parser<Name,unit> = 
        let line1 = restOfLine true
        let linesK = many1 (pchar ' ') >>. restOfLine true
        parse { 
            let! s = line1 
            let! ss = many linesK 
            let name1 = String.concat "" (s::ss)
            return name1.Trim()
            }


    // Note this is UK centric    
    let private pDateTime : Parser<DateTime,unit> = 
        pipe5   pint32 
                (pchar '/' >>. pint32) 
                (pchar '/' >>. symbol pint32) 
                pint32 
                (pchar ':' >>. pint32)
                (fun dd dm dy th tm -> makeDateTime dy dm dd th tm 0)
    
    let private pMode : Parser<string,unit> = many1Chars (lower <|> pchar '-') 

    let private isDir (mode:string) : bool = mode.StartsWith("d")



    let private pDirectoryDirective : Parser<Name,unit> = 
        let indent = manyChars (pchar ' ')
        indent >>. keyword1 "Directory:" >>. pName

    let private pHeadings : Parser<string list,unit> = 
        let columns = pipe4 (keyword "Mode")
                            (keyword "LastWriteTime")
                            (keyword "Length")
                            (keyword "Name")
                            (fun a b c d -> [a;b;c;d])
        let underline = restOfLine false
        columns .>> newline .>> underline


    let private pFolder (pathTo:string) (mode:string) : Parser<Row, unit> = 
        parse { 
            let! timestamp = symbol pDateTime 
            let! name = pName 
            return (FolderRow (name, { Mode = Some mode; ModificationTime = Some timestamp}, pathTo))
            }

    let private pFile (pathTo:string) (mode:string) : Parser<Row, unit> = 
        parse { 
            let! timestamp = symbol pDateTime
            let! size = symbol pint64
            let! name = pName 
            return (FileRow (name, { Mode = Some mode; ModificationTime = Some timestamp}, size, pathTo))
            }

    // Note - file store is flat at parse time (represented as a "Row")
    // It needs postprocessing to build.
    let private pRow (pathTo:string) : Parser<Row,unit> = 
        let parseK mode = 
            if isDir mode then pFolder pathTo mode else pFile pathTo mode
        (symbol pMode) >>= parseK





    let private pBlock : Parser<Block, unit> = 
        parse { 
            let! parent = (spaces >>. pDirectoryDirective) 
            do! emptyLine
            do! emptyLine
            let! _ = pHeadings .>> newline
            let! rows = many1 (pRow parent)
            return { Path = parent; Rows = rows }
            }



    let private pListing : Parser<Block list,unit> = many (pBlock .>> spaces)

    let readDirRecurseOutput (inputPath:string) : Choice<string,Block list> = 
        let source = File.ReadAllText(inputPath)
        match runParserOnString pListing () inputPath source with
        | Success(a,_,_) -> Choice2Of2 a
        | Failure(s,_,_) -> Choice1Of2 s



    // *************************************
    // Build from flat.

    // TODO - potentially we should (optionally) generate SWI-Prolog records
    // for properties, (and file and folder?)
    // See Manual, Section A.31 library(record)

    type Label = 
        | FolderLabel of Name * Properties
        | FileLabel of Name * Properties * Size
        member x.Name = 
            match x with
            | FolderLabel(name,_) -> name
            | FileLabel(name,_,_) -> name
        member x.Properties = 
            match x with
            | FolderLabel(_,props) -> props
            | FileLabel(_,props,_) -> props
        


    let private treeHelper : ILabelledTreeBuilder<Row,Label> = 
        { new ILabelledTreeBuilder<Row,Label>
          with member this.GetParentName (row:Row) = 
                    match row with
                    | FileRow(_,_,_,path) -> path
                    | FolderRow(_,_,path) -> path

               member this.MakeNode (row:Row) = 
                    match row with
                    | FileRow(name,props,sz,path) ->
                        let fullpath = path + "\\" + name
                        Leaf(fullpath,FileLabel(name,props,sz)) 

                    | FolderRow(name,props,path) ->
                        let fullpath = path + "\\" + name
                        Tree(fullpath,FolderLabel(name,props),[]) }


    let fileObjToValue (fobj:LabelledTree<Label>) : Value = 
        let getDateTime (label:Label) : Value = 
            match label.Properties.ModificationTime with
                    | None -> prologAtom "unknown"
                    | Some dt -> prologDateTime dt
        
        let getMode (label:Label) : Value = 
            match label.Properties.Mode with
                    | None -> prologAtom "unknown"
                    | Some dt -> prologSymbol dt

        let rec work (x:LabelledTree<Label>) : Value = 
            match x with
            | Tree (_, label, kids) -> 
                prologFunctor "folder_object" [ prologSymbol label.Name
                                              ; getDateTime label
                                              ; getMode label
                                              ; prologList (List.map work kids)]
            | Leaf (_, label) -> 
                let sz = 
                    match label with
                    | FileLabel (_,_,sz) -> sz
                    | _ -> 0L
                prologFunctor "file_object" [ prologSymbol label.Name
                                            ; getDateTime label
                                            ; getMode label
                                            ; prologInt64 sz ]
        work fobj

    let private buildFileStore1 (blocks:Block list) : LabelledTree<Label> list = 
        let allRows = List.collect (fun (b:Block) -> b.Rows) blocks
        match blocks with
        | [] -> []
        | b1 :: _ -> 
            let getRoots (xs:Row list) = 
                List.filter (fun (row:Row) -> row.Path = b1.Path) xs
            buildTopDownForest treeHelper getRoots allRows

    let private buildFileStore (blocks:Block list) : Value = 
        let root = match blocks with | [] -> ""| (b1 :: bs) -> b1.Path
        let trees = buildFileStore1 blocks
        let kids = List.map (fun (tree:LabelledTree<Label>) -> fileObjToValue tree) trees
        prologFunctor "file_store" [ prologSymbol root; prologList kids ]

    let listingToProlog (inputPath:string) (name:string) : option<FactBase> =
        match readDirRecurseOutput inputPath with
        | Choice1Of2 err -> failwith err
        | Choice2Of2 ans -> 
            let store = buildFileStore ans
            let c1 = Clause.cons( signature = "listing(name,store)."
                                , body = [prologSymbol name; store] )
            Some <| FactBase.ofList [c1]

