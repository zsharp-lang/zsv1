// Builds the package manager as an assembly, into packages/package-manager/build, with the Cecil platform,
// for the .NET that runs the toolchain, which imports it. Run it from the toolchain's
// directory. Until it is built, the standard library compiles the package manager from
// its source, which is what builds it the first time.
import "std:prelude";
import { print } from "std:io";
import { bundle } from "pkg:clr/platform";
import { Object } from "core:runtime";
import { Path, Directory } from Object.Assembly;
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { lower } from "std:modules/compiler";
import { Cecil } from "pkg:cecil";

let packages = Compiler(discover("packages/package-manager/src")).compile()[0] as Module;

let target = Cecil("Packages", null);
target.add(lower(packages, target));

Directory.CreateDirectory("packages/package-manager/build");
let written = Path.GetFullPath("packages/package-manager/build/Packages.dll");
target.write(written);
bundle(target.dependencies(), Path.GetDirectoryName(written));
print("wrote " + written);
