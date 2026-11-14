// std:zsharp/clr, as a project: its module, built.
import { compile, emit } from "core:project";
import { Object, List, String, Int32, Boolean, Type as ClrType, ilLoader, predefined, SByte, Int16, Int64, Byte, UInt16, UInt32, UInt64, Single, Double } from "core:runtime";
import { Implementation } from "std:traits";
import { build } from "std:framework";
emit(build(compile("clr")));
