// std:project, as a project: its module, built.
import { compile, emit } from "core:project";
import { Object, List, String } from "core:runtime";
import { build } from "std:framework";
emit(build(compile("project")));
