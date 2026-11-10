// Builds the project in src/ as a .NET assembly, with the Cecil platform: the platform
// is a package that is a project, `pkg:cecil`, compiled and loaded as it is imported.
// Shop is lowered for it, and it writes the assembly, for a version given, as net8.0, or
// for the .NET that runs it, given null. Then the assembly is imported, as any .dll is,
// and its module's functions called.
import "std:prelude";
import { print } from "std:io";
import { Path } from "core:runtime";
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { lower } from "std:modules/compiler";

import { Cecil } from "pkg:cecil";

let shop = Compiler(discover("examples/project/src")).compile()[1] as Module;

let target = Cecil("Shop", "net8.0");
target.add(lower(shop, target));

let written = Path.Combine(Path.GetTempPath(), "Shop.dll");
target.write(written);

import { describe } from written;
print(describe("apple"));
