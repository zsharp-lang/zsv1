# TS API - Type-System API


The `TS` API defines interactions to work with types. More specifically, it defines a single
interaction and another helper method:
* TypeOf
* IsTyped - helper method. not overridable.


## Interaction Shape

* `TypeOf(object: CompilerObject): InteractionResult`

The helper method is implemented like this:
```cs
public bool IsTyped(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? type)
{
    var result = TypeOf(@object);
    if (result.IsError) return (type = null) is not null;

    return (type = result.Unwrap()) is not null;
}
```

Of course, the implementation can be different, but the idea is the same.
Doing it like this means that even when overriding the `TypeOf` API, every other consumer
of this helper function still stays valid and up-to-date.
