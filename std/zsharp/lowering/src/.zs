// std:zsharp/lowering, as a project: its module, built.
import { compile, emit } from "core:project";
import { Interface as DotNet_Interface, Class as DotNet_Class } from "pkg:clr/hir";
import { build } from "std:framework";
emit(build(compile("lowering")));
