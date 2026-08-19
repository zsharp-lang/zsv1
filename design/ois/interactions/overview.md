# Overview

As we've already seen, the OIS system gives us tools to define interactions in a composable way.
That is, we can compose interactions together. For example, if we have interaction `A` and `B`,
`A` produces a result, and if succeeded, we can proceed to interaction `B`.

However, we also know that OIS does not define interactions itself.

This document describes a general approach when we define interactions.

The general rule of thumb is the following:
> An interaction input must only contain types that are trivially convertible from `CompilerObject`.
> An interaction output must only contain types that are trivially convertible to `CompilerObject`.

For example, the an `InteractionResult Eat(CompilerObject person, CompilerObject food)` interaction
indeed follows the rule. The output is a result type, as it should always be for failable interactions,
and it directly wraps a `CompilerObject`.

If we have a type `NamedCompilerObject` that wraps a `CompilerObject` and it trivially constructible,
e.g. it's possible to do `new NamedCompilerObject(someCO)`, it can also be used as an input for an
interaction.

For output types, we can use result of subtypes of `CompilerObject`, since they are always trivialy
convertible to `CompilerObject`. However, it is important to keep in mind that using any type other
than `CompilerObject` should only be done when it is a *requirement* of the interaction.

## Interactions as API

When we define an interaction, we generally want to define it as an API rather than an implementation.
The reason for that is because given a `CompilerObject` we can't generally know what it can and cannot
do.

So what we do is define an API / an interface, and allow other code to override the actual implementation.

### Example

I'll now give an example that deliberately has nothing to do with programming languages.
Now, suppose that we defined an API group for a `Player`, named `PlayerAPI`:
* `UseItem(player: CompilerObject, item: CompilerObject): InteractionResult`
* `WalkTo(player: CompilerObject, target: CompilerObject): InteractionResult`
* `Interact(player: CompilerObject, target: CompilerObject): InteractionResult`

Each of these functions is accessible through a singleton `PlayerAPI` object.
That is, somewhere we have a global `static var playerAPI = new PlayerAPI` and when we want to use the
API we do `playerAPI.X` where `X` is the interaction we want.

As we discussed, the `PlayerAPI` doesn't define how the interaction is actually done. However, it can be
implemented in such a way that all interactions always result with an error saying the interaction is not
implemented. This approach has a real advantage, if done correctly.

To do it properly, the API itself should be defined as a C# `interface` without default implementation.
Any implementation must implement all APIs.
Now, we also define a class that implements the interface in such a way that every interaction simply
returns an error message saying that this particular interaction is not implemented.

Now, in some other part of the program, we can define a new class to implement the `PlayerAPI` interface.
However, we don't want to simply override the current API, because it might be the case that there's
already a proper implementation that was assigned.

The general approach in this case is to, when implementing, design the implementation so that it can first
try to use the previous implementation, and only in case of failure we try our own. An example:
```cs
interface IUsable { 
    public bool Use(player: CompilerObject);
}

class ItemsAreIUsable(PlayerAPI original) : PlayerAPI {
    private PlayerAPI Original { get; } = original;

    public InteractionResult UseItem(CompilerObject player, CompilerObject item) {
        var result = Original.UseItem(player, item);

        if (result.IsError && item is IUsable usable)
            if (usable.Use(player)) result = InteractionResult.Ok(item);
            else result = InteractionResult.Error($"Failed to use item: {item}");

        return result;
    }

    // The other 2 go directly through to the Original since we don't care about them
}
```

And somewhere in the code:
```cs
playerAPI = new ItemsAreIUsable(playerAPI);
```

This shows 2 things:
* An API implementation generally lets some type hierarchy to participate in interactions without
  modifying the core system.
* An API implementation can coexist and respect ordering of registration without ever knowing about
  the exact shape or type of the original implementation.

This architecture will prove *invaluable* when we talk about custom code-generation.
