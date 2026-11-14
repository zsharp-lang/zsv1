// std:traits/dynamic, as a project: its module, built.
import { compile, emit } from "core:project";
import { String } from "core:runtime";
import { build } from "std:framework";
emit(build(compile("DynamicTraits")));
