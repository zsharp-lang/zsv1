# Code: loops, operators, names and calls

## Loops

A loop can be named, as in `while outer (condition) { ... }`. `break` and `continue`
act on the innermost loop, or on the one they name with `from`: `break from outer;`,
`continue from outer;`. A loop's `else` runs when its condition stops holding, but not
when a `break` leaves it.

A loop's body is parsed with what the loop is in: `break` and `continue` are the loop's,
and any other statement, as a function's `return`, is for the parser of what holds the
loop.

A loop can be a statement or an expression. A loop statement's `else` is optional and
its `break` has no value. A loop expression must have an `else`, which gives its value
when its condition stops holding, and every `break` of it has a value, as in
`break value;` or `break value from outer;`. Its type is that of its `else`, and each
`break` value has to convert to it. This holds for `for` as well, once it exists; `for`
needs an iteration protocol, which is still to be designed.

In HIR, `While` is a statement and `WhileExpression` an expression, and both are a
`Loop`. `Break` and `Continue` refer to their loop's node, so the frontend resolves the
names and HIR never has to. A loop is reified by building its node first, in a variable
of the building code, so a `break` in its body can refer to it. It lowers to
`loop { if (condition) body else { else-part; break } }`, so a `continue` checks the
condition again and a `break` skips the `else`. A loop expression puts that in a
sequence with a local that a `break` or the `else` sets before leaving.

A break or continue can only reach loops of the code it is in: a loop outside the
function it is in is out of reach.

## Operators

Some syntax compiles to a call of a function the language is given, as in `zsv1`.
`Language.Functions` maps a piece of syntax to an HIR `OverloadGroup`: an operator such
as `+` or `<`, and `import`. Each is a group so that more overloads can be added, and
overloading chooses between them. `a + b` compiles to an ordinary call of the `+` group
with both operands. `=`, `.`, `as` and `is` stay syntax, since they aren't calls.

Equality binds looser than comparisons, `as` and `is`, as in C#: `a == b as T` is
`a == (b as T)`, and `a < b == c` is `(a < b) == c`.

An operator before its operand, as `-x`, is a call of the group of its syntax with the
operand alone, so it is an overload that takes one, as `core:runtime`'s `Negate` is of
`-` for `Int32`, `Int64`, `Single` and `Double`, and a CLR type's `op_UnaryNegation`. A
minus before an operand binds tighter than `*` and looser than `**`, so `-2 ** 2` is
`-(2 ** 2)`, and `5 - -3` is 8.

A C# method marked `[Operator("+")]` is an overload of what `+` stands for, and
`ILLoader.Operators` finds them in a type. For now the CLI adds those of `core:runtime`
and `core:zsharp` to one table for the whole language. Scoped tables can come later.
`core:runtime` gives the operators C# predefines: `+ - * / < > <= >= == !=` on `Int32`,
`UInt32`, `Int64`, `UInt64`, `Single` and `Double`, `+` on `String` and between a `String`
and any value, and `or == !=` on `Boolean`. Smaller integers have none of their own and
widen to `Int32`, and mixed operands widen to the most specific overload both go to, so
an `Int32` and an `Int64` add as `Int64`, as C# promotes them. The operators CLR types
define are overloads too, so `String`'s `==` is its own.

A whole number is an `Int32` and a number with a fraction is a `Double`, as in C#. They
are `IntegerLiteral` and `DecimalLiteral` in HIR.

## Binding names

Defining a name asks two separate things: what to bind, and how to handle a name that is
already bound. What to bind depends on the context and is decided by whoever defines
the name. Inside a definition it is the definition's temporary local. Outside it, where
the definition is already loaded, as at document level, it is a `Get` of the
definition's HIR node or of a reference to it.

Conflicts go through the binding REI, `Binder`, which belongs to the Z# frontend and works
during AST -> HIR, so there are no COs. That is right: names are a source-level concept, and conflicts only happen
where there are no actual references. Its dispatchers implement behaviour for HIR
without HIR implementing it. There are three cases:

