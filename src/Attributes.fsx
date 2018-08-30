// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

type Atom (name:string) = 
    inherit System.Attribute()
    member v.Name = name

type QuotedAtom (name:string) = 
    inherit System.Attribute()
    member v.Name = name

type Clause (name:string) = 
    inherit System.Attribute()
    
    member v.Name = name

// Attributes are a potential alternative to IFactHelper.

[<Clause("person")>]
type Person = 
    {   
        [<QuotedAtom("name")>]
        Name: string
        
        [<Atom("age")>]
        Age: int
    }
        
let stephen = { Name = "Stephen"; Age = 46 }

// let clauseName (o:obj) : string option = 
    

let main () = 
    stephen.GetType().GetMembers()

       