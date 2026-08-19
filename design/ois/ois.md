# OIS - Object Interaction System


The OIS system defines ONLY 3 types:
* CompilerObject (CO)
* Error
* Interaction Result

The idea behind this is that every interaction between 2 COs can either produce another
CO, or it can error.

More specifically, the `CompilerObject` type should have the exact shape of:
```cs
interface CompilerObject {
    public bool Is<T>([NotNullWhen(true)] out T? capability) 
        where T : class
        => (capability = this as T) is not null;
}
```

The `Error` type is the root node in the error type hierarchy.

The Interaction Result type can be implemented as a standalone type, or we can define
a generic `Either<A, B>` type, build a `Result<T, E>` on top of it, and build the
Interaction Result on top of `Result<T, E>`. If we go this way, both `Either` and `Result`
should be defined in a separate, shared utility library.


The OIS does not define actual interactions itself. It only gives the tools that allows
us to define interactions such that they're composable.

## Helpers

For simplicity, we can also define an `ErrorMessage` type deriving from `Error` and add a
static `InteractionResult Error(string message)` in `InteractionResult` so that we can easily
create error messages without explicitly constructing a new `ErrorMessage`. The important
thing to keep in mind is that this `ErrorMessage` type must not be used to define an API.

Same goes for an `ErrorList` type, which basically means it holds multiple errors.
A static `InteractionResult Error(params IEnumerable<Error> errors)` should be good enough.
