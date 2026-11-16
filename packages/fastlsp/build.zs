// Builds pkg:fastlsp as an assembly, into packages/fastlsp/build, with the Cecil platform.
// Once built, `pkg:fastlsp` is the assembly. Run it from the toolchain's directory.
import "std:prelude";
import { print } from "std:io";
import { bundle } from "pkg:clr/platform";
import { Object } from "core:runtime";
import { Path, Directory } from Object.Assembly;
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { lower } from "std:modules/compiler";
import { Cecil } from "pkg:cecil";
import { encode } from "std:encoding";

let project = Compiler(discover("packages/fastlsp/src")).compile()[0] as Module;

let target = Cecil("fastlsp", null);
let described = encode(project);
target.add(lower(project, target));
target.embed("ZSharp.Reflection", described, "std:encoding");

Directory.CreateDirectory("packages/fastlsp/build");
let written = Path.GetFullPath("packages/fastlsp/build/fastlsp.dll");
target.write(written);
bundle(target.dependencies(), Path.GetDirectoryName(written));
print("wrote " + written);
