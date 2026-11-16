// What the editor can ask of the server.

@notification("textDocument/didOpen")
fun opened(params: DidOpenTextDocumentParams) {
    let document = Document(params.textDocument.uri);
    document.version = params.textDocument.version;
    document.text = params.textDocument.text;
    documents[document.uri] = document;
    analyze(document);
}

// The editor sends the whole text each time: that is what the server says it wants.
@notification("textDocument/didChange")
fun changed(params: DidChangeTextDocumentParams) {
    let document = documentOf(params.textDocument.uri);
    if (document == null) return;
    if (params.contentChanges.Count == 0) return;

    // A change is a union of a range with its text and the text alone, so it is JSON.
    let change = Unsafe.As[JsonValue](params.contentChanges[params.contentChanges.Count - 1]);
    document.text = change.member("text").asString();
    document.version = params.textDocument.version;
    analyze(document);
}

@notification("textDocument/didClose")
fun closed(params: DidCloseTextDocumentParams) {
    documents.Remove(params.textDocument.uri);

    let parameters = JsonValue.object();
    parameters.put("uri", JsonValue.ofString(params.textDocument.uri));
    parameters.put("diagnostics", JsonValue.array());
    publish("textDocument/publishDiagnostics", parameters);
}

@request("textDocument/documentSymbol")
fun symbols(params: DocumentSymbolParams): List[DocumentSymbol] {
    let document = documentOf(params.textDocument.uri);
    if (document == null) return null;
    return document.symbols;
}

// Where the name at the position is declared in the document: any declaration of that
// name, since what a name refers to needs scopes, which the AST alone doesn't give.
@request("textDocument/definition")
fun definition(params: DefinitionParams): List[Location] {
    let document = documentOf(params.textDocument.uri);
    if (document == null) return null;
    if (params.position.line >= document.lines.Count) return null;

    let name = wordAt(document.lines[params.position.line], params.position.character);
    if (name == null) return null;

    let found = List[Location]();
    declarationsOf(document.symbols, name, document.uri, found);
    return found;
}

@request("textDocument/hover")
fun hover(params: HoverParams): Hover {
    let document = documentOf(params.textDocument.uri);
    if (document == null) return null;
    if (params.position.line >= document.lines.Count) return null;

    let name = wordAt(document.lines[params.position.line], params.position.character);
    if (name == null) return null;
    let declared = firstDeclaration(document.symbols, name);
    if (declared == null) return null;

    // What a declaration is written as: its first line.
    let line = document.lines[declared.range.start.line].Trim();
    let contents = JsonValue.object();
    contents.put("kind", JsonValue.ofString("markdown"));
    contents.put("value", JsonValue.ofString("```zs\n" + line + "\n```"));

    let result = Hover();
    result.contents = contents;
    return result;
}
