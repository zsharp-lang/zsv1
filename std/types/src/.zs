// std:types, as a project: its module, built.
import { compile, emit } from "core:project";
import { Class } from "std:oop";
import { build } from "std:framework";
emit(build(compile("types")));