- The name isn't bound yet: bind it.
- The name is bound in the current scope: merge the two if they can be merged,
  otherwise shadow or report an error.
- The name is bound in an outer scope: bind it here, and the new binding may also do
  something with the one it shadows, such as copying its overloads or adding a lookup
  in the parent.

`MappedBinder` implements it with registered merge and shadow rules and a policy for
conflicts nothing merges, which is an error by default. A name bound again to what it
already stands for, as importing it again does, isn't a conflict. The language registers a rule
that a `let` bound over another `let` overrides it, which is how `let` reuses a name.

A definition compiles roughly to a block that declares the local holding its HIR, as
the meta-type builds it, with the inner construction in a nested block. That local is
never overridden. Inside a container, the locals of its definitions live in the same
scope, but binding works exactly the same, so two definitions with one name are an error
unless a rule merges them: methods merge into an overload group, and a rule could merge
classes that differ in their generic signatures, for example. The .NET meta-types register a rule that merges methods into an
HIR `OverloadGroup`, which loads to a CO that applies an operation to each overload and
keeps the one it succeeds on. `Scope` only stores names; every binding goes through the
binder.

Inside a definition, its own name means what binding it where it is gives: for a method,
all the overloads it is one of. It is in a scope of its own, around the definition's
names, and is bound again when the definition's contents are compiled, since by then a
container has bound the definitions next to it too. Binding it there shadows what the
name means outside, and the .NET meta-types register a shadow rule that a method
shadowing methods overloads them, as the merge rule does in one scope; each overload is
in a group once. Where nothing outside has the name, as at document level before the
definition runs, the name is the definition alone.

Code that is built refers to a definition still being built through the local of the
building code that holds it, which reifying encodes as what the local holds when the
code is built. An overload group is reified the same way, built of its items, so a group
of definitions being built is a group of the definitions.

## Overloads

In HIR, an overload is pure structure. A class doesn't hold overloads, it holds methods.

A class loaded to a CO builds whatever is convenient for it, such as a map from name to
CO. Methods that share a name collapse into an overload group CO at that point, because a
group is special in code generation and not in the structure of a class.

Where one HIR value has to stand for many and there is no class, as in the operator
table, an HIR `OverloadGroup` can be an `Instance`.

Inside a definition of such a kind, its own name is the overloads it is one of, as a
method's is.

What overloads by its name is a kind of definition, by the HIR capability `Overloadable`
of its node's type, as a Z# function's: a name bound in a scope to one such definition
and then to another stands for both, as a group, as the binder merges them, in a module,
a document or anywhere else names are bound, and a call chooses by its arguments. A
definition being built is known by its kind, what describes its node's type, as a
document's reference to one is too. A module keeps each definition of a name, and its
member of that name is their group.

Name-based overloading on the AST side is similar, since it also collapses multiple
values into one object. Member access defers to the CO. A free name uses the same CO,
through the method's `this`.

For .NET meta-types, overloads across a class and its base behave as they do on the CLR.

A group of methods from existing IL can be built directly as a CO while loading, since the
metadata is fully resolved. An overload group in HIR is structure, and the types of its
definitions are known, which is enough because HIR has no behaviour; only loading it
makes a proper CO. A class's overloads are complete when it is loaded, but at document
level more overloads can be added later.

Overloading has nothing to do with calls in particular. It chooses between candidates for
any operation, such as a call or a cast to a function pointer, so each candidate applies
the operation in its own way.

The `Overloading` REI has two entrypoints:

| Entrypoint | Question |
|------------|----------|
| `Apply(candidates, operation)` | Apply an operation to each candidate. Whose result is the result? |
| `IsMoreSpecific(candidate, other)` | When an operation applies to both, is this candidate the better choice? |

The group CO, `OverloadGroup`, is the same for every kind of candidate: HIR overload
groups, IL methods and constructors, and the methods and constructors of a class being
loaded. Each candidate is a CO of its own, and the group implements each operation it
supports as `Apply(candidates, c => CG.<operation>(c, ...))`, so nothing in it is about
calls. A single candidate stands for itself, with no group. For now the group supports
calls; other operations are added the same way.

