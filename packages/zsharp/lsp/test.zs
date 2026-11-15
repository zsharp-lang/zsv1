// Talks to the Z# language server in memory: opens a document that has a mistake, asks
// for its outline, where a name is declared, and what a name is, and prints what it said.
//
//     zs packages/zsharp/lsp/test.zs
import "std:prelude";
import { print } from "std:io";
import { String, Int32 } from "core:runtime";
import { Server, MemoryTransport, JsonValue, writeJson } from "pkg:fastlsp/runtime";
import { register } from "pkg:zsharp/lsp";

let server = Server();
register(server);

let source = "import { print } from \"std:io\";\ndef Point {\n    let x: Int32 = 0;\n    fun moved(by: Int32): Int32 { return x + by; }\n}\nfun broken( ;\nfun after() { }\n";

fun message(id: Int32, method: String, parameters: String): String {
    return "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"method\":\"" + method + "\",\"params\":" + parameters + "}";
}

fun notification(method: String, parameters: String): String {
    return "{\"jsonrpc\":\"2.0\",\"method\":\"" + method + "\",\"params\":" + parameters + "}";
}

let document = "{\"uri\":\"file:///a.zs\"}";
let transport = MemoryTransport();
transport.send(message(1, "initialize", "{}"));
transport.send(notification("textDocument/didOpen", "{\"textDocument\":{\"uri\":\"file:///a.zs\",\"languageId\":\"zsharp\",\"version\":1,\"text\":" + writeJson(JsonValue.ofString(source)) + "}}"));
transport.send(message(2, "textDocument/documentSymbol", "{\"textDocument\":" + document + "}"));
transport.send(message(3, "textDocument/definition", "{\"textDocument\":" + document + ",\"position\":{\"line\":3,\"character\":41}}"));
transport.send(message(4, "textDocument/hover", "{\"textDocument\":" + document + ",\"position\":{\"line\":6,\"character\":6}}"));
transport.send(message(5, "shutdown", "null"));
transport.send(notification("exit", "null"));
server.run(transport);

let index = 0;
while (index < transport.sent.Count) {
    print(transport.sent[index]);
    index = index + 1;
}
