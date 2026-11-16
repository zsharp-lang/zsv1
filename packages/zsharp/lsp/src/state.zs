// The documents the editor has open, and what is told of them as they change.

let documents: Dictionary[String, Document] = Dictionary[String, Document]();

fun jsonPosition(line: Int32, character: Int32): JsonValue {
    let made = JsonValue.object();
    made.put("line", JsonValue.ofNumber(line));
    made.put("character", JsonValue.ofNumber(character));
    return made;
}

fun diagnosticOf(problem: Problem): JsonValue {
    let span = problem.Span;
    let start = span.Start;
    let end = span.End;
    let endColumn = end.Column;
    // An empty range is nowhere an editor can show: the character it is at.
    if (end.Line == start.Line) {
        if (end.Column <= start.Column) endColumn = start.Column + 1;
    }

    let range = JsonValue.object();
    range.put("start", jsonPosition(start.Line - 1, start.Column - 1));
    range.put("end", jsonPosition(end.Line - 1, endColumn - 1));

    let diagnostic = JsonValue.object();
    diagnostic.put("range", range);
    diagnostic.put("severity", JsonValue.ofNumber(1));
    diagnostic.put("source", JsonValue.ofString("zsharp"));
    diagnostic.put("message", JsonValue.ofString(problem.Message));
    return diagnostic;
}

// Reads a document again and tells the editor what is wrong with it.
fun analyze(document: Document) {
    document.lines = linesOf(document.text);
    document.parsed = Recovery.Parse(document.text, document.uri);
    document.symbols = List[DocumentSymbol]();
    collect(document.parsed.Statements, document.lines, false, document.symbols);

    let diagnostics = JsonValue.array();
    let index = 0;
    while (index < document.parsed.Problems.Count) {
        diagnostics.add(diagnosticOf(document.parsed.Problems[index]));
        index = index + 1;
    }

    let parameters = JsonValue.object();
    parameters.put("uri", JsonValue.ofString(document.uri));
    parameters.put("version", JsonValue.ofNumber(document.version));
    parameters.put("diagnostics", diagnostics);
    publish("textDocument/publishDiagnostics", parameters);
}

fun documentOf(uri: String): Document {
    if (documents.ContainsKey(uri)) return documents[uri];
    return null;
}
