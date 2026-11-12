// pkg:clr/platform, as a project: its module. Its sources use what CoreLib has that
// the toolchain's modules don't.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type } from "core:runtime";

import { Convert } from Object.Assembly;

emit(build(compile("platform")));
