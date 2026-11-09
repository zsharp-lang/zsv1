// Builds the toolchain's Z# projects from their sources, as an installer would, and
// checks the build: `zs bootstrap.zs`.
//
// It starts from the sources, so nothing an older build lacks is needed: what earlier
// bootstraps built is removed first, before the prelude is imported, which then compiles
// the package manager from its source. Then the Cecil platform builds itself, first
// compiled from its source and then as what it built, until it builds what it was, which
// it must within a few generations, as builds are deterministic. Then the package manager
// is built with it.

import { Object, String, Type, Int32, Path, Directory, File, Environment } from "core:runtime";

let objectType: Type = Object;

// The toolchain's directory: the one the standard library is in, at or above where this
// runs from.
let root = Path.GetFullPath(".");
while (Directory.Exists(Path.Combine(root, "std")) == false) root = Path.GetDirectoryName(root);
Directory.SetCurrentDirectory(root);

let platformBuild = Path.Combine(root, "packages", "cecil", "build");
let managerBuild = Path.Combine(root, "pkg", "build");
if (Directory.Exists(platformBuild)) Directory.Delete(platformBuild, true);
if (Directory.Exists(managerBuild)) Directory.Delete(managerBuild, true);

import "std:prelude";
import { print } from "std:io";
import { documents } from "core:zsharp";

let objectAssembly = objectType.Assembly.Location;
import { Convert } from objectAssembly;

let platform = Path.Combine(platformBuild, "cecil.dll");

documents.Execute(Path.Combine(root, "packages", "cecil", "build.zs")).Unwrap();
let built = Convert.ToBase64String(File.ReadAllBytes(platform));
let generation = 1;
let same = false;

while (same == false) {
    if (generation == 4) {
        print("The Cecil platform didn't build what it was in " + generation + " generations.");
        Environment.Exit(1);
    }
    documents.Execute(Path.Combine(root, "packages", "cecil", "build.zs")).Unwrap();
    generation = generation + 1;
    let again = Convert.ToBase64String(File.ReadAllBytes(platform));
    same = again == built;
    built = again;
}

print("The Cecil platform builds itself, the same, from generation " + (generation - 1) + ".");

documents.Execute(Path.Combine(root, "pkg", "build.zs")).Unwrap();
print("Bootstrapped.");