The static dispatcher chooses the most specific candidate, as C# does. When one candidate
applies, it is the result. When several do, the result is the one that is more specific
than each of the others, and otherwise the call is ambiguous. When none do, each says
why, and the operation is unsupported only if every candidate says it is.

A candidate answers specificity through `ISpecific.IsAsSpecificAs(other)`. Candidates
are never inspected, not even by each other: a candidate with parameters is at least as
specific as another if the other can be called with values of its own parameter types.
So `f(String)` is more specific than `f(Object)`, since `f(Object)` takes a `String` and
`f(String)` doesn't take an `Object`. The values are placeholders that only have a type,
and nothing compiles them. This replaces the separate "exact match" rule IL method groups
had.

These rules are provisional, and nothing should rely much on them:

- CLR methods and constructors are meant to follow C#'s overloading rules. What is here
  is a subset: there are no generics yet, on the CLR or in Z#, and nothing about
  `params`, optional parameters, named arguments or numeric conversions.
- How Z# standard entities overload may change. They use the same rule for now only
  because it is the simplest one; a different rule is a different answer to
  `IsAsSpecificAs`, or a different dispatcher.

Bitwise `&` and `|` bind looser than equality, `&` tighter than `|`, as in C#. They are
defined for integers, for a CLR type's `op_BitwiseAnd` and `op_BitwiseOr`, and for the
flags of an enum, which combine as its underlying integers.

## Arguments

Parameters take their values from a call, as in `zsv1`. A call's arguments become an
`ArgumentStream`: positional ones in order, and named ones by name, in any order. Each
parameter, in order, takes what it needs from the stream through the matching REI,
and an argument left over when they are done is one too many. A callable never looks at
its parameters: it asks each one to match, and a candidate whose parameters don't match
doesn't apply, so overloading doesn't change.

A parameter is any `HIR.Code.Parameter`: a variable that takes its value from a call,
with the name the outside sees and a type, which is all that lowering needs to give it a
place in a signature. How it takes its value is its kind's, through its definition, and
as an instance it is the variable of the code it belongs to. A list of parameters only
holds parameters, so nothing else can get into one; a kind whose definition doesn't
match arguments fails at the call. The kinds so far:

- `PositionalParameter` takes only the next positional argument.
- `NamedParameter` takes only the argument with its name.
- `Package.DotNet.Parameter` takes the argument with its name, or else the next
  positional one, as C# does. A CLR method's parameters match this way too, with the
  default values their metadata gives.

Each takes its default value when it gets no argument. Z# writes the first two:

```zs
fun f(a, b, c, {d, e, f})
```

`a`, `b` and `c` are positional only, and `d`, `e` and `f` named only, so a call can't
name a positional parameter or pass a named one by position, which avoids what mixing
does in Python. The arguments of a call can still come in any order. A CLR method
allows both, as C# does.

A parameter can be written `external as internal`: the external name is what a named
argument names and what documents it, and the internal name is what the code uses, so
an API can read well from outside while the code stays convenient. HIR keeps the
external name; only the frontend knows the internal one, as it binds it.

A Z# default value, `x: T = value`, is code, run at each call that leaves the argument
out, so it isn't limited to constants. A Z# method keeps its parameters once its class
is loaded: the IL loader remembers the parameters each method and constructor it
defined was written with, so a call to it matches those rather than its CLR metadata. A
default can't use the other parameters yet.

How a parameter gets its value is its own, so other kinds fit without changing calls.
An injected parameter takes nothing from the stream and gives the value it is injected
with, so it needs no default value standing in for "not given", and callers never see
it. This generalizes default values and dependency injection. A kind is written with a
parameter decorator that gives one, as an `@inject` decorator gives an injected
parameter; the IL loader's `LoadParameter` is what such a kind registers to be its
variable in the code it belongs to. A parameter that takes
the rest of the arguments, `...rest` or `{...rest}`, is another kind: it takes every
positional or every named argument left. It isn't supported yet, because its values
have to be gathered into a collection, which LIR can't build yet.
