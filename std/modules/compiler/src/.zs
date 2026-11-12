// std:modules/compiler, as a project: its module, built. Its sources use what CoreLib has
// of reflection, which binding a built global needs.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type } from "core:runtime";

import { MemberInfo } from Object.Assembly;

emit(build(compile("compiler")));
