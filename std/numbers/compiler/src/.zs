// std:numbers/compiler, as a project: its module, built.
import { compile, emit } from "core:project";
import { Class as DotNet_Class } from "pkg:clr/hir";
import { build } from "std:framework";
emit(build(compile("compiler")));
