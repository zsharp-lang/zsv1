// std:modules — Z#'s own modules, defined in Z#: their structure.
//
// A module is structure: its globals and its definitions. This is their HIR, which
// reflection needs alone. std:modules/compiler is what the compiler needs of them: the
// COs that lower a module and its globals, and the meta-compiler that builds a module's
// node, which importing it installs, so `module` is a Z# module wherever nothing else
// compiles one.
//
// The nodes are the platform's own definitions, written with one meta-type, `Node`, so
// another platform reimplements them by giving that name its own.

import { String, Object, List, DotNet_Class as Node, explicitThis } from "core:runtime";
import { importers } from "core:zsharp";
import { entrypoint } from "std:framework";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated } from "std:zsharp/hir";
import { Expression } from "std:zsharp/hir/code";

// ---- The HIR

// A global of a module: one variable, of the type written or of its initializer, which
// is the code that gives its value.
def Global of Node : Instance, Definition, Variable, NamedDefinition, Annotated {
    let name: String;
    let Type: Type;
    let Initializer: Expression;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// A module: its globals and its definitions. It is the root of what it holds, an
// instance and not a definition, so nothing that holds definitions can hold it. Its
// entry point, if it has one, is a definition of it that is called, with nothing, when
// the module is imported: what loads it into the environment that imports it, as a
// plugin is. Building it, as compiling its project does, doesn't call it.
def Module of Node : Instance, NamedDefinition, Annotated {
    let name: String;
    let Globals: List[Global];
    let Definitions: List[Definition];
    let EntryPoint: Instance;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Globals = List[Global]();
        this.Definitions = List[Definition]();
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// ---- Installing it

// Importing it installs what compiles its definitions, so `module` is a Z# module and a
// library, which is a module, can be loaded: importing any library imports it, as its
// nodes are modules. A program that only reflects loads it for its nodes, which doesn't
// import it, so it gets its structure alone.
@entrypoint
fun install() { importers.Import("std:modules/compiler").Unwrap(); }
