// std:numbers, as a project: its module, built.
import { compile, emit } from "core:project";
import { String, Int32, Boolean } from "core:runtime";
import { Class as Node } from "pkg:clr/hir";
import { build } from "std:framework";
emit(build(compile("numbers")));
