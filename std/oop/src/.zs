// std:oop, as a project: its module, built. Its sources use what CoreLib has of
// reflection, which binding a built class needs.
import { compile, emit } from "core:project";
import { Class as Node } from "pkg:clr/hir";
import { build } from "std:framework";
import { Object, Type, String, Boolean, List } from "core:runtime";
import { MethodInfo, FieldInfo, BindingFlags } from Object.Assembly;

emit(build(compile("oop")));
