// std:zsharp/capabilities, as a project: its module, built.
import { compile, emit } from "core:project";
import { Int32, Boolean } from "core:runtime";
import { Interface as DotNet_Interface } from "pkg:clr/hir";
import { build } from "std:framework";
emit(build(compile("capabilities")));
