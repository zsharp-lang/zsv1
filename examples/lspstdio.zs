// Runs the server built from examples/lspserver over standard input and output, as an
// editor would: `zs examples/lspstdio.zs`.
import "std:prelude";
import { add } from "core:packages";
import { Server, Stdio } from "pkg:fastlsp/runtime";

add("examples");
import { register } from "pkg:lspserver";

let server = Server();
register(server);
server.run(Stdio());
