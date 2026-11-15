// The Language Server Protocol, as types: what a client and a server say to each other,
// and nothing about how either does. The structures, the enumerations and the table of
// requests and notifications are generated from the specification's model by
// `generate.zs`; the rest is written here.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope.

import { compile, emit } from "core:project";
import { Object, String, Int32, Boolean, Double, List, Dictionary, Nullable } from "core:runtime";
import { static } from "std:oop";

emit(compile("LSP"));
