// Not actual code, just concepts here


import { Compiler, discover } from "pkg:zsharp/compiler/project";


let project = discover("./src/"); // Ideally we'd use Directory.cwd() but we don't have that yet :D


let projectCompiler = Compiler(project);
let module: Module = projectCompiler.compile(); // HIR

import { load, lir } from "core:compiler";
import { Module as LIRModule } from "pkg:zsharp/core/lir";

let moduleLIR = ir.compile[LIRModule](load(module)?);

import { write } from "pkg:zsharp/platform/lir-file";

write(moduleLIR, "./build/module.lir");
