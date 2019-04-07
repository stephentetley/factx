// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

namespace FactX

module FactOutput2 = 

    open FactX.Internal.Syntax2



    type Signature = Signature of string * string list

    /// A Fact is a Functor with a signature
    type Fact = Fact of Atom * Term list
