// Builds the toolchain's Z# projects from their sources, as an installer would, and
// checks the build: `zs bootstrap.zs`.
//
// The standard library is its own seed: a library that is a project is built by the
// toolchain that loads its built assembly, as a compiler that compiles itself is. So each
// run, a generation, builds each library with the libraries it loads, and if any of them
// built something else than what was loaded, a new process, which loads what was built,
// is the next generation, until a generation builds what it loads. A library is built
// only by a generation that loads what it needs built, as its assembly refers to theirs.
//
// The Cecil platform and the package manager are built from their sources every
// generation, so nothing an older build lacks is needed: what earlier bootstraps built of
// them is removed first, before the prelude is imported, which then compiles the package
// manager from its source. Then the Cecil platform builds itself, first compiled from its
// source and then as what it built, until it builds what it was. Then the standard
// library is built with it, and the package manager is built with it once the standard
// library builds itself the same.

import { Object, String, Type, Int32, Boolean, List, Path, Directory, File, Environment, DotNet_Module } from "core:runtime";

let objectType: Type = Object;

// The toolchain's directory: the one the standard library is in, at or above where this
// runs from.
let root = Path.GetFullPath(".");
while (Directory.Exists(Path.Combine(root, "std")) == false) root = Path.GetDirectoryName(root);
Directory.SetCurrentDirectory(root);

let generation = 1;
let given = Environment.GetEnvironmentVariable("ZS_BOOTSTRAP_GENERATION");
if (given != null) generation = Int32.Parse(given);

// The standard library's projects, each after those it needs, and those of them it
// needs, separated by commas.
let libraries = List[String]();
let needs = List[String]();
libraries.Add("zsharp/lowering"); needs.Add("");
libraries.Add("zsharp/capabilities"); needs.Add("");
libraries.Add("traits"); needs.Add("");
libraries.Add("functions"); needs.Add("zsharp/lowering");
libraries.Add("traits/static"); needs.Add("traits");
libraries.Add("traits/dynamic"); needs.Add("traits");
libraries.Add("zsharp/clr"); needs.Add("traits,zsharp/capabilities");
libraries.Add("modules"); needs.Add("zsharp/lowering");
libraries.Add("oop"); needs.Add("functions,zsharp/capabilities,zsharp/lowering");
libraries.Add("framework"); needs.Add("functions,modules");
libraries.Add("dotnet"); needs.Add("");

// Which libraries this generation loads built: those built before it started.
let loaded = List[String]();
let index = 0;
while (index < libraries.Count) {
    let name = libraries[index];
    if (File.Exists(Path.Combine(root, "std", name, "build", Path.GetFileName(name) + ".dll"))) loaded.Add(name);
    index = index + 1;
}

let platformBuild = Path.Combine(root, "packages", "cecil", "build");
let managerBuild = Path.Combine(root, "pkg", "build");
if (Directory.Exists(platformBuild)) Directory.Delete(platformBuild, true);
if (Directory.Exists(managerBuild)) Directory.Delete(managerBuild, true);

import "std:prelude";
import { print } from "std:io";
import { documents } from "core:zsharp";
import { discover, Compiler } from "std:project";
import { Module, lower, failures } from "std:modules";

let objectAssembly = objectType.Assembly.Location;
import { Convert, AppContext } from objectAssembly;
import { Process, ProcessStartInfo } from Path.Combine(Path.GetDirectoryName(objectAssembly), "System.Diagnostics.Process.dll");

def Bootstrap of DotNet_Module {
    fun bytes(path: String): String {
        if (File.Exists(path) == false) return "";
        return Convert.ToBase64String(File.ReadAllBytes(path));
    }

    // Whether this generation loads what a library needs as it is built: each of them
    // built, and none built something else in this generation.
    fun ready(needed: String, libraries: List[String], loaded: List[String], changed: List[String]): Boolean {
        let at = 0;
        while (at < libraries.Count) {
            if (("," + needed + ",").Contains("," + libraries[at] + ",")) {
                if (loaded.Contains(libraries[at]) == false) return false;
                if (changed.Contains(libraries[at])) return false;
            }
            at = at + 1;
        }
        return true;
    }

    // Runs the next generation, a process that loads what this one built, and gives what
    // it exits with.
    fun next(root: String, generation: Int32): Int32 {
        let start = ProcessStartInfo();
        start.FileName = Environment.ProcessPath;
        if (Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "dotnet") start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "zs.dll"));
        start.ArgumentList.Add(Path.Combine(root, "bootstrap.zs"));
        start.UseShellExecute = false;
        start.Environment["ZS_BOOTSTRAP_GENERATION"] = "" + (generation + 1);

        let process = Process.Start(start);
        process.WaitForExit();
        return process.ExitCode;
    }
}

let platform = Path.Combine(platformBuild, "cecil.dll");

documents.Execute(Path.Combine(root, "packages", "cecil", "build.zs")).Unwrap();
let built = Bootstrap.bytes(platform);
let platformGeneration = 1;
let same = false;

while (same == false) {
    if (platformGeneration == 4) {
        print("The Cecil platform didn't build what it was in " + platformGeneration + " generations.");
        Environment.Exit(1);
    }
    documents.Execute(Path.Combine(root, "packages", "cecil", "build.zs")).Unwrap();
    platformGeneration = platformGeneration + 1;
    let again = Bootstrap.bytes(platform);
    same = again == built;
    built = again;
}

print("Generation " + generation + ": the Cecil platform builds itself, the same, from its generation " + (platformGeneration - 1) + ".");

import { Cecil } from "pkg:cecil";

def Libraries of DotNet_Module {
    // Builds a library of the standard library into its build folder, as an assembly
    // named for it, for the .NET that runs the toolchain, and whether it built what was
    // there.
    fun library(root: String, name: String): Boolean {
        let folder = Path.Combine(root, "std", name);
        let written = Path.Combine(folder, "build", Path.GetFileName(name) + ".dll");
        let before = Bootstrap.bytes(written);

        let emitted = Compiler(discover(Path.Combine(folder, "src"))).compile();
        let target = Cecil("std." + name.Replace("/", "."), null);
        let at = 0;
        while (at < emitted.Count) {
            let node = emitted[at] as Module;
            target.add(lower(node, target));

            // A library is all of its module, so what lowering left out fails the build.
            let failed = failures(node);
            if (failed.Count > 0) {
                print("std:" + name + " can't be built: " + failed[0]);
                Environment.Exit(1);
            }
            at = at + 1;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(written));
        target.write(written);
        return Bootstrap.bytes(written) == before;
    }
}

let settled = true;
let changed = List[String]();
index = 0;
while (index < libraries.Count) {
    if (Bootstrap.ready(needs[index], libraries, loaded, changed)) {
        print("Generation " + generation + ": building std:" + libraries[index] + ".");
        if (Libraries.library(root, libraries[index]) == false) {
            settled = false;
            changed.Add(libraries[index]);
        }
    } else settled = false;
    index = index + 1;
}

if (settled == false) {
    if (generation == 4) {
        print("The standard library didn't build what it loads in " + generation + " generations.");
        Environment.Exit(1);
    }
    print("Generation " + generation + ": the standard library built something else than it loads.");
    Environment.Exit(Bootstrap.next(root, generation));
}

print("Generation " + generation + ": the standard library builds itself, the same.");

documents.Execute(Path.Combine(root, "pkg", "build.zs")).Unwrap();
print("Bootstrapped.");
