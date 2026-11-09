// Builds the package manager as an assembly, into pkg/build, with the Cecil platform,
// for the .NET that runs the toolchain, which imports it. Run it from the toolchain's
// directory. Until it is built, the standard library compiles the package manager from
// its source, which is what builds it the first time.
import "std:prelude";
import { print } from "std:io";
import { Path, Directory } from "core:runtime";
import { discover, Compiler } from "std:project";
import { Module, lower } from "std:modules";
import { Cecil } from "pkg:cecil";

let packages = Compiler(discover("pkg/src")).compile()[0] as Module;

let target = Cecil("Packages", null);
target.add(lower(packages, target));

Directory.CreateDirectory("pkg/build");
let written = Path.GetFullPath("pkg/build/Packages.dll");
target.write(written);
print("wrote " + written);
