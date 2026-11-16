// The Z# language server: what an editor asks of Z# code, answered from the code alone.
// It parses a document, tolerating what is wrong with it, and answers from the AST, so it
// runs none of the code it reads: diagnostics for what doesn't parse, the outline of a
// document, and where a name is declared in it. What needs Z# to run, as the types the
// code builds, is for later.
//
// It is built with fastlsp, so each endpoint is a decorated function of the protocol's
// own types, and `register(server)` is what `build` makes of them.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope.

import { compile, emit } from "core:project";
import { Object, String, Int32, Boolean, List, Dictionary } from "core:runtime";
import { Path, Convert, Unsafe } from Object.Assembly;

import { request, notification, build } from "pkg:fastlsp";
import { publish, JsonValue, parseJson } from "pkg:fastlsp/runtime";
import {
    DidOpenTextDocumentParams, DidChangeTextDocumentParams, DidCloseTextDocumentParams,
    DocumentSymbolParams, DocumentSymbol, DefinitionParams, HoverParams, Hover,
    Location, Range, Position, SymbolKind
} from "pkg:lsp";

import { Recovery, Parsed, Problem } from "pkg:zsharp/lang/syntax";
import {
    Statement, DefinitionStatement, DecoratedStatement, ImportStatement, LetStatement,
    BlockStatement, TypeDefinition, Constructor as AST_Constructor, Accessor,
    Function as AST_Function, Module as AST_Module, LetExpression, VarExpression, ExpressionStatement
} from "pkg:zsharp/lang/ast";

emit(build(compile("ZSharpLsp")));
