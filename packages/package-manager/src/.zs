// The package manager: what `pkg:` imports with. It is a project, built as an assembly
// with the Cecil platform, which the standard library imports; until it is built, the
// standard library compiles it from here instead, and it is the same either way.
//
// What its sources use is imported here, once.

import { compile, emit } from "core:project";
import { Object, String, Boolean, Int32, Type, List, Func, Path, Directory, File, Environment, Assembly, ilLoader } from "core:runtime";

import { AppContext, Version, AssemblyName, AssemblyLoadContext } from Object.Assembly.Location;

import { Instance } from "std:zsharp/hir";
import { Error, Result, IResult } from "std:zsharp/common";
import { Importer } from "core:zsharp";

emit(compile("Packages"));
