// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

namespace FactX.Extra.LabelledTree


[<AutoOpen>]
module LabelledTree = 

    type Name = string

    /// A LabelledTree has a Name / Id (string) and a polymorphic label.
    /// The uid is essential for construction.
    type LabelledTree<'label> = 
        | Tree of Name * 'label * LabelledTree<'label> list
        | Leaf of Name * 'label


    // *************************************
    // Build from flat.

    /// F# design guidelines say favour object-interfaces rather than 
    /// records of functions...
    type ILabelledTreeBuilder<'row,'label> = 
        abstract member GetParentName : 'row -> Name
        abstract member MakeNode : 'row -> LabelledTree<'label>


    type private FlatKids<'label> = Map<Name, LabelledTree<'label> list>



    
    // Flat kids have no recusion, i.e. Tree is just Tree(_,_,[])
    let private makeFlatKids (helper:ILabelledTreeBuilder<'row,'label>) 
                                (rows:'row list) : FlatKids<'label> = 
        let step acc row = 
            let parent = helper.GetParentName row
            let node = helper.MakeNode row
            match Map.tryFind parent acc with
            | Some ns -> Map.add parent (node::ns) acc
            | None -> Map.add parent [node] acc
        List.fold step Map.empty rows

    /// Fill out children
   
    let private fillOutKids (store:FlatKids<'label>) (source:LabelledTree<'label>) : LabelledTree<'label> = 
        let rec work (node:LabelledTree<'label>) (cont:LabelledTree<'label> -> 'a) = 
            match node with
            | Leaf _ -> cont node
            | Tree(name,label,_) -> 
                let kids1 = 
                    match Map.tryFind name store with
                    | Some(ks) -> ks
                    | None -> []
                workList kids1 (fun xs -> let revs = List.rev xs in 
                                          cont (Tree (name, label, revs)))
        and workList (nodes:LabelledTree<'label> list) (cont :LabelledTree<'label> list -> 'a) = 
            match nodes with
            | [] -> cont []
            | z :: zs -> 
                work z      (fun x -> 
                workList zs (fun xs -> 
                cont (x :: xs))) 
        work source (fun a -> a)



    let buildTopDown (helper:ILabelledTreeBuilder<'row,'label>) 
                        (getRoot:'row list -> option<'row>)
                        (rows: 'row list) : LabelledTree<'label> option = 
        match getRoot rows with 
        | Some rootRow -> 
            let flatKids  = makeFlatKids helper rows
            Some <| fillOutKids flatKids (helper.MakeNode rootRow)
        | None -> None

    let buildTopDownForest (helper:ILabelledTreeBuilder<'row,'label>) 
                        (getRoots:'row list -> 'row list)
                        (rows: 'row list) : LabelledTree<'label> list = 
        let roots = getRoots rows
        let flatKids  = makeFlatKids helper rows
        List.map (fun root -> fillOutKids flatKids (helper.MakeNode root)) roots
