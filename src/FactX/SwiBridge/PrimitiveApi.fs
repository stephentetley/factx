module FactX.SwiBridge.PrimitiveApi

open System

open FactX.SwiBridge.ApiStubs

// F# 4.1 has Struct Unions (Single Case) that are like Haskell's newtype



// Prolog atom
[<Struct>]
type AtomT = AtomT of Atom_T
let inline internal getAtomT (atom:AtomT) : Atom_T = match atom with | AtomT a0 -> a0

// Name/arity pair
[<Struct>]
type FunctorT = FunctorT of Functor_T
let inline internal getFunctorT (functor:FunctorT) : Functor_T = match functor with | FunctorT a0 -> a0

[<Struct>]
type ModuleT = ModuleT of Module_T
let inline internal getModuleT (modulet:ModuleT) : Module_T = match modulet with | ModuleT a0 -> a0

[<Struct>]
type PredicateT = PredicateT of Predicate_T
let inline internal getPredicateT (pred:PredicateT) : Predicate_T = match pred with | PredicateT a0 -> a0

[<Struct>]
type RecordT = RecordT of Record_T
let inline internal getRecordT (record:RecordT) : Record_T = match record with | RecordT a0 -> a0




[<Struct>]
type TermT = TermT of Term_T
let inline internal getTermT (term:TermT) : Term_T = match term with | TermT a0 -> a0


[<Struct>]
type QidT = QidT of Qid_T
let inline internal getQidT (qid:QidT) : Qid_T = match qid with | QidT q0 -> q0

[<Struct>]
type FidT = FidT of Fid_T
let inline internal getFidT (fid:FidT) : Fid_T = match fid with | FidT a0 -> a0



let plContext () : ModuleT = 
    let m0 = PL_context () in ModuleT m0


let plIinitialise (argv:string list) : bool = 
    let arr = argv |> List.toArray
    if PL_initialise(arr.Length, arr) = 0 then true else false

let plHalt (status:int) : int = PL_halt(status)

let plToplevel () : bool = 
    if PL_toplevel () = 0 then true else false


let plOpenForeignFrame() : FidT = 
    let f0 = PL_open_foreign_frame () in FidT f0


let plDiscardForeignFrame (fid:FidT) : unit = 
    PL_discard_foreign_frame(getFidT fid)


let plNewAtom (label:string) : AtomT = 
    let a0 = PL_new_atom(label) in AtomT a0

let plNewFunctor (name:AtomT) (arity:int) : FunctorT  = 
    let f0 = PL_new_functor(getAtomT name, arity) in FunctorT f0

let plNewTermRef () : TermT = 
    let t0 = PL_new_term_ref () in TermT t0

let plNewTermRefs (n:int) : TermT [] = 
    let t0 = PL_new_term_refs(n)
    [| for i in 0 .. n-1 -> IntPtr.Add(t0,i) |> TermT |]

let plPutInteger(t:TermT, n:int) : int = 
    PL_put_integer(getTermT t, n)

let plGetInteger(t:TermT) : int * int = 
    let nptr:IntPtr = new IntPtr(0)
    let errno = PL_get_integer(getTermT t, nptr)
    let n = nptr.ToInt32() 
    errno, n

let plUnifyInteger(t:TermT, n:int) : int = 
    PL_unify_integer(getTermT t, n)

let plPredicate (name:string) (arity:int) (moduleName:string): PredicateT =
    let p0 = PL_predicate(name, arity, moduleName) in PredicateT p0

let  plRecord(term:TermT) : RecordT = 
    let r0 = PL_record (getTermT term) in RecordT r0


type PlQueryFlags = 
    | PlQNormal             = 0x0002
    | PlQNoDebug            = 0x0004
    | PlQCatchException     = 0x0008
    | PlQPassException      = 0x0010
    | PlQAllowYield         = 0x0020
    | PlQExtStatus          = 0x0040

let plOpenQuery (moduleCtx:ModuleT) (flags: PlQueryFlags list) (predicate:PredicateT) (term:TermT) : QidT = 
    let flagsInt : int = List.fold (fun ac flag -> ac ||| int flag) 0 flags
    let q0 = PL_open_query(getModuleT moduleCtx, flagsInt, getPredicateT predicate, getTermT term) in QidT q0
