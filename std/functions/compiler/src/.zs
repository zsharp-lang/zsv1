// std:functions/compiler, as a project: its module, built. Its sources use what CoreLib has
// of reflection, which binding a built function needs.
import { compile, emit } from "core:project";
import { Class as DotNet_Class } from "pkg:clr/hir";
import { build } from "std:framework";
import { Object, Type, String, Boolean, Int32, List, Dictionary, ilLoader } from "core:runtime";
import { MethodInfo } from Object.Assembly;

emit(build(compile("compiler")));
