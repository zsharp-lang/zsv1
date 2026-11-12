// std:traits/dynamic — the dynamic typing dispatchers of traits. Matching a value
// against a trait, or casting it to one, looks an implementation up by the type the value
// has as the code runs, so a value whose static type says nothing, as an Object, is
// still matched by what its type implements.

import { Traits, Lookup } from "std:traits";
import { String, DotNet_Module } from "core:runtime";
import { Error, Result, IResult } from "std:zsharp/common";
import { CompilerObject } from "std:zsharp/mir";
import { Argument, Cast } from "std:zsharp/compiler/cg";
import { PatternMatch } from "std:zsharp/compiler/matching";
import { entrypoint } from "std:framework";
import { cg, matching, hirLoader } from "core:compiler";
import { Instance } from "std:zsharp/hir";

let previousMatch: PatternMatch;
let previousCast: Cast;
let lookup: CompilerObject;

fun call(name: String, object: CompilerObject, trait: CompilerObject): IResult[CompilerObject, Error] {
    let member = cg.Member(lookup, name);
    if (member.IsError) return member;

    return cg.Call(member.Unwrap(), [Argument(object), Argument(trait)]);
}

fun match(object: CompilerObject, pattern: CompilerObject): IResult[CompilerObject, Error] {
    if (Traits.isTrait(pattern)) return call("has", object, pattern);

    return previousMatch(object, pattern);
}

// The value as the trait, its implementation's adapter if it isn't one already, which
// a cast then makes the trait's type.
fun cast(object: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
    if (Traits.isTrait(type)) {
        let adapted = call("adapt", object, type);
        if (adapted.IsError) return adapted;

        return previousCast(adapted.Unwrap(), type);
    }

    return previousCast(object, type);
}

// Matches and casts look a trait's implementation up as the code runs, as importing it
// makes them.
@entrypoint
fun install() {
    lookup = hirLoader.LoadInstance(Lookup as Instance).Unwrap();
    previousMatch = matching.Pattern;
    matching.Pattern = match;
    previousCast = cg.Cast;
    cg.Cast = cast;
}
