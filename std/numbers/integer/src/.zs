// std:numbers/integer, as a project: its module, built.
import { compile, emit } from "core:project";
import { Boolean } from "core:runtime";
import { FixedBitWidthInteger } from "std:numbers";
import { build } from "std:framework";
emit(build(compile("integer")));
