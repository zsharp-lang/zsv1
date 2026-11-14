// std:boolean — Z#'s truth values: the type Bool, and the operators on it.
//
// Each operator is declared with no code, so it is what the platform implements it with:
// the platform says, as std:zsharp/clr does for the CLR. Conditions still take the CLR's
// Boolean, which Bool is cast to, until Z#'s types and the platform's convert.

import { operator } from "core:zsharp";
import { Truth } from "std:boolean/truth";

// ---- The type

def Bool of Truth() {}

// ---- The operators

@operator((==)) fun equal(left: Bool, right: Bool): Bool;
@operator((!=)) fun notEqual(left: Bool, right: Bool): Bool;
