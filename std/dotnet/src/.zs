// std:dotnet, as a project: its module, built. Its sources use what CoreLib has that
// the toolchain's modules don't.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type } from "core:runtime";

let objectType: Type = Object;
import { Convert } from objectType.Assembly.Location;

emit(build(compile("dotnet")));
