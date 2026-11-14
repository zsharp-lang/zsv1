// std:modules, as a project: its module, built.
import { compile, emit } from "core:project";
import { Class as Node } from "pkg:clr/hir";
import { build } from "std:framework";

emit(build(compile("modules")));
