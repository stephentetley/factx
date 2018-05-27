module FactX.SwiBridge.PrimitiveApi

open FactX.SwiBridge.ApiStubs

type FidT = FidT of Fid_T

let plOpenForeignFrame() : FidT = 
    let fid0 = PL_open_foreign_frame () in FidT fid0


let plDiscardForeignFrame (cid:FidT) : unit = 
    match cid with
    | FidT fid -> PL_discard_foreign_frame(fid)


