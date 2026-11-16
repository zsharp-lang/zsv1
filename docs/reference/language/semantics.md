# Semantics

What Z#'s programs mean, as the standard definitions define it. Little of it is the
compiler's: what a class, a function or a module is, is defined by the standard library,
and what follows holds where its libraries are installed, as the prelude installs them.
This is a specification: [the basics](../../learn/basics/basics.md) teach it.

## Documents

- A document runs statement by statement: each statement is compiled and run before the
  next is parsed.
- A `let` of a document is a local of it, which lives as long as the document.
- A document's import runs as its statement does, and binds its names to what it gave. A
  name the imported value has no member for is an error of the import.
- A document gives, when imported, what it marks with `@export`. It runs once in a
  process, and two documents that import each other are an error.

## Names

- A definition statement binds its name in the scope it is in, and inside itself.
- A definition in an expression binds its name only inside itself.
- A name is found in the innermost scope that has it. Inside a class, the class's members
  are found by their names before names outside it, unless a parameter has the name.

## Modules

- A module holds definitions: its globals, its functions and its classes.
- A global is initialized in its order, as the module is built. Its initial value is code
  that runs where the global is.
- What is in a module reaches the module's other members by their names, with no import.
- A module's import is run when something of the module that uses it is loaded, once.

## Functions

- A function takes its arguments by its parameters: positional ones by position, named ones
  by name, in any order. A parameter with a default value can be left out.
- Functions of one name in one scope overload. A call chooses the one its arguments apply
  to; of several that apply, the most specific, or else it is an error.
- A function with no body is declared: what it is is provided elsewhere, as a platform or a
  `premap` provides it.
- A function of a module is a value: it converts to a delegate of its signature.

## Classes

- A class's object has its fields, which its constructors set. A field's default value is
  given when the object is made.
- A method runs on an object; `this` is its first parameter where the method asks for it,
  `fun name(this, ...)`.
- `@static` members are the class's, not an object's. `@class` members are the class's
  type object's, and run on it.
- A property is a `get` and a `set`, called as a field is read and written.
- A class has no base class. It implements interfaces, and its objects go wherever an
  interface it implements is expected.
- An interface's methods are what its implementers have. A cast to a class or interface
  checks, as the code runs, that the object is of it.

## Types and values

- Values are of .NET's types, as `Int32`, `String` and `Boolean`, and have their members.
- A value goes where its type is expected, or where its type converts to implicitly, as
  `Int32` does to `Double`.
- `null` goes where any reference type is expected.
- `x as T` converts explicitly. `x is T` tests, and `x is let name: T` tests and binds.

## Operators

- An operator is a call of the functions its syntax stands for, chosen by its operands'
  types as an overload is.
- .NET's operators are those functions for its types; a definition marked
  `@operator((syntax))` adds one.
- `or` gives `true` if either operand is.

## Control flow

- `if` runs its branch if its condition is `true`.
- `while` runs its body while its condition is `true`. As an expression, its value is what
  a `break` leaves it with, or its `else` when the condition stops holding.
- `break` and `continue` leave or repeat the innermost loop, or the loop they name with
  `from`.
- `return` leaves a function, with a value if the function gives one.
