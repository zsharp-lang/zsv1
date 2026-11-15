// The Z# language server, over standard input and output: what an editor runs.
//
//     zs packages/zsharp/lsp/serve.zs
//
// An editor's extension starts it as a process, with `zs` of the toolchain installed and this
// script, and speaks the Language Server Protocol to it. Nothing in the extension need
// know Z#: it is a client of this program.
import "std:prelude";
import { Object } from "core:runtime";
import { Environment } from Object.Assembly;
import { Server, Stdio } from "pkg:fastlsp/runtime";
import { register } from "pkg:zsharp/lsp";

let server = Server();
register(server);
server.run(Stdio());
Environment.Exit(server.ExitCode);
