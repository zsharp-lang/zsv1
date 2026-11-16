// Runs the server built from examples/lspserver, in memory: a hover request to an
// endpoint written with the protocol's own types.
import "std:prelude";
import { print } from "std:io";
import { add } from "core:packages";
import { Server, MemoryTransport } from "pkg:fastlsp/runtime";

add("examples");
import { register } from "pkg:lspserver";

let server = Server();
register(server);

let transport = MemoryTransport();
transport.send("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
transport.send("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"textDocument/documentHighlight\",\"params\":{\"textDocument\":{\"uri\":\"file:///a.zs\"},\"position\":{\"line\":7,\"character\":3}}}");
transport.send("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"shutdown\"}");
transport.send("{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
server.run(transport);

let index = 0;
while (index < transport.sent.Count) {
    print(transport.sent[index]);
    index = index + 1;
}
