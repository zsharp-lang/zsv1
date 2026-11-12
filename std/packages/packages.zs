// std:packages — what `pkg:` has beyond the toolchain's own packages, from the package
// manager. The package manager is a project of the toolchain's, in
// `packages/package-manager`: its assembly, once it is built into its `build`, or else
// the project, compiled now. Either is `core:packages`, whose roots code can add to, and
// a package that is a project is compiled by the project compiler. An assembly the
// running .NET can't find, as one a package's assembly refers to, it asks the package
// manager for.

import { Object, String, Type, ilLoader } from "core:runtime";
import { Path, File, Directory, Assembly } from Object.Assembly;
import { Instance } from "pkg:zsharp/hir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { core, packages, Importer } from "core:zsharp";
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { enter } from "std:modules/compiler";

import { AppContext, AssemblyLoadContext } from Object.Assembly;

module Bootstrap {
    // Where the toolchain is: the directory the standard library is in.
    fun toolchain(): String {
        let directory = AppContext.BaseDirectory;
        while (directory != null) {
            if (Directory.Exists(Path.Combine(directory, "std"))) return directory;
            directory = Path.GetDirectoryName(directory);
        }
        return Directory.GetCurrentDirectory();
    }

    // The assembly is loaded from its bytes, so building it again, while it is in use,
    // can write its file.
    fun manager(): Instance {
        let built = Path.Combine(toolchain(), "packages", "package-manager", "build", "Packages.dll");
        if (File.Exists(built)) return ilLoader.Load(Assembly.Load(File.ReadAllBytes(built)).ManifestModule);
        return Compiler(discover(Path.Combine(toolchain(), "packages", "package-manager", "src"))).compile()[0];
    }

    // A package that is a project is what compiling it emits first, imported as a built
    // one is: a module is entered.
    fun project(directory: String): IResult[Instance, Error] {
        let emitted = Compiler(discover(directory)).compile();
        if (emitted.Count == 0) return Result[Instance].Error("The project " + directory + " emits nothing.");
        if (emitted[0] is let built: Module) enter(built);
        return Result[Instance].Ok(emitted[0]);
    }
}

core.Add("packages", Bootstrap.manager());

import { load, compileProjectsWith, resolveAssembly } from "core:packages";
compileProjectsWith(Bootstrap.project);
packages.FallBackTo(load);
AssemblyLoadContext.Default.Resolving.add(resolveAssembly);
