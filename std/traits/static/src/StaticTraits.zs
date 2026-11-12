// std:traits/static — the static typing dispatchers of traits. Where a value's static
// type implements a trait, the value goes where the trait is expected, implicitly or by
// a cast, as the adapter of its implementation; that is decided as the code compiles.

import { Module as DotNet_Module } from "pkg:clr/hir";
import { Traits } from "std:traits";
import { Error, IResult } from "pkg:zsharp/core";
import { CompilerObject } from "pkg:zsharp/mir";
import { Argument, ImplicitCast, Cast } from "core:zsharp/mir/cg";
import { entrypoint } from "std:framework";
import { cg } from "core:compiler";

let previousImplicitCast: ImplicitCast;
let previousCast: Cast;

fun implicitCast(object: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
    let result = previousImplicitCast(object, type);
    if (result.IsOk) return result;

    let adapter = Traits.adapterFor(object, type);
    if (adapter == null) return result;

    return cg.Call(adapter, [Argument(object)]);
}

fun cast(object: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
    let adapter = Traits.adapterFor(object, type);
    if (adapter == null) return previousCast(object, type);

    return cg.Call(adapter, [Argument(object)]);
}

// Casts reach a trait's implementation by the static type, as importing it makes them.
@entrypoint
fun install() {
    previousImplicitCast = cg.ImplicitCast;
    cg.ImplicitCast = implicitCast;
    previousCast = cg.Cast;
    cg.Cast = cast;
}
