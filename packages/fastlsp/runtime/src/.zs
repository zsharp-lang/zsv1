// fastlsp's runtime: what a language server built with it runs against. JSON, the server
// and its lifecycle, the transports, and the readers and writers' helpers that the code
// `pkg:fastlsp`'s `build` generates calls. It has no building code, so a server needs
// this and not the generator, and the generated code refers to it as to any library.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope.

import { compile, emit } from "core:project";
import { Object, String, Int32, Int64, Boolean, Double, Byte, List, Dictionary, Func, Nullable } from "core:runtime";
import { Interface } from "std:oop";
import { ErrorCodes } from "pkg:lsp";
import { IDictionary, Unsafe } from Object.Assembly;
import { Path, Convert, Math, StringBuilder, CultureInfo, Stream, Encoding } from Object.Assembly;
import { Console } from Path.Combine(Path.GetDirectoryName(Object.Assembly.Location), "System.Console.dll");

emit(compile("FastLSPRuntime"));
