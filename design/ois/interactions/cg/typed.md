# CG Implementation - Typed

The `Typed` implementation is one of a number of implementations of the CG API.

For each interaction `X`, the `Typed` library defines an `ITyped{X}` interface.
For example, for the `Call` interaction, it defines an `ITypedCallable` interface.

For each of the interactions, the implementation should do something like this:
```cs
if (TS.IsTyped(target, out var type) && type.Is<ITyped{X}>(out var capability))
    result = capability.{X}({Args...});
```

As we can see, this implementation also depends on another API, the `TS` API.

In other words, this implementation forwards the interaction to the *target's type*
object, hence its name.


For example, a `LocalVariable` is not directly callable, but its type *might* be.
