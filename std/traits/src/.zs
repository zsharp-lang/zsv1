// std:traits, as a project: its module, built.
import { compile, emit } from "core:project";
import { String, Object, Boolean, Int32, List, Type as ClrType, ilLoader } from "core:runtime";
import { Class as DotNet_Class } from "pkg:clr/hir";
import { build } from "std:framework";
emit(build(compile("traits")));
