// A server: what answers the messages of a client. It knows JSON-RPC and the protocol's
// lifecycle (`initialize`, `shutdown`, `exit`), and nothing of a language. What a language
// adds is endpoints, which are registered by the method they answer, as JSON in and JSON
// out; fastlsp generates the code that makes them typed (see `build`).
//
// `handle` takes one message and gives the response text, or null if there is none, so a
// server can be run on any transport, or with none, as a test does.

def Server {
    let requests: Dictionary[String, Func[JsonValue, JsonValue]] = Dictionary[String, Func[JsonValue, JsonValue]]();
    let notifications: Dictionary[String, Func[JsonValue, JsonValue]] = Dictionary[String, Func[JsonValue, JsonValue]]();

    // The methods registered, in order, which the capabilities are made from.
    let methods: List[String] = List[String]();

    // Messages the server sends of its own accord, as a notification is, in the order they
    // were made: they go out after the response to the message that made them.
    let outgoing: List[String] = List[String]();

    let initialized: Boolean = false;
    let shutdownRequested: Boolean = false;
    let exited: Boolean = false;

    new() { }

    get IsExited(): Boolean { return exited; }

    // What answers a request: it takes the request's `params` and gives its result.
    fun onRequest(method: String, handler: Func[JsonValue, JsonValue]) {
        requests[method] = handler;
        methods.Add(method);
    }

    // What handles a notification: its result, whatever it is, is dropped.
    fun onNotification(method: String, handler: Func[JsonValue, JsonValue]) {
        notifications[method] = handler;
        methods.Add(method);
    }

    fun isRegistered(method: String): Boolean {
        if (requests.ContainsKey(method)) return true;
        return notifications.ContainsKey(method);
    }

    // Sends a notification to the client.
    fun notify(method: String, parameters: JsonValue) {
        let message = JsonValue.object();
        message.put("jsonrpc", JsonValue.ofString("2.0"));
        message.put("method", JsonValue.ofString(method));
        message.put("params", parameters);
        outgoing.Add(writeJson(message));
    }

    // What the server sent since this was last asked, which is then forgotten.
    fun drain(): List[String] {
        let sent = List[String]();
        sent.AddRange(outgoing);
        outgoing.Clear();
        return sent;
    }

    // ---- Messages

    fun result(id: JsonValue, value: JsonValue): String {
        let message = JsonValue.object();
        message.put("jsonrpc", JsonValue.ofString("2.0"));
        message.put("id", id);
        if (value == null) {
            message.put("result", JsonValue());
        } else {
            message.put("result", value);
        }
        return writeJson(message);
    }

    fun failure(id: JsonValue, code: Int32, text: String): String {
        let error = JsonValue.object();
        error.put("code", JsonValue.ofNumber(code));
        error.put("message", JsonValue.ofString(text));

        let message = JsonValue.object();
        message.put("jsonrpc", JsonValue.ofString("2.0"));
        if (id == null) {
            message.put("id", JsonValue());
        } else {
            message.put("id", id);
        }
        message.put("error", error);
        return writeJson(message);
    }

    // Answers one message: the text of the response, or null if the message needs none.
    fun handle(text: String): String {
        let parser = JsonParser(text);
        let message = parser.parse();
        if (message == null) return failure(null, ErrorCodes.ParseError, parser.error);
        if (message.IsObject == false) return failure(null, ErrorCodes.InvalidRequest, "A message is an object");

        let name = message.member("method");
        // A response to a request the server made.
        if (name == null) return null;

        let method = name.asString();
        let id = message.member("id");
        let parameters = message.member("params");

        if (id == null) {
            receive(method, parameters);
            return null;
        }
        return request(id, method, parameters);
    }

    fun receive(method: String, parameters: JsonValue) {
        if (method == "exit") {
            exited = true;
            return;
        }
        if (method == "initialized") return;
        if (notifications.ContainsKey(method)) notifications[method](parameters);
    }

    fun request(id: JsonValue, method: String, parameters: JsonValue): String {
        if (method == "initialize") {
            initialized = true;
            if (requests.ContainsKey(method)) return result(id, requests[method](parameters));
            return result(id, defaultInitialize());
        }

        if (initialized == false) return failure(id, ErrorCodes.ServerNotInitialized, "The server is not initialized");
        if (shutdownRequested) return failure(id, ErrorCodes.InvalidRequest, "The server is shut down");

        if (method == "shutdown") {
            shutdownRequested = true;
            return result(id, null);
        }
        if (requests.ContainsKey(method) == false) return failure(id, ErrorCodes.MethodNotFound, "No endpoint answers " + method);
        return result(id, requests[method](parameters));
    }

    // ---- Capabilities

    // What the protocol calls a capability of a method: the key of the server's
    // capabilities that says the server answers it, for the methods that have one.
    fun capabilityOf(method: String): String {
        if (method == "textDocument/hover") return "hoverProvider";
        if (method == "textDocument/definition") return "definitionProvider";
        if (method == "textDocument/declaration") return "declarationProvider";
        if (method == "textDocument/typeDefinition") return "typeDefinitionProvider";
        if (method == "textDocument/implementation") return "implementationProvider";
        if (method == "textDocument/references") return "referencesProvider";
        if (method == "textDocument/documentHighlight") return "documentHighlightProvider";
        if (method == "textDocument/documentSymbol") return "documentSymbolProvider";
        if (method == "textDocument/codeAction") return "codeActionProvider";
        if (method == "textDocument/formatting") return "documentFormattingProvider";
        if (method == "textDocument/rangeFormatting") return "documentRangeFormattingProvider";
        if (method == "textDocument/rename") return "renameProvider";
        if (method == "textDocument/foldingRange") return "foldingRangeProvider";
        if (method == "textDocument/selectionRange") return "selectionRangeProvider";
        if (method == "workspace/symbol") return "workspaceSymbolProvider";
        return null;
    }

    // The options of a capability that is not a plain yes.
    fun optionsOf(capability: String): JsonValue {
        let options = JsonValue.object();
        return options;
    }

    // The capabilities of the methods registered: what a server that answers them says it
    // can do.
    fun capabilities(): JsonValue {
        let found = JsonValue.object();
        let index = 0;
        while (index < methods.Count) {
            let method = methods[index];
            let capability = capabilityOf(method);
            if (capability != null) found.put(capability, JsonValue.ofBoolean(true));
            if (method == "textDocument/completion") found.put("completionProvider", optionsOf("completionProvider"));
            if (method == "textDocument/signatureHelp") found.put("signatureHelpProvider", optionsOf("signatureHelpProvider"));
            index = index + 1;
        }
        // Full synchronization, once the server wants to know what a document says.
        if (isRegistered("textDocument/didChange")) found.put("textDocumentSync", JsonValue.ofNumber(1));
        return found;
    }

    fun defaultInitialize(): JsonValue {
        let found = JsonValue.object();
        found.put("capabilities", capabilities());
        return found;
    }

    // ---- Running

    // Answers each message the transport reads, through it, until a client's `exit` or the
    // transport ends: it reads null.
    fun run(transport: Transport) {
        let text = transport.read();
        while (text != null) {
            let response = handle(text);
            if (response != null) transport.write(response);
            let sent = drain();
            let at = 0;
            while (at < sent.Count) {
                transport.write(sent[at]);
                at = at + 1;
            }
            if (exited) return;
            text = transport.read();
        }
    }

    // The code the process should exit with: 0 if the client asked for a shutdown first.
    get ExitCode(): Int32 {
        if (shutdownRequested) return 0;
        return 1;
    }
}
