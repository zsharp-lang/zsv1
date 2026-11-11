// std:oop, as a project: its module, built. Its sources use what CoreLib has of
// reflection, which binding a built class needs.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type } from "core:runtime";

let objectType: Type = Object;
import { MethodInfo, FieldInfo, BindingFlags } from objectType.Assembly.Location;

emit(build(compile("oop")));
