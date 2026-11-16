# From HIR to LIR

A definition's meaning is not in its node. The node says what it is; what using it does is
its compiler object's, its CO, which a loader makes of the node. And what runs is the LIR
the CO lowers to. This page takes one node through all of it.

```text
HIR node  →  loader  →  CO  →  lowering  →  LIR  →  a platform
```

## A node, a loader and a CO

This defines a node, `Number`, whose CO is a number of the value it holds: used as an
expression, it lowers to a constant.

```zs
import { print } from "std:io";
import { ct } from "core:zsharp";
import { Instance, Type as HIRType } from "pkg:zsharp/hir";
import { Get } from "pkg:zsharp/hir/code";
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { ICompileIRExpression } from "core:zsharp/m2l";
import { ITyped } from "core:zsharp/mir/ts";
import { Constant, Expression as LIRExpression } from "pkg:zsharp/lir";
import { ir, hirLoader, mappedLoader } from "core:compiler";
import { Class as DotNet_Class, public, explicitThis } from "pkg:clr/hir";
import { Object, Int32 } from "core:runtime";

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
def NumberObject of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
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
}

// The loader: a number's node to its CO.
module Loading {
    fun load(node: Number): IResult[CompilerObject, Error] {
        return Result[CompilerObject].Ok(NumberObject(node));
    }
}

mappedLoader.Instances.Register[Number](Loading.load);

print(ct(Get(Number(42))));
```

```output
42
```

## What happened

1. **HIR.** `Number(42)` is a node, and `Get(node)` is the code that refers to it: HIR,
   structure only. Nothing has looked at it yet.
2. **Loading.** `ct` runs the code. Running it loads it, and loading `Get(node)` asks the
   HIR loader for the node's CO. The loader finds what is registered for `Number`, as an
   instance, and calls it: `NumberObject(node)`.
3. **MIR.** The CO is what the compiler works with. It is `ITyped`, so the compiler knows
   its type, `Int32`, and anything around it can be checked against that. It is nothing
   else: it can't be called, and has no members, so code that tried would fail there,
   with an error of the operation.
4. **Lowering.** Running code means lowering it for the running platform. The CO is asked
   for its LIR as an expression, `CompileIRExpression(target)`, and gives a `Constant` of
   its type's LIR.
5. **The platform** runs the LIR, which gives 42.

The same node, loaded and lowered in a build for the Cecil platform, would be the same
constant in an assembly. Nothing in it is the runtime's: the CO lowered to core LIR, which
any platform reads.

## The three parts, apart

- The **node** is the part reflection and code generation see. A program can build it,
  read it, and carry it in a built library, without the CO.
- The **loader** is where the node is checked. A node can be built as anything, and an
  invalid one is fine until it is loaded, where the loader says why it can't be.
- The **CO** is the semantics: what each operation does with it, and what it lowers to.
  Another library could load the same node to a CO of its own, and give it another
  meaning.

[Compiler objects](compiler-objects.md) goes through what a CO can do, and [common
REIs](common-reis.md) through the operations it takes part in. What lowering gives, and
for which platform, is [custom platforms](../custom-platforms/overview.md)' subject.
