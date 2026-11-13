// std:boolean/truth — the structure of Z#'s truth values.
//
// The type of truth values is a definition whose meta-type is a Truth, as `def Bool of
// Truth() {}`, which std:boolean defines. What a platform's truth values are is the
// platform's: the type lowers to what its target says they are. std:boolean/compiler is
// what the compiler needs of it.
//
// The nodes are the platform's own definitions, written with one meta-type, `Node`, so
// another platform reimplements them by giving that name its own.

import { String } from "core:runtime";
import { Class as Node } from "pkg:clr/hir";
import { public, explicitThis } from "pkg:clr/hir";
import { importers } from "core:zsharp";
import { entrypoint } from "std:framework";
import { Instance, Definition, Type, Named as NamedDefinition } from "pkg:zsharp/hir";

// ---- The HIR

// A type of truth values. Every node of one is the same type, whatever its name.
@public def Truth of Node : Instance, Definition, Type, NamedDefinition {
    @public let name: String;

    @public @explicitThis new(this) { this.name = ""; }
    @public @explicitThis new(this, name: String) { this.name = name; }

    @public @explicitThis get Name(this): String { return this.name; }
}

// ---- Installing it

// Importing it installs what compiles and loads its type. A program that only reflects
// loads it for its nodes, which doesn't import it, so it gets its structure alone.
@entrypoint
fun install() { importers.Import("std:boolean/compiler").Unwrap(); }
