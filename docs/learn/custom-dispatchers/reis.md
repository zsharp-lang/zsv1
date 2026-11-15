# How REIs work

Some behavior belongs to no single object: a conversion every value of one kind has, or a
relation between two types neither knows about. A CO can't provide it, since it isn't the
CO's. What provides it is a dispatcher: code that changes how an operation is done, for
every object.

The operations a dispatcher changes are the compiler's REIs, replaceable entrypoints
interfaces. An REI is a set of operations whose signatures are fixed and whose behavior is
whatever is installed: `cg`, `ts`, `ir`, the HIR loader, matching and overloading are all
REIs. Each entrypoint is a delegate, a property that can be set, and setting one changes
how that operation is done everywhere.

## What an REI specifies

Only signatures. An REI says what an operation takes and gives, and never what it does:
`ImplicitCast(object, type)` gives the object converted to the type, or an error, and
nothing in the REI says how. What it does is the job of whatever is installed, which is
called a dispatcher.

A default entrypoint does nothing useful: it gives an *unsupported* error, a kind of error
that says the operation isn't handled, as opposed to one that says it was tried and
failed. That difference is how dispatchers compose.

## A dispatcher

A dispatcher replaces an entrypoint, and keeps what it replaced to delegate to. This is
`std:traits/static`, which makes a value go where a trait its type implements is
expected:

```zs fragment
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, IResult } from "pkg:zsharp/core";
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

@entrypoint
fun install() {
    previousImplicitCast = cg.ImplicitCast;
    cg.ImplicitCast = implicitCast;
    previousCast = cg.Cast;
    cg.Cast = cast;
}
```

- `install` runs when the library is imported, as its entry point. It keeps the current
  entrypoints and installs its own.
- Each of its own decides when to delegate. `implicitCast` asks what it replaced first,
  and handles only what that couldn't; `cast` handles traits first and gives everything
  else to what it replaced.

No order is enforced: a dispatcher can check its own case first and delegate otherwise,
or delegate first and handle what is left. What is enforced is only that the result is
what the signature says.

## Unsupported falls through, failure doesn't

A dispatcher that delegates should fall through on an *unsupported* error only. A real
failure, as a call whose arguments don't match, is the answer: replacing it with another
dispatcher's would hide why the operation failed. The compiler's own dispatchers follow
that rule, and a value's own error is never overwritten by a later dispatcher.

## Dispatchers stack

Each library that installs a dispatcher wraps what was installed before it, so the
dispatchers of an entrypoint are a stack, in the order the libraries were imported. The
prelude imports the trait dispatchers after the standard definitions, so traits wrap what
Z#'s own classes do. The CG dispatchers themselves are such a stack: `CG.Direct` asks
what the objects say of themselves, and `CG.Typed`, on top of it, what their types say.
