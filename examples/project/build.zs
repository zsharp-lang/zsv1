// Builds the project in src/ with std:project, and uses what it emitted: each module
// is HIR, which this loads and calls into.
import "std:prelude";
import { print } from "std:io";
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { Get, NamedMember, StringLiteral } from "std:zsharp/hir/code";
import { Bodies } from "std:zsharp/lang";
import { ct } from "core:zsharp";
import { String, Object } from "core:runtime";

let project = discover("examples/project/src");
let emitted = Compiler(project).compile();

fun call(target: Module, name: String, argument: String): Object {
    let member = NamedMember();
    member.Target = Get(target);
    member.Name = name;
    return ct(Bodies.Call(member, StringLiteral(argument)));
}

let index = 0;
while (index < emitted.Count) {
    print("emitted " + (emitted[index] as Module).Name);
    index = index + 1;
}

print(call(emitted[0] as Module, "hello", "world"));
print(call(emitted[1] as Module, "describe", "apple"));
print(call(emitted[1] as Module, "describe", "pear"));
