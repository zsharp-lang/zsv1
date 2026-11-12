// The package manager: what `pkg:` imports with. It is a project, built as an assembly
// with the Cecil platform, which the standard library imports; until it is built, the
// standard library compiles it from here instead, and it is the same either way.
//
// What its sources use is imported here, once.

import { compile, emit } from "core:project";
import { Object, String, Boolean, Int32, Type, List, Func, ilLoader } from "core:runtime";
import { Path, Directory, File, Environment, Assembly } from Object.Assembly;

import { AppContext, Version, AssemblyName, AssemblyLoadContext } from Object.Assembly;

import { Instance } from "pkg:zsharp/hir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { Importer } from "core:zsharp";

emit(compile("Packages"));
