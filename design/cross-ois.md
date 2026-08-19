There are 2 cases in which we cross the OIS boundary.

The first is when we want to produce a `CompilerObject` from some other source.
The second is when we want to produce some object from a given `CompilerObject`.

## Going Into OIS

Sometimes we want to do operations on objects without caring about the exact
types of these objects. This is exactly when we want to use the OIS.

For example, suppose we have an `AST.Expression` node that describes some sort
of expression. Traditional compilers will recursively iterate over the tree and,
based on each node type, decide what to do.

Using the OIS is similar, but has an important difference.
Suppose we have a `Compile(AST.Expression) -> CompilerObject` function. This
function is the entrypoint to recursion. As we recursively iterate into the tree,
we might get to some nodes which, for example, the CG API can already handle.
For example, an `AST.Call` node can be directly compiled via `CG.Call`, given we
already compiled the argumens.

The advantage of using this system is that it allows the AST to refer to COs which
came from a completely different source. This is the very backbone of
cross-language interopability.

> NOTE: The actual script runner does *not* do AST -> CO. Instead, it compiles
> AST nodes to ORS objects. More details are in the script runner docs.
> See `ors-vs-ois.md` for more information.


The act of going from some T -> CO can be implemented via a custom API defined
similarly to interactions API:
```cs
interface ASTLoader {
    public InteractionResult Load(AST.Node node);
}
```

This approach sometimes, however, doesn't actually makes much sense to use and
a normal recursive function makes more sense.

## Going Out Of OIS

After we've successfully applied all the interactions we want, we want to produce
some object we can actually work with. For example, some form of platform-agnostic
IR (see `design/lir.md` for more details).

The way we can do it, as we do with COs, is to define an additional API.
