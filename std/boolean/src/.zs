// std:boolean, as a project: its module, built.
import { compile, emit } from "core:project";
import { Truth } from "std:boolean/truth";
import { build } from "std:framework";
emit(build(compile("boolean")));
