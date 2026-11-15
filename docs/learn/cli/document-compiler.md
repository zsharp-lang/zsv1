# The document compiler

A document is a file of Z#, and the document compiler is its meta-compiler: what a
document means is that it runs statement by statement. That is a choice of the document's
meta-compiler, not a property of Z#: a module's meta-compiler compiles its statements as a
module's members, and runs none of them.

## Statement by statement

A document is parsed and run one statement at a time. Each statement is compiled, lowered
for the runtime and run, before the next one is parsed. So:

- what a statement defines is there for the next one, as a real value;
- an import runs as its statement does, and its names are bound to what it gave;
- a definition is built, loaded and run right before the statement that has it, as
  nothing else builds a document's code;
- a statement nested in another, as one in a `while`'s block, is run the same way.

## Locals

A `let` of a document is a local of the document, which lives as long as the document
does. The runtime gives each document a frame that holds its locals' values, and runs
each statement with it. A local is never captured: code built in a document gets a
local's value from when it was built, since a module compiled to an assembly no longer has
the document it was compiled in.

## What importing one gives

Running a document fills in the node that importing it gives: what it marks with
`@export`, by name. A document runs once in a process, however often it is imported, and
two documents that import each other are an error.

## The script and the documents it imports

The script `zs` runs is the first document. A document it imports is run when the import
is, with its own scope: it has only what it imports itself, and the prelude isn't imported
for it. A project's module file is a document too, which the project compiler runs, with
`core:project`.
