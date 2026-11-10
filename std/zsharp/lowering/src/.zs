// std:zsharp/lowering, as a project: its module, built.
import { compile, emit } from "core:project";
import { build } from "std:framework";
emit(build(compile("lowering")));
