// std:functions — Z#'s own functions, defined in Z#: their structure.
//
// A Z# function is a function of its own, not a method of anything. This is its HIR node
// and its parameters', which reflection needs alone. std:functions/compiler is what the
// compiler needs of them: their COs and meta-compiler, which importing it installs, so
// `fun` is a Z# function wherever nothing else compiles one.
//
// The nodes are the platform's own definitions, written with one meta-type, `Node`, so
// another platform reimplements them by giving that name its own.

import { String, Object, List, DotNet_Class as Node, explicitThis } from "core:runtime";
import { importers } from "core:zsharp";
import { entrypoint } from "std:framework";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated, Overloadable } from "std:zsharp/hir";
import { Code, Local, Expression } from "std:zsharp/hir/code";

// ---- The HIR

// A Z# function. Its meta-compiler fills it through bodies, by these names. What is
// said of it, as a decorator says it is an entry point, is its annotations. Functions of
// one name overload.
def Function of Node : Instance, Definition, NamedDefinition, Annotated, Overloadable {
    let name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// A parameter that a positional argument fills, or else its default value.
def Positional of Node : Instance, Definition, Variable {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

// A parameter that an argument with its name fills, or else its default value.
def Named of Node : Instance, Definition, Variable {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

// ---- Installing it

// Importing it installs what compiles its definitions, so `fun` is a Z# function. A program that only
// reflects loads it for its nodes, which doesn't import it, so it gets its structure alone.
@entrypoint
fun install() { importers.Import("std:functions/compiler").Unwrap(); }
