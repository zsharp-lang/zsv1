// Mono.Cecil from Z#: a .dll is imported by its path, and this one is found in the NuGet
// cache, until a package manager resolves it. It writes an assembly, then imports it and
// calls what it wrote.
import { print } from "std:io";
import { Object, String } from "core:runtime";
import { Path, Environment } from Object.Assembly;

let cecil = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget/packages/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll");
import { AssemblyDefinition, AssemblyNameDefinition, ModuleKind, TypeDefinition, TypeAttributes, MethodDefinition, MethodAttributes, OpCodes } from cecil;

let assembly = AssemblyDefinition.CreateAssembly(AssemblyNameDefinition("Answers", null), "Answers", ModuleKind.Dll);
let main = assembly.MainModule;

let answers = TypeDefinition("Answers", "Answers", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, main.TypeSystem.Object);
main.Types.Add(answers);

let answer = MethodDefinition("Answer", MethodAttributes.Public | MethodAttributes.Static, main.TypeSystem.Int32);
answers.Methods.Add(answer);

let body = answer.Body.GetILProcessor();
body.Emit(OpCodes.Ldc_I4, 42);
body.Emit(OpCodes.Ret);

let written = Path.Combine(Path.GetTempPath(), "Answers.dll");
assembly.Write(written);

import { Answers } from written;
print("the answer is " + Answers.Answer());
