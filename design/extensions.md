# Diagnostics, extensions and metaclasses

## Diagnostics

`ZSharp.Diagnostics.HIR` prints any HIR graph, with the common code nodes printed as
code, so reified code reads as the calls it makes. The CLI's `--trace-reification`
prints what each reification receives and produces, so every level can be seen, and
`--dump-definitions` prints each definition a document defines. Z# code can call
`dump` from `core:diagnostics` to print an HIR object.

## Extensions

Extensions nest without limit, and an extension can have its own extensions.
The built-in language is itself an extension, written in C#.

Every extension worries only about itself, and works with general shapes rather
than concrete types as much as possible. COs for .NET, for example, should work
with any .NET platform, both the runtime and Mono.Cecil.

Z# should be able to reach the parser, the AST compiler and its configuration
(`of`, imports, operators and so on), the string importer, the `std:` and
`core:` importers, the CG, TS and IR APIs, the runtime, the IL loader, and more
as needed. Custom `std:` and `core:` modules can be registered.

## Metaclasses

Meta compilers emit HIR code that, when executed, evaluates to an HIR `Instance`.
This is close to how Python metaclasses work. They emit code rather than the
object so a definition written inside a function constructs a new object on
every call:

```text
fun foo(base: Type) {
    let cls = def A of Class : base {
        let parent: A?; // ok, references itself
    } as Class;

    // let a: A; // error: A is not a real type here

    return cls;
}
```

The `as` cast marking such a definition is not settled.
