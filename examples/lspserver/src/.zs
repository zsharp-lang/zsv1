// A language server's endpoints, as fastlsp builds them: each function is decorated with
// the method it answers, and its types are the protocol's own.
import { compile, emit } from "core:project";
import { List } from "core:runtime";
import { request, notification, build } from "pkg:fastlsp";
import { DocumentHighlightParams, DocumentHighlight, DocumentHighlightKind, Range, Position } from "pkg:lsp";

emit(build(compile("LspServer")));
