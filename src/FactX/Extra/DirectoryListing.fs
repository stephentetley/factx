// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.DirectoryListing


open System
open System.IO
open System.Text

open FParsec

open FactX.Internal
open FactX
open FactX.Extra.PathString
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
    let private symbol1 (p:Parser<'a,unit>)     : Parser<'a,unit> = p .>> ws1

    let private keyword (s:string) : Parser<string,unit> = pstring s .>> ws
    let private keyword1 (s:string) : Parser<string,unit> = pstring s .>> ws1

    let private lineOf (p:Parser<'a,unit>) : Parser<'a,unit> = 
        p .>> newline

    let private twice (p:Parser<'a,unit>) : Parser<('a * 'a),unit> = pipe2 p p (fun a b -> (a,b))

    let private blankline : Parser<unit,unit> = lineOf ws >>. preturn ()

    let private pName : Parser<Name,unit> = restOfLine false |>> (fun s -> s.TrimEnd ())


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

    // Note - if directory name longer than 100(?) chars it is listed on a new line
    let pDirPath : Parser<Name,unit> = 
        let indent = pstring "    "
        let line1 = pName .>> newline
        let linesK = indent >>. pName .>> newline
        pipe2 line1 (many linesK) (fun s ss -> String.concat "" (s::ss))


    let private pDirectoryDirective : Parser<Name,unit> = 
        let indent = manyChars (pchar ' ')
        indent >>. pstring "Directory:" >>. spaces >>. pDirPath

    let private pHeadings : Parser<string list,unit> = 
        let columns = pipe4 (keyword "Mode")
                            (keyword "LastWriteTime")
                            (keyword "Length")
                            (keyword "Name")
                            (fun a b c d -> [a;b;c;d])
        let underline = restOfLine false
        columns .>> newline .>> underline

    // Note - file store is flat at parse time (represented as a "Row")
    // It needs postprocessing to build.
    let private pRow (pathTo:string) : Parser<Row,unit> = 
        let pFolder mode = 
             pipe2 (symbol pDateTime) 
                   pName 
                   (fun timestamp name -> FolderRow (name, { Mode = Some mode; ModificationTime = Some timestamp}, pathTo))
        let pFile mode = 
            pipe3 (symbol pDateTime) 
                  (symbol pint64) 
                  pName 
                  (fun timestamp size name-> FileRow (name, { Mode = Some mode; ModificationTime = Some timestamp}, size, pathTo))
        let parseK mode = 
            if isDir mode then pFolder mode else pFile mode
        (symbol pMode) >>= parseK





    let private pBlock : Parser<Block, unit> = 
        parse { 
            let! parent = (spaces >>. pDirectoryDirective)  
            do! blankline
            do! blankline
            let! _ = lineOf pHeadings
            let! rows = many1 (lineOf (pRow parent))
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

    // TODO - should use LabelledTree builder

    type Label = 
        | FolderLabel of Name * Properties
        | FileLabel of Name * Properties * Size
        member x.Name = 
            match x with
            | FolderLabel(name,_) -> name
            | FileLabel(name,_,_) -> name


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
        let rec work (x:LabelledTree<Label>) : Value = 
            match x with
            | Tree (_, label, kids) -> 
                prologFunctor "folder" [ prologSymbol label.Name; prologList (List.map work kids)]
            | Leaf (_, label) -> 
                let sz = 
                    match label with
                    | FileLabel (_,_,sz) -> sz
                    | _ -> 0L
                prologFunctor "file" [ prologSymbol label.Name; prologInt64 sz ]
        work fobj

    let private buildFileStore (blocks:Block list) : LabelledTree<Label> list = 
        let allRows = List.collect (fun (b:Block) -> b.Rows) blocks
        match blocks with
        | [] -> []
        | b1 :: _ -> 
            let getRoots (xs:Row list) = 
                List.filter (fun (row:Row) -> row.Path = b1.Path) xs
            buildTopDownForest treeHelper getRoots allRows


    let listingToProlog (inputPath:string) : option<FactBase> =
        match readDirRecurseOutput inputPath with
        | Choice1Of2 err -> failwith err
        | Choice2Of2 ans -> 
            let root = match ans with | [] -> ""| (b1 :: bs) -> b1.Path

            let trees = buildFileStore ans
            let kids = List.map (fun (tree:LabelledTree<Label>) -> fileObjToValue tree) trees
            let c1 = Clause.cons( signature = "file_store(path,kids)."
                                , body = [prologSymbol root; prologList kids] )
            Some <| FactBase.ofList [c1]

