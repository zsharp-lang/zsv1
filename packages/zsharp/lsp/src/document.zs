// A document the editor has open: its text as the editor last said, and what was read
// of it.

def Document {
    let uri: String;
    let version: Int32 = 0;
    let text: String = "";
    let lines: List[String] = List[String]();
    let parsed: Parsed = null;
    let symbols: List[DocumentSymbol] = List[DocumentSymbol]();

    new(this, uri: String) { this.uri = uri; }
}

// The lines of a text, which is what a position of the protocol counts in.
fun linesOf(text: String): List[String] {
    let result = List[String]();
    let rest = text.Replace("\r", "");
    let end = rest.IndexOf("\n");
    while (end >= 0) {
        result.Add(rest.Substring(0, end));
        rest = rest.Substring(end + 1);
        end = rest.IndexOf("\n");
    }
    result.Add(rest);
    return result;
}

fun position(line: Int32, character: Int32): Position {
    let made = Position();
    made.line = line;
    made.character = character;
    return made;
}

fun rangeOf(start: Position, end: Position): Range {
    let made = Range();
    made.start = start;
    made.end = end;
    return made;
}

// The range of a span of the parser, whose lines and columns count from 1 and whose end
// is after its last character.
fun spanRange(startLine: Int32, startColumn: Int32, endLine: Int32, endColumn: Int32): Range {
    return rangeOf(position(startLine - 1, startColumn - 1), position(endLine - 1, endColumn - 1));
}

fun isNameCharacter(code: Int32): Boolean {
    if (code >= 48) {
        if (code <= 57) return true;
    }
    if (code >= 65) {
        if (code <= 90) return true;
    }
    if (code >= 97) {
        if (code <= 122) return true;
    }
    return code == 95;
}

// The name a position is in or at the end of, or null.
fun wordAt(line: String, character: Int32): String {
    if (line == null) return null;
    let start = character;
    if (start > line.Length) start = line.Length;
    let end = start;
    while (start > 0) {
        if (isNameCharacter(Convert.ToInt32(line[start - 1])) == false) break;
        start = start - 1;
    }
    while (end < line.Length) {
        if (isNameCharacter(Convert.ToInt32(line[end])) == false) break;
        end = end + 1;
    }
    if (end == start) return null;
    return line.Substring(start, end - start);
}
