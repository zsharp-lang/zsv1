# Syntax

The syntax of Z#, as its grammar, `pkg:zsharp/lang/syntax`, defines it. This is a
specification: [the basics](../../learn/basics/basics.md) teach it. The notation is
informal: `[x]` is optional, `x*` is any number of `x`, `x | y` is either, and quoted text
is written as it is.

## Lexical

- **Comments** run from `//` to the end of the line.
- **Names** are a letter or `_`, then letters, digits and `_`. A keyword can't be a name.
- **Keywords**: `as`, `break`, `case`, `continue`, `def`, `else`, `for`, `from`, `fun`,
  `get`, `if`, `import`, `in`, `is`, `let`, `module`, `new`, `of`, `or`, `return`, `set`,
  `var`, `when`, `while`.
- **Literals**: integers, as `42`; decimals, as `1.5`; strings in double quotes, with `\`
  escapes; `true`, `false` and `null`.

## Documents and contexts

A document is a list of statements. What a statement can be depends on its context,
each of which has the statements of its keywords, and a default for what no keyword
starts:

| Context | Statements | Default |
|---|---|---|
| A document | `import`, `let`, `var`, `def`, `fun`, `module`, `if`, `while`, decorated statements | an expression statement |
| A module's body | `import`, `let`, `var`, `def`, `fun` | an expression statement |
| A class's body | `def`, `import`, `let`, `fun`, `new`, `get`, `set`, decorated members | |
| A function's body | `return`, `let`, `var`, `fun`, `if`, `while`, `for`, `case`, `import` | an expression statement |
| A loop's body | `break`, `continue` | what the loop is in parses |

```text
statement   = expression ";" | block | definition | import | if | while | return | break | continue
block       = "{" statement* "}"
```

## Imports

```text
import      = "import" [ "(" arguments ")" ] "{" imported ("," imported)* "}" "from" expression ";"
            | "import" expression ";"
imported    = name [ "as" name ]
```

## Definitions

```text
definition  = decorator* ( class | function | module )
decorator   = "@" expression

class       = "def" name [ "of" expression ] [ ":" expression ("," expression)* ] "{" member* "}"
function    = "fun" [ name ] [ "[" generic ("," generic)* "]" ] "(" parameters ")" [ ":" expression ]
              ( block | "=>" expression ";" | ";" )
module      = "module" [ name ] ( "{" statement* "}" | ";" statement* )

member      = decorator* ( let | function | constructor | accessor | class | import )
constructor = "new" [ name ] "(" parameters ")" block
accessor    = ( "get" | "set" ) name "(" parameters ")" [ ":" expression ] block

parameters  = [ parameter ("," parameter)* ] [ "," ] [ "{" parameter ("," parameter)* "}" ]
parameter   = name [ "as" name ] [ ":" expression ] [ "=" expression ]
```

- A function with `;` for its body is declared: what provides it is elsewhere.
- `module Name;` makes the rest of the file the module.
- Parameters in braces are named: given by name, in any order.
- A parameter `a as b` is named `a` to callers and `b` to its code.

## Variables

```text
let         = "let" name [ ":" expression ] [ "=" expression ]
var         = "var" name [ ":" expression ] [ "=" expression ]
```

## Control flow

```text
if          = "if" "(" expression ")" statement [ "else" statement ]
while       = "while" [ name ] "(" expression ")" statement [ "else" statement ]
break       = "break" [ expression ] [ "from" name ] ";"
continue    = "continue" [ "from" name ] ";"
return      = "return" [ expression ] ";"
```

A `while` is an expression too: its value is what a `break` leaves it with, or its `else`
when the condition stops holding.

## Expressions

From the loosest to the tightest:

| Precedence | Operators | Associativity |
|---|---|---|
| 10 | `=` | right |
| 15 | `or` | left |
| 16 | `\|` | left |
| 17 | `&` | left |
| 18 | `==` `!=` | left |
| 20 | `<` `>` `<=` `>=` `as` `is` | left |
| 50 | `+` `-` | left |
| 70 | `*` `/` | left |
| 75 | `-x` | prefix |
| 80 | `**` | left |
| 100 | `f(arguments)`, `x[arguments]` | left |
| 150 | `x.name` | left |

```text
primary     = name | literal | "(" expression ")" | "(" operator ")" | array
            | let | class | function | module | while
array       = "[" [ expression ("," expression)* ] "]"
arguments   = [ argument ("," argument)* ]
argument    = [ name ":" ] expression
cast        = expression "as" expression
test        = expression "is" pattern
pattern     = expression | "let" name [ ":" expression ]
```

- `(operator)`, as `(+)`, is an operator literal: the functions the operator stands for.
- `def`, `fun` and `module` are expressions too, as in `def Point { } as Class`: a
  definition in an expression binds no name outside itself.
- `x is let name: T` tests a value against a type and binds it as `name` if it matches.

## Not yet

`for` and `case` parse, but nothing compiles them yet. `**` parses, and no type defines
it yet. There is no `and`, `not` or `!`.
