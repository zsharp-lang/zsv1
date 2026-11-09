// Builds the Cecil platform as an assembly, into packages/cecil/build, with the Cecil
// platform: the one `pkg:cecil` gives, which is the assembly this built before, or else
// the project, compiled now. Once built, `pkg:cecil` is the assembly. It is written for
// the .NET that runs the toolchain, which loads it. Run it from the toolchain's directory.
import "std:prelude";
import { print } from "std:io";
import { bundle } from "std:dotnet";
import { Path, Directory } from "core:runtime";
import { discover, Compiler } from "std:project";
import { Module, lower } from "std:modules";
import { Cecil } from "pkg:cecil";

let platform = Compiler(discover("packages/cecil/src")).compile()[0] as Module;

let target = Cecil("cecil", null);
target.add(lower(platform, target));

Directory.CreateDirectory("packages/cecil/build");
let written = Path.GetFullPath("packages/cecil/build/cecil.dll");
target.write(written);
bundle(target.dependencies(), Path.GetDirectoryName(written));
print("wrote " + written);
