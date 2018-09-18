#I @"..\packages\FParsec.1.0.4-RC3\lib\portable-net45+win8+wp8+wpa81"
#r "FParsec"
#r "FParsecCS"
open FParsec

#load "..\FactX\FactX\Internal\FormatCombinators.fs"
#load "..\FactX\FactX\Internal\PrologSyntax.fs"
#load "..\FactX\FactX\FactOutput.fs"
open FactX.Internal.FormatCombinators
open FactX.Internal
open FactX.FactSignature


let test01 () = 
    let d1 = formatString "Hello" +^+ formatString "world!"
    let d2 = formatString "***** ******"
    render (indent 2 (d1 @@@ d2)) |> printfn "%s"

    let fact1 : Doc = 
        prologFact (formatString "address") 
                    [quotedAtom "UID001"; prologString "1, Yellow Brick Road" ]
    testRender fact1 

    let mdirective = 
        moduleDirective "os_relations" 
                        [ "osName", 2
                        ; "osType", 2
                        ; "odComment", 2
                        ]
    testRender mdirective 

let test02 () = 
    let doc1 = commaSepListVertically [formatString "one"; formatString "two"; formatString "three"]
    let doc2 = indent 10 doc1
    testRender doc1 
    testRender doc2

let test03 () = 
    let doc1 = indent 10 (formatString "start")
    testRender doc1

let test04 () = 
    vsep [formatString "start"; empty; formatString "end" ]
        |> testRender

// Temp - parsing signatures.

//let lexeme : Parser<string, 'u> = 
//    let opts = IdentifierOptions(isAsciiIdStart = isLetter)
//    identifier opts .>> spaces

//let lparen : Parser<unit, 'u> = (pchar '(') >>. spaces
//let rparen : Parser<unit, 'u> = (pchar ')') >>. spaces
//let comma : Parser<unit, 'u> = (pchar ',') >>. spaces
//let dot : Parser<unit, 'u> = (pchar '.') >>. spaces


//let pSignature : Parser<Signature, 'u> =
//    let body = between lparen rparen (sepBy lexeme comma)
//    pipe3 lexeme body dot (fun x xs _ -> Signature(x,xs))

let test05 () = 
    runParserOnString lexeme () "NONE" "identifier_one()."

let test06 () = 
    runParserOnString pSignature () "NONE" "identifier_one(blue, yellow)."

let test07 () = 
    testRender (PrologSyntax.PDecimal 1.078M).Format 

