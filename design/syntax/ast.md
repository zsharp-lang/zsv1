# AST

Z# syntax is build in such a way that many things are expressions.
If it wasn't so inconvenient to write code such that everything is
an expression, Z# would've been an everything-is-an-expression
language. However, nature does not agree with this, and so we also
have statements.

In general we can split the entire AST to 2 main categories:
* Expression
* Statement

There are some nodes that are neither. Things like `Parameter` or
`Argument` that have 1 and only 1 possible slot.

Z# represents types as expression.

See `D:/zsharp/zsv1/ZSharp.AST` for the actual implementation.
