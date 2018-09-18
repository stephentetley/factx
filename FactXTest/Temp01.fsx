#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"
open FParsec

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\Internal\PrologSyntax.fs"
#load "..\FactX\FactX\FactOutput.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal
open FactX


let test01 () = 
    testRender <| (PrologSyntax.PDecimal 1.078M).Format()

/// FSharps Map<> is purely functional (d'oh!)
let temp01 () = 
    let m1 : Map<string,int list> = Map.empty
    m1.Add("four",[4]) |> ignore
    m1.Add("five",[5]) |> ignore
    m1

/// FSharps Map<> is purely functional (d'oh!)
let temp02 () = 
    let signature = FactSignature.parseSignature "person(name, age)."
    let m1 : FactBase = FactBase.empty
    m1.Add({Signature = signature; Body = [PrologSyntax.PString "stephen"; PrologSyntax.PInt 46]}) |> ignore
    m1

let temp02b () = 
    let signature = FactSignature.parseSignature "person(name, age)."
    let m1 : FactBase = FactBase.empty
    let m2 = m1.Add({Signature = signature; Body = [PrologSyntax.PString "stephen"; PrologSyntax.PInt 46]})
    m2

let temp03 () = 
    let signature = FactSignature.parseSignature "person(name, age)."
    let csyn = {Signature = signature; Body = [PrologSyntax.PString "stephen"; PrologSyntax.PInt 46]}
    let abssyn = csyn.ToProlog()
    printfn "%s" << render <| abssyn.Format() 

let temp04 () = 
    let m1 : Map<string,int list> = Map.empty.Add("four",[4])
    let m2 : Map<string,int list> = Map.empty.Add("five",[5]) 
    List.fold (fun ac (n,v) -> Map.add n v ac) m1 (Map.toList m2)

