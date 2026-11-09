// std:project — a minimal project compiler, in Z#, to build the real one with.
//
// A project is a tree of submodules, found from a directory. A submodule is a directory
// with a module file, `.zs`: its other `.zs` files are its sources, and so are those of
// a directory in it that has no module file of its own, while one that has is a
// submodule of it.
//
// Compiling a project runs the root's module file as a document. That code decides what
// the project gives: `emit(x)`, from `core:project`, adds x to what compiling gives, and
// `compile()` compiles the submodule's sources as one module, and runs the module file
// of each of its submodules. It is a plain call, so it runs as the module file does, in
// the document's scope: the sources see what the module file imported. Its sources need
// not import one another, as they are one module.
//
// What to do with what was emitted is the build script's.

import { String, List, DotNet_Module, Directory, File, Path } from "core:runtime";
import { Instance, Type as HIRType } from "std:zsharp/hir";
import { Statement as StatementSyntax, Module as ModuleSyntax, ModuleTokens } from "std:zsharp/lang/ast";
import { Parser } from "std:zsharp/lang/parser";
import { Tokenizer, TextStream } from "std:zsharp/lang/tokenizer";
import { Syntax } from "std:zsharp/lang";
import { Module } from "std:modules";
import { compiler, metaTypes, ct, core, documents, export } from "core:zsharp";

// ---- Discovery

@export
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

@export
def Project {
    let Root: Submodule;

    new(this, root: Submodule) { this.Root = root; }
}

def Discovery of DotNet_Module {
    fun submodule(location: String): Submodule {
        let found = Submodule(location);
        collect(found, location);
        return found;
    }

    // What a directory adds to the submodule it is in, in the order of their names.
    fun collect(owner: Submodule, location: String) {
        let files = List[String](Directory.EnumerateFiles(location, "*.zs"));
        files.Sort();

        let index = 0;
        while (index < files.Count) {
            if (Path.GetFileName(files[index]) != ".zs") owner.Sources.Add(files[index]);
            index = index + 1;
        }

        let directories = List[String](Directory.EnumerateDirectories(location));
        directories.Sort();

        index = 0;
        while (index < directories.Count) {
            let inner = directories[index];
            if (File.Exists(Path.Combine(inner, ".zs"))) owner.Submodules.Add(submodule(inner));
            else collect(owner, inner);
            index = index + 1;
        }
    }
}

// The project in a directory, which is its root submodule.
@export
fun discover(directory: String): Project {
    return Project(Discovery.submodule(Path.GetFullPath(directory)));
}

// ---- Compiling

// What is being compiled: the submodule whose module file runs, and what was emitted.
def Running of DotNet_Module {
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
        return Syntax.Install(parser).Parse(parser).Statements;
    }
}

// What a module file is given, as `core:project`.
def Live of DotNet_Module {
    // Adds something to what compiling the project gives.
    fun emit(instance: Instance) { Running.emitted.Add(instance); }

    // Adds a type, which it is given as what describes it, as a type is where one is
    // expected: `emit(Int32)`.
    fun emit(type: HIRType) { Running.emitted.Add(type as Instance); }

    // Compiles the running submodule's sources as a module named after its directory.
    fun compile(): Module { return compile(Running.current.Name); }

    // Compiles the running submodule's sources as a module: the module meta-compiler
    // compiles them, on a fork of the compiler, in the scope of the code that called it.
    // Then each of its submodules' module file runs.
    fun compile(name: String): Module {
        let submodule = Running.current;

        let node = ModuleSyntax(ModuleTokens());
        node.Name = name;

        let index = 0;
        while (index < submodule.Sources.Count) {
            node.Body.AddRange(Running.parse(submodule.Sources[index]));
            index = index + 1;
        }

        let compiled = metaTypes.Compile(compiler.Fork(), node, Module as Instance).Unwrap();
        let built = ct(compiled.Build().Unwrap()) as Module;

        index = 0;
        while (index < submodule.Submodules.Count) {
            Running.run(submodule.Submodules[index]);
            index = index + 1;
        }

        return built;
    }
}

core.Add("project", Live as Instance);

// Compiles a project, giving what its module files emitted, in the order they did.
@export
def Compiler {
    let project: Project;

    new(this, project: Project) { this.project = project; }

    fun compile(): List[Instance] {
        let outer = Running.emitted;
        Running.emitted = List[Instance]();
        Running.run(project.Root);

        let result = Running.emitted;
        Running.emitted = outer;
        return result;
    }
}
