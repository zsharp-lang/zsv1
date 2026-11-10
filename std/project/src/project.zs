// std:project — a minimal project compiler, in Z#, to build the real one with.
//
// A project is a tree of submodules, found from a directory. A submodule is a directory
// with a module file, `.zs`: its other `.zs` files are its sources, and so are those of
// a directory in it that has no module file of its own, while one that has is a
// submodule of it.
//
// Compiling a project runs the root's module file as a document. That code decides what
// the project gives, with what std:project/core gives it as `core:project`: `emit(x)`
// adds x to what compiling gives, and `compile()` compiles the submodule's sources as one
// module, and runs the module file of each of its submodules.
//
// What to do with what was emitted is the build script's.

import { String, List, Directory, File, Path } from "core:runtime";
import { Instance } from "std:zsharp/hir";
import { core, importers } from "core:zsharp";
import { Submodule, Project, compileProject } from "std:project/core";
import { entrypoint } from "std:framework";

// ---- Discovery

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

// The project in a directory, which is its root submodule.
fun discover(directory: String): Project {
    return Project(submodule(Path.GetFullPath(directory)));
}

// ---- Compiling

// Compiles a project, giving what its module files emitted, in the order they did.
def Compiler {
    let project: Project;

    new(this, project: Project) { this.project = project; }

    fun compile(): List[Instance] { return compileProject(project); }
}

// A module file is given std:project/core as `core:project`.
@entrypoint
fun install() { core.Add("project", importers.Import("std:project/core").Unwrap()); }
