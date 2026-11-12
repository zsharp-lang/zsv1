// std:project/core — what a module file runs in, which std:project gives it as
// `core:project`: the submodule running, what was emitted, and `compile` and `emit`.
//
// `compile()` compiles the running submodule's sources as one module, and runs the module
// file of each of its submodules. It is a plain call, so it runs as the module file does,
// in the document's scope: the sources see what the module file imported. Its sources need
// not import one another, as they are one module.

import { String, List, Directory, File, Path } from "core:runtime";
import { Instance, Type as HIRType } from "std:zsharp/hir";
import { Statement as StatementSyntax, Module as ModuleSyntax, ModuleTokens } from "std:zsharp/lang/ast";
import { Parser } from "std:zsharp/lang/parser";
import { Tokenizer, TextStream } from "std:zsharp/lang/tokenizer";
import { Grammar } from "std:zsharp/lang/syntax";
import { Module } from "std:modules";
import { compiler, metaTypes, ct, documents } from "core:zsharp";

// ---- A project

// A directory with a module file: its other `.zs` files are its sources, and so are those
// of a directory in it that has no module file of its own, while one that has is a
// submodule of it.
def Submodule {
    let Name: String;
    let Location: String;
    let ModuleFile: String;
    let Sources: List[String];
    let Submodules: List[Submodule];

    new(this, location: String) {
        this.Name = Path.GetFileName(location);
        this.Location = location;
        this.ModuleFile = Path.Combine(location, ".zs");
        this.Sources = List[String]();
        this.Submodules = List[Submodule]();
    }
}

def Project {
    let Root: Submodule;

    new(this, root: Submodule) { this.Root = root; }
}

// ---- Running

// The submodule whose module file runs, and what was emitted.
let current: Submodule;
let emitted: List[Instance];

// Runs a submodule's module file, as a document of its own each time the project is
// compiled.
fun run(submodule: Submodule) {
    let outer = current;
    current = submodule;
    documents.Execute(submodule.ModuleFile).Unwrap();
    current = outer;
}

fun parse(path: String): List[StatementSyntax] {
    let parser = Parser(Tokenizer.Tokenize(TextStream(File.ReadAllText(path))));
    return Grammar.Parse(parser).Statements;
}

// Runs a project's module files, from its root's, and gives what they emitted, in the
// order they did.
fun compileProject(project: Project): List[Instance] {
    let outer = emitted;
    emitted = List[Instance]();
    run(project.Root);

    let result = emitted;
    emitted = outer;
    return result;
}

// ---- What a module file is given

// Adds something to what compiling the project gives.
fun emit(instance: Instance) { emitted.Add(instance); }

// Adds a type, which it is given as what describes it, as a type is where one is
// expected: `emit(Int32)`.
fun emit(type: HIRType) { emitted.Add(type as Instance); }

// Compiles the running submodule's sources as a module named after its directory.
fun compile(): Module { return compile(current.Name); }

// Compiles the running submodule's sources as a module: the module meta-compiler compiles
// them, on a fork of the compiler, in the scope of the code that called it. Then each of
// its submodules' module file runs.
fun compile(name: String): Module {
    let submodule = current;

    let node = ModuleSyntax(ModuleTokens());
    node.Name = name;

    let index = 0;
    while (index < submodule.Sources.Count) {
        node.Body.AddRange(parse(submodule.Sources[index]));
        index = index + 1;
    }

    let compiled = metaTypes.Compile(compiler.Fork(), node, Module as Instance).Unwrap();
    let built = ct(compiled.Build().Unwrap()) as Module;

    index = 0;
    while (index < submodule.Submodules.Count) {
        run(submodule.Submodules[index]);
        index = index + 1;
    }

    return built;
}
