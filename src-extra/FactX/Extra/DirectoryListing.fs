// Copyright (c) Stephen Tetley 2018,2019
// License: BSD 3 Clause

namespace FactX.Extra.DirectoryListing




[<AutoOpen>]
module DirectoryListing = 
    open System
    open System.IO

    open FParsec

    open FactX
    open FactX.Extra.LabelledTree

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
        member x.Name 
            with get () : string = 
                match x with
                | FolderRow(name,_,_) -> name
                | FileRow(name,_,_,_) -> name

        member x.Properties 
            with get () :Properties = 
                match x with
                | FolderRow(_,props,_) -> props
                | FileRow(_,props,_,_) -> props

        member x.Path
            with get () : string = 
                match x with
                | FolderRow(_,_,path) -> path
                | FileRow(_,_,_,path) -> path


    type Block = 
        { Path: FilePath 
          Rows: Row list }
        




    let makeDateTime (year:int) (month:int) (day:int) (hour:int) (minute:int) (second:int) : DateTime = 
        new DateTime(year=year, month=month, day=day, hour=hour, minute=minute, second=second)


    
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


    let fileObjToValue (fobj:LabelledTree<Label>) : Term = 
        let getDateTime (label:Label) : Term = 
            match label.Properties.ModificationTime with
            | None -> nullTerm
            | Some dt -> dateTimeTerm dt
        
        let getMode (label:Label) : Term = 
            match label.Properties.Mode with
            | None -> nullTerm
            | Some s -> stringTerm s

        /// CPS transformed
        let rec work (x:LabelledTree<Label>) 
                     (cont: Term -> Term) : Term = 
            match x with
            | Tree (_, label, kids) -> 
                workList kids (fun vs -> 
                cont (functor "folder_object" 
                                     [ stringTerm label.Name
                                     ; getDateTime label
                                     ; getMode label
                                     ; listTerm vs]))
            | Leaf (_, label) -> 
                let sz = 
                    match label with
                    | FileLabel (_,_,sz) -> sz
                    | _ -> 0L
                cont (functor "file_object" 
                                    [ stringTerm label.Name
                                    ; getDateTime label
                                    ; getMode label
                                    ; int64Term sz ])
        
        and workList (kids:LabelledTree<Label> list) 
                     (cont: Term list -> Term) : Term = 
            match kids with
            | [] -> cont []
            | x :: xs ->
                work x (fun v1 -> 
                workList xs (fun vs -> 
                cont (v1::vs)))
        work fobj (fun x -> x)

    let private buildFileStore1 (blocks:Block list) : LabelledTree<Label> list = 
        let allRows = List.collect (fun (b:Block) -> b.Rows) blocks
        match blocks with
        | [] -> []
        | b1 :: _ -> 
            let getRoots (xs:Row list) = 
                List.filter (fun (row:Row) -> row.Path = b1.Path) xs
            buildTopDownForest treeHelper getRoots allRows

    let private buildFileStore (blocks:Block list) : Term = 
        let root = match blocks with | [] -> ""| (b1 :: bs) -> b1.Path
        let trees = buildFileStore1 blocks
        let kids = List.map (fun (tree:LabelledTree<Label>) -> fileObjToValue tree) trees
        functor "file_store" [ stringTerm root; listTerm kids ]

    let listingToProlog (inputPath:string) : option<Term> =
        match readDirRecurseOutput inputPath with
        | Choice1Of2 err -> printfn "%s" err; None
        | Choice2Of2 ans -> buildFileStore ans |> Some
            

