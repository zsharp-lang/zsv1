// std:encoding, as a project: its module, built. Its sources use what CoreLib has of
// reflection and binary streams, and the CLR platforms' half of reflection, which the
// toolchain's attributes assembly has.
import { compile, emit } from "core:project";
import { build } from "std:framework";
import { Object, Type, Path } from "core:runtime";

import {
    BinaryWriter, BinaryReader, MemoryStream, BindingFlags, FieldInfo, MethodInfo, ConstructorInfo, MemberInfo, ParameterInfo,
    RuntimeHelpers, Activator, Array, Enum, Convert, Delegate, AppContext, Module as ReflectedModule
} from Object.Assembly;
import { Metadata } from Path.Combine(AppContext.BaseDirectory, "ZSharp.Importer.ILLoader.Attributes.dll");

emit(build(compile("encoding")));
