// std:reflection — what describes a type, as code asks for it as it runs.
//
// `reflect` is declared, with no code of its own: what describes a type depends on where
// the code runs, so whatever assembles the environment premaps it, as the prelude makes
// it the IL loader's description of a type. A platform with no reflection leaves it
// unmapped, and a build for one can give code that rebuilds a description instead.

import { Type } from "core:runtime";
import { export } from "core:zsharp";
import { Instance } from "std:zsharp/hir";

@export
fun reflect(type: Type): Instance;
