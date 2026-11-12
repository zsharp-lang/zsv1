// std:functions/compiler, as a project: its module, built. Its sources use what CoreLib has
// of reflection, which binding a built function needs.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type } from "core:runtime";

import { MethodInfo } from Object.Assembly.Location;

emit(build(compile("compiler")));
