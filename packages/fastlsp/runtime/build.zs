// Builds pkg:fastlsp/runtime as an assembly, into packages/fastlsp/runtime/build, with the Cecil
// platform. Run it from the toolchain's directory.
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

let project = Compiler(discover("packages/fastlsp/runtime/src")).compile()[0] as Module;

let target = Cecil("fastlsp.runtime", null);
let described = encode(project);
target.add(lower(project, target));
target.embed("ZSharp.Reflection", described, "std:encoding");

Directory.CreateDirectory("packages/fastlsp/runtime/build");
let written = Path.GetFullPath("packages/fastlsp/runtime/build/runtime.dll");
target.write(written);
bundle(target.dependencies(), Path.GetDirectoryName(written));
print("wrote " + written);
