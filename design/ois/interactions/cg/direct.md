# CG Implementation - Direct

The `Direct` implementation is one of a number of implementations of the CG API.

For each interaction `X`, the `Direct` library defines an `IDirect{X}` interface.
For example, for the `Call` interaction, it defines an `IDirectCallable` interface.

For each of the interactions, the implementation should do something like this:
```cs
if (target.Is<IDirect{X}>(out var capability))
    result = capability.{X}({Args...});
```

In other words, this implementation forwards the interaction to the target object
*directly*, hence its name.


For example, a `Function` is directly callable.
