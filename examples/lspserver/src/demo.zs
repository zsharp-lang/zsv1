@request("textDocument/documentHighlight")
fun highlight(params: DocumentHighlightParams): List[DocumentHighlight] {
    let result = List[DocumentHighlight]();

    let first = DocumentHighlight();
    first.range = Range();
    first.range.start = params.position;
    first.range.end = Position();
    first.range.end.line = params.position.line;
    first.range.end.character = params.position.character + 3;
    first.kind = DocumentHighlightKind.Text;
    result.Add(first);
    return result;
}
