// std:traits/dynamic — the dynamic typing dispatchers of traits. Matching a value
// against a trait, or casting it to one, looks an implementation up by the type the value
// has as the code runs, so a value whose static type says nothing, as an Object, is
// still matched by what its type implements.

import { Traits, Lookup } from "std:traits";
import { String, DotNet_Module } from "core:runtime";
import { Error, Result, IResult } from "std:zsharp/common";
import { CompilerObject } from "std:zsharp/mir";
import { Argument, Match, Cast, MatchResult } from "std:zsharp/compiler/cg";
import { cg, hirLoader } from "core:compiler";
import { Instance } from "std:zsharp/hir";

def DynamicTraits of DotNet_Module {
    let previousMatch: Match;
    let previousCast: Cast;
    let lookup: CompilerObject;

    fun call(name: String, object: CompilerObject, trait: CompilerObject): IResult[CompilerObject, Error] {
        let member = cg.Member(lookup, name);
        if (member.IsError) return member;

        return cg.Call(member.Unwrap(), [Argument(object), Argument(trait)]);
    }

    fun match(object: CompilerObject, pattern: CompilerObject): IResult[MatchResult, Error] {
        if (Traits.isTrait(pattern)) {
            let has = call("has", object, pattern);
            if (has.IsError) return Result[MatchResult].Error(has.UnwrapError());

            let result = MatchResult();
            result.Match = has.Unwrap();
            return Result[MatchResult].Ok(result);
        }

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
}

DynamicTraits.lookup = hirLoader.LoadInstance(Lookup as Instance).Unwrap();
DynamicTraits.previousMatch = cg.Match;
cg.Match = DynamicTraits.match;
DynamicTraits.previousCast = cg.Cast;
cg.Cast = DynamicTraits.cast;
