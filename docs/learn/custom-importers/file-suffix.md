# Custom file suffix importers

A source with no prefix is a file, and a file is imported by what is registered for its
extension in `core:zsharp`'s `files`. `.zs` and `.dll` are registered that way. A library
registers another with `files.Register(extension, importer)`, where the importer is given
the file's full path and gives what importing it gives, or why it can't.

## An importer of text files

This imports a `.txt` file as a module with one global, `text`, its contents:

```zs
import { print } from "std:io";
import { files } from "core:zsharp";
import { Module } from "std:modules";
import { Instance } from "pkg:zsharp/hir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { Object, String } from "core:runtime";
import { File } from Object.Assembly;

module Text {
    fun load(path: String): IResult[Instance, Error] {
        let content = File.ReadAllText(path);
        return Result[Instance].Ok(module TextFile { let text = content; } as Module);
    }
}

files.Register(".txt", Text.load);

File.WriteAllText("notes.txt", "hello from a text file");

import { text } from "notes.txt";
print(text);
```

```output
hello from a text file
```

- `load` is in a module because what is registered is a delegate, and a function of a
  module is one.
- The module it gives is built by a cast: `content` is read as the cast runs, so the
  module's `text` holds the file's contents.
- The import is a document's, so it runs as it is compiled, after the statements before
  it ran: `notes.txt` exists by then.

## What to give

An importer gives a description, not the thing itself. Here it is a module built of Z#'s
own definitions, which is the simplest. An importer of a format with its own structure,
as a schema, a grammar or another language's sources, can describe it in its own terms
with [nodes of its own](nodes-and-loaders.md).

What importing a file gives is decided by its extension alone, so a format is imported the
same way wherever its files are: a document's import, a module's, or a package that is a
file of that extension.
