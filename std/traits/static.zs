// std:traits/static — the static typing dispatchers of traits. Where a value's static
// type implements a trait, the value goes where the trait is expected, implicitly or by
// a cast, as the adapter of its implementation; that is decided as the code compiles.

import { DotNet_Module } from "core:runtime";
import { Traits } from "std:traits";
import { Error, IResult } from "std:zsharp/common";
import { CompilerObject } from "std:zsharp/mir";
import { Argument, ImplicitCast, Cast } from "std:zsharp/compiler/cg";
import { cg } from "core:compiler";

def StaticTraits of DotNet_Module {
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
}

StaticTraits.previousImplicitCast = cg.ImplicitCast;
cg.ImplicitCast = StaticTraits.implicitCast;
StaticTraits.previousCast = cg.Cast;
cg.Cast = StaticTraits.cast;
