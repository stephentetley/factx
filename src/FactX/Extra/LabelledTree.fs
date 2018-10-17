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

    // Root is always first
    // TODO this may be too strong a condition.
    let private getRoot (rows:'row list) : 'row option = 
        match rows with
        | x :: _ -> Some x
        | _ -> None

    
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
    let rec private fillOutKids (store:FlatKids<'label>) (node:LabelledTree<'label>) : LabelledTree<'label> = 
        match node with
        | Leaf _ -> node
        | Tree(name,label,_) -> 
            let kids1 = 
                match Map.tryFind name store with
                | Some(xs) -> xs
                | None -> []
            let kids2 : LabelledTree<'label> list =
                // The list is reversed because we always have been adding to the front.
                List.rev <| List.map (fillOutKids store) kids1
            Tree (name, label, kids2) 



    let buildTopDown (helper:ILabelledTreeBuilder<'row,'label>) 
                        (rows: 'row list) : LabelledTree<'label> option = 
        match getRoot rows with 
        | Some rootRow -> 
            let flatKids  = makeFlatKids helper rows
            Some <| fillOutKids flatKids (helper.MakeNode rootRow)
        | None -> None



