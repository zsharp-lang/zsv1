# Compiler objects

The previous page's `NumberObject` only had a type and a way to lower. A CO for a real
definition does more: it can be called, have members, convert, be assigned. This page
goes through how a CO says which of those it does.

A CO is what the compiler works with. It is opaque: the one thing every CO is, is a
`CompilerObject`, and everything else it can do, it does by implementing a capability, an
interface of the operation it takes part in.

## Giving the number a member

`NumberObject` can be used as an expression because it is `ICompileIRExpression`, and has
a type because it is `ITyped`. To give it a member, it implements one more capability,
`ICTGetMember[String]`, members by name. Here a number has `doubled`, another number:

```zs
import { print } from "std:io";
import { ct } from "core:zsharp";
import { Instance, Type as HIRType } from "pkg:zsharp/hir";
import { Get, NamedMember } from "pkg:zsharp/hir/code";
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { ICompileIRExpression } from "core:zsharp/m2l";
import { ITyped } from "core:zsharp/mir/ts";
import { ICTGetMember } from "core:zsharp/mir/cg/direct";
import { Constant, Expression as LIRExpression } from "pkg:zsharp/lir";
import { ir, hirLoader, mappedLoader } from "core:compiler";
import { Class as DotNet_Class, public, explicitThis } from "pkg:clr/hir";
import { Object, Int32, String } from "core:runtime";

// The node: a number, as structure.
@public
def Number of DotNet_Class : Instance {
    @public
    let Value: Int32;

    @public @explicitThis
    new(this, value: Int32) { this.Value = value; }
}

// The CO: what using a number does.
@public
def NumberObject of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped, ICTGetMember[String] {
    @public
    let node: Number;

    @public @explicitThis
    new(this, node: Number) { this.node = node; }

    @public @explicitThis
    get Type(this): CompilerObject { return hirLoader.LoadType(Int32 as HIRType).Unwrap(); }

    @public @explicitThis
    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let type = ir.CompileType(this.Type, target);
        if (type.IsError) return Result[LIRExpression].Error(type.UnwrapError());
        return Result[LIRExpression].Ok(Constant(type.Unwrap(), this.node.Value));
    }

    @public @explicitThis
    fun Member(this, name: String): IResult[CompilerObject, Error] {
        if (name == "doubled") return Result[CompilerObject].Ok(NumberObject(Number(this.node.Value * 2)));
        return Result[CompilerObject].Error("A number has no member named " + name + ".");
    }
}

// The loader: a number's node to its CO.
module Loading {
    fun load(node: Number): IResult[CompilerObject, Error] {
        return Result[CompilerObject].Ok(NumberObject(node));
    }
}

mappedLoader.Instances.Register[Number](Loading.load);

let doubled = NamedMember();
doubled.Target = Get(Number(21));
doubled.Name = "doubled";
print(ct(doubled));
```

```output
42
```

`doubled.Target` is the number, and `doubled.Name` the member asked for. Running the code
asks the number's CO for the member, `Member("doubled")`, and what it gives, another
`NumberObject`, is what lowers to `42`.

## Each operation gives a result

Asked for a member it doesn't have, the CO says so:

```text
error: A number has no member named halved.
```

An operation gives a result: what it made, or why it can't. A CO that is asked something
it supports, with what it can't take, gives an error, as `Member` did for `halved`. So a
CO checks what it is given where it is given it, and code that uses a CO never asks
beforehand whether something will work: it does it, and handles the error.

What a CO doesn't implement at all is *unsupported*, an error of its own kind. A number
can't be called, since it isn't `ICTCallable`, so calling one is unsupported, which lets a
[dispatcher](../custom-dispatchers/reis.md) handle it instead, if one does.

Lowering is the same. A CO lowered for a target with something it can't lower, as a
structure the target has no form of, gives an error. What it doesn't catch, the platform
does.

## The capabilities there are

`ICTGetMember` is one of many. The common ones:

| To be | Implement | From |
|---|---|---|
| called | `ICTCallable` | `core:zsharp/mir/cg/direct` |
| read as a value | `ICTGet` | `core:zsharp/mir/cg/direct` |
| assigned | `ICTSet` | `core:zsharp/mir/cg/direct` |
| a member's owner | `ICTGetMember`, `ICTSetMember` | `core:zsharp/mir/cg/direct` |
| indexed | `ICTGetIndex`, `ICTSetIndex` | `core:zsharp/mir/cg/direct` |
| cast, or converted implicitly | `ICTCastTo`, `ICTImplicitCastTo`, and from the type's side, `ICTCastFrom`, `ICTImplicitCastFrom` | `core:zsharp/mir/cg/direct` |
| typed | `ITyped` | `core:zsharp/mir/ts` |
| a type others go to, or go from | `IAssignableTo`, `IAssignableFrom` | `core:zsharp/mir/ts` |
| lowered | `ICompileIRExpression`, `ICompileIRStatement`, `ICompileIRType`, `ICompileIRReference`, `ICompileIRDefinitionIn` | `core:zsharp/m2l` |

`CT` is for what is decided as code is compiled. What is decided by a value's type as the
code runs is `CG.Typed`'s, with `RT` capabilities of a type, as `IRTGetMember`, which a
type implements for its values.

## What a CO may hold

A CO may hold anything: its node, other COs, objects of other layers, live objects of the
process. What it may not do is depend on another layer in how it interacts with other COs.
A class's CO asks a parameter what it is through the operations, as what a parameter is
referred to as and as its type, not as a parameter of `std:functions`'.

The exception is a CO that only exists for one layer, as a CO of the runtime's types does:
it may use the runtime to decide how it behaves and what it lowers to, since it is never
anything else.

## Rules of thumb

- **Never inspect a CO.** Apply an operation and handle the result. Asking what a CO is,
  or what it implements, restricts what can take part to what you knew of.
- **Never compare COs by identity.** Two objects can stand for one thing. Ask the type
  system, as `ts.IsAssignableTo`, and the COs concerned answer.
- **Forward with `Is`.** A CO that stands for another, as a wrapper does, forwards its
  capabilities by overriding `Is<T>`, rather than by implementing every interface.
- **Lower to core LIR.** A CO that lowers to LIR every platform reads works on every
  platform. One that needs more asks the target for a capability; one that is only ever
  one platform's may lower to that platform's dialect. [Custom
  platforms](../custom-platforms/capabilities.md) says how.
