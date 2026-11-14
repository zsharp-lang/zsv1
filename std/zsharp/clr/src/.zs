// std:zsharp/clr, as a project: its module, built.
import { compile, emit } from "core:project";
import { Implementation } from "std:traits";
import { build } from "std:framework";
emit(build(compile("clr")));
