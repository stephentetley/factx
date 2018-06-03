module FactX.SwiBridge.Easy

open FactX.SwiBridge.PrimitiveApi



[<Struct>]
type Atom = 
    val internal atom : AtomT
    new (name:string) = { atom = plNewAtom name }

[<Struct>]
type Functor = 
    val internal atom : Atom
    val internal arity : int
    new (f:Atom, arity:int) = { atom = f; arity = arity }
    new (f:string, arity:int) = { atom = new Atom(f); arity = arity}

[<Struct>]
type Term = 
    val internal term : TermT
    new (a:unit) = { term = plNewTermRef () }

