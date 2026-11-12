// std:types — the types of values, as the compiler knows them.
//
// `Type[T]` is the type of T's value: for a definition, the type of what getting its
// value gives, as its type object. It is written as code that runs, and builds the HIR
// that describes it, which is reflection; the type itself is what the compiler finds
// once that HIR is loaded. So it takes nothing of a meta-type beyond the value its CO
// gives: a definition whose CO gives none, as a module's, has no type.

import { Class as DotNet_Class } from "pkg:clr/hir";
import { Instance, Type as HIRType } from "pkg:zsharp/hir";
import { Get, GetValue, TypeOf } from "pkg:zsharp/hir/code";
import { Operator } from "pkg:zsharp/lang/hir";
import { class, Class } from "std:oop";
import { operator } from "core:zsharp";

def Type of Class {
    @class @operator(Operator.Index) fun(type: HIRType): HIRType {
        let value = GetValue();
        value.Of = Get(type as Instance);

        let typeOf = TypeOf();
        typeOf.Value = value;
        return typeOf;
    }
}
