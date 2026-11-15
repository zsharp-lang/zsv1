// The outline of a document: what it declares, as the AST says, nested as it is written.
// Each declaration has the range of what declares it and the range of its name.

// The range of a name inside what declares it: where the name is written on the first
// line of it, or else that line.
fun nameRange(lines: List[String], startLine: Int32, startColumn: Int32, name: String): Range {
    let row = startLine - 1;
    if (row < lines.Count) {
        let found = lines[row].IndexOf(name, startColumn - 1);
        if (found >= 0) return spanRange(startLine, found + 1, startLine, found + 1 + name.Length);
    }
    return spanRange(startLine, startColumn, startLine, startColumn);
}

fun symbol(node: Statement, lines: List[String], name: String, kind: Int32): DocumentSymbol {
    let span = node.Location.Span;
    let made = DocumentSymbol();
    made.name = name;
    made.kind = kind;
    made.range = spanRange(span.Start.Line, span.Start.Column, span.End.Line, span.End.Column);
    made.selectionRange = nameRange(lines, span.Start.Line, span.Start.Column, name);
    made.children = List[DocumentSymbol]();
    return made;
}

fun fieldOrVariable(inType: Boolean): Int32 {
    if (inType) return SymbolKind.Field;
    return SymbolKind.Variable;
}

// A declaration written as the expression of a definition statement.
fun symbolOfDefinition(statement: Statement, definition: Object, lines: List[String], inType: Boolean, into: List[DocumentSymbol]) {
    if (definition is let accessor: Accessor) {
        into.Add(symbol(statement, lines, accessor.Name, SymbolKind.Property));
    } else if (definition is let local: LetExpression) {
        into.Add(symbol(statement, lines, local.Name, fieldOrVariable(inType)));
    } else if (definition is let local: VarExpression) {
        into.Add(symbol(statement, lines, local.Name, fieldOrVariable(inType)));
    } else if (definition is let function: AST_Function) {
        if (function.Name != "") {
            if (inType) {
                into.Add(symbol(statement, lines, function.Name, SymbolKind.Method));
            } else {
                into.Add(symbol(statement, lines, function.Name, SymbolKind.Function));
            }
        }
    } else if (definition is let constructor: AST_Constructor) {
        let name = constructor.Name;
        if (name == null) name = "new";
        if (name == "") name = "new";
        into.Add(symbol(statement, lines, name, SymbolKind.Constructor));
    } else if (definition is let type: TypeDefinition) {
        if (type.Name != "") {
            let made = symbol(statement, lines, type.Name, SymbolKind.Class);
            if (type.Content != null) collect(type.Content.Statements, lines, true, made.children);
            into.Add(made);
        }
    } else if (definition is let unit: AST_Module) {
        let made = symbol(statement, lines, unit.Name, SymbolKind.Module);
        collect(unit.Body, lines, false, made.children);
        into.Add(made);
    }
}

// What a list of statements declares.
fun collect(statements: List[Statement], lines: List[String], inType: Boolean, into: List[DocumentSymbol]) {
    let index = 0;
    while (index < statements.Count) {
        let statement = statements[index];
        while (statement is let decorated: DecoratedStatement) { statement = decorated.Statement; }

        if (statement is let definition: DefinitionStatement) {
            symbolOfDefinition(statement, definition.Definition, lines, inType, into);
        } else if (statement is let expression: ExpressionStatement) {
            symbolOfDefinition(statement, expression.Expression, lines, inType, into);
        } else if (statement is let declaration: LetStatement) {
            let at = 0;
            while (at < declaration.Definitions.Count) {
                let name = declaration.Definitions[at].Name;
                if (inType) {
                    into.Add(symbol(statement, lines, name, SymbolKind.Field));
                } else {
                    into.Add(symbol(statement, lines, name, SymbolKind.Variable));
                }
                at = at + 1;
            }
        } else if (statement is let imported: ImportStatement) {
            if (imported.ImportedNames != null) {
                let at = 0;
                while (at < imported.ImportedNames.Count) {
                    let name = imported.ImportedNames[at].Name;
                    if (imported.ImportedNames[at].Alias != null) name = imported.ImportedNames[at].Alias;
                    into.Add(symbol(statement, lines, name, SymbolKind.Variable));
                    at = at + 1;
                }
            }
        }
        index = index + 1;
    }
}

// Each declaration of a name, in a tree of them.
fun declarationsOf(symbols: List[DocumentSymbol], name: String, uri: String, into: List[Location]) {
    let index = 0;
    while (index < symbols.Count) {
        if (symbols[index].name == name) {
            let found = Location();
            found.uri = uri;
            found.range = symbols[index].selectionRange;
            into.Add(found);
        }
        declarationsOf(symbols[index].children, name, uri, into);
        index = index + 1;
    }
}

// The first declaration of a name.
fun firstDeclaration(symbols: List[DocumentSymbol], name: String): DocumentSymbol {
    let index = 0;
    while (index < symbols.Count) {
        if (symbols[index].name == name) return symbols[index];
        let inner = firstDeclaration(symbols[index].children, name);
        if (inner != null) return inner;
        index = index + 1;
    }
    return null;
}
