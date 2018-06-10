module FactX.SwiBridge.Easy

open FactX.SwiBridge.PrimitiveApi


// Structs are value types, classes are reference types



[<Struct>]
type Atom = 
    val internal atom : AtomT
    new (name:string) = { atom = plNewAtom name }

[<Struct>]
type Functor = 
    val internal atom : AtomT
    val internal arity : int
    new (f:Atom, arity:int) = { atom = f.atom; arity = arity }
    new (f:string, arity:int) = { atom = plNewAtom f; arity = arity}

[<Struct>]
type Term = 
    val internal term : TermT
    new (a:unit) = { term = plNewTermRef () }






