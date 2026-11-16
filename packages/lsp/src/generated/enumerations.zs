// Generated from the LSP 3.17.0 model by packages/lsp/generate.zs. Do not edit.
// The enumerations of the protocol: a class of constants for each.

// A set of predefined token types. This set is not fixed
// an clients can specify additional token types via the
// corresponding client capabilities.
//
// @since 3.16.0
def SemanticTokenTypes {
    @static let namespace: String = "namespace";
    // Represents a generic type. Acts as a fallback for types which can't be mapped to
    // a specific type like class or enum.
    @static let type: String = "type";
    @static let class: String = "class";
    @static let enum: String = "enum";
    @static let interface: String = "interface";
    @static let struct: String = "struct";
    @static let typeParameter: String = "typeParameter";
    @static let parameter: String = "parameter";
    @static let variable: String = "variable";
    @static let property: String = "property";
    @static let enumMember: String = "enumMember";
    @static let event: String = "event";
    @static let function: String = "function";
    @static let method: String = "method";
    @static let macro: String = "macro";
    @static let keyword: String = "keyword";
    @static let modifier: String = "modifier";
    @static let comment: String = "comment";
    @static let string: String = "string";
    @static let number: String = "number";
    @static let regexp: String = "regexp";
    @static let operator: String = "operator";
    // @since 3.17.0
    @static let decorator: String = "decorator";
    new() { }
}

// A set of predefined token modifiers. This set is not fixed
// an clients can specify additional token types via the
// corresponding client capabilities.
//
// @since 3.16.0
def SemanticTokenModifiers {
    @static let declaration: String = "declaration";
    @static let definition: String = "definition";
    @static let readonly: String = "readonly";
    @static let static: String = "static";
    @static let deprecated: String = "deprecated";
    @static let abstract: String = "abstract";
    @static let async: String = "async";
    @static let modification: String = "modification";
    @static let documentation: String = "documentation";
    @static let defaultLibrary: String = "defaultLibrary";
    new() { }
}

// The document diagnostic report kinds.
//
// @since 3.17.0
def DocumentDiagnosticReportKind {
    // A diagnostic report with a full
    // set of problems.
    @static let Full: String = "full";
    // A report indicating that the last
    // returned report is still accurate.
    @static let Unchanged: String = "unchanged";
    new() { }
}

// Predefined error codes.
def ErrorCodes {
    @static let ParseError: Int32 = -32700;
    @static let InvalidRequest: Int32 = -32600;
    @static let MethodNotFound: Int32 = -32601;
    @static let InvalidParams: Int32 = -32602;
    @static let InternalError: Int32 = -32603;
    // Error code indicating that a server received a notification or
    // request before the server has received the `initialize` request.
    @static let ServerNotInitialized: Int32 = -32002;
    @static let UnknownErrorCode: Int32 = -32001;
    new() { }
}

def LSPErrorCodes {
    // A request failed but it was syntactically correct, e.g the
    // method name was known and the parameters were valid. The error
    // message should contain human readable information about why
    // the request failed.
    //
    // @since 3.17.0
    @static let RequestFailed: Int32 = -32803;
    // The server cancelled the request. This error code should
    // only be used for requests that explicitly support being
    // server cancellable.
    //
    // @since 3.17.0
    @static let ServerCancelled: Int32 = -32802;
    // The server detected that the content of a document got
    // modified outside normal conditions. A server should
    // NOT send this error code if it detects a content change
    // in it unprocessed messages. The result even computed
    // on an older state might still be useful for the client.
    //
    // If a client decides that a result is not of any use anymore
    // the client should cancel the request.
    @static let ContentModified: Int32 = -32801;
    // The client has canceled a request and a server has detected
    // the cancel.
    @static let RequestCancelled: Int32 = -32800;
    new() { }
}

// A set of predefined range kinds.
def FoldingRangeKind {
    // Folding range for a comment
    @static let Comment: String = "comment";
    // Folding range for an import or include
    @static let Imports: String = "imports";
    // Folding range for a region (e.g. `#region`)
    @static let Region: String = "region";
    new() { }
}

// A symbol kind.
def SymbolKind {
    @static let File: Int32 = 1;
    @static let Module: Int32 = 2;
    @static let Namespace: Int32 = 3;
    @static let Package: Int32 = 4;
    @static let Class: Int32 = 5;
    @static let Method: Int32 = 6;
    @static let Property: Int32 = 7;
    @static let Field: Int32 = 8;
    @static let Constructor: Int32 = 9;
    @static let Enum: Int32 = 10;
    @static let Interface: Int32 = 11;
    @static let Function: Int32 = 12;
    @static let Variable: Int32 = 13;
    @static let Constant: Int32 = 14;
    @static let String: Int32 = 15;
    @static let Number: Int32 = 16;
    @static let Boolean: Int32 = 17;
    @static let Array: Int32 = 18;
    @static let Object: Int32 = 19;
    @static let Key: Int32 = 20;
    @static let Null: Int32 = 21;
    @static let EnumMember: Int32 = 22;
    @static let Struct: Int32 = 23;
    @static let Event: Int32 = 24;
    @static let Operator: Int32 = 25;
    @static let TypeParameter: Int32 = 26;
    new() { }
}

// Symbol tags are extra annotations that tweak the rendering of a symbol.
//
// @since 3.16
def SymbolTag {
    // Render a symbol as obsolete, usually using a strike-out.
    @static let Deprecated: Int32 = 1;
    new() { }
}

// Moniker uniqueness level to define scope of the moniker.
//
// @since 3.16.0
def UniquenessLevel {
    // The moniker is only unique inside a document
    @static let document: String = "document";
    // The moniker is unique inside a project for which a dump got created
    @static let project: String = "project";
    // The moniker is unique inside the group to which a project belongs
    @static let group: String = "group";
    // The moniker is unique inside the moniker scheme.
    @static let scheme: String = "scheme";
    // The moniker is globally unique
    @static let global: String = "global";
    new() { }
}

// The moniker kind.
//
// @since 3.16.0
def MonikerKind {
    // The moniker represent a symbol that is imported into a project
    @static let import: String = "import";
    // The moniker represents a symbol that is exported from a project
    @static let export: String = "export";
    // The moniker represents a symbol that is local to a project (e.g. a local
    // variable of a function, a class not visible outside the project, ...)
    @static let local: String = "local";
    new() { }
}

// Inlay hint kinds.
//
// @since 3.17.0
def InlayHintKind {
    // An inlay hint that for a type annotation.
    @static let Type: Int32 = 1;
    // An inlay hint that is for a parameter.
    @static let Parameter: Int32 = 2;
    new() { }
}

// The message type
def MessageType {
    // An error message.
    @static let Error: Int32 = 1;
    // A warning message.
    @static let Warning: Int32 = 2;
    // An information message.
    @static let Info: Int32 = 3;
    // A log message.
    @static let Log: Int32 = 4;
    // A debug message.
    //
    // @since 3.18.0
    @static let Debug: Int32 = 5;
    new() { }
}

// Defines how the host (editor) should sync
// document changes to the language server.
def TextDocumentSyncKind {
    // Documents should not be synced at all.
    @static let None: Int32 = 0;
    // Documents are synced by always sending the full content
    // of the document.
    @static let Full: Int32 = 1;
    // Documents are synced by sending the full content on open.
    // After that only incremental updates to the document are
    // send.
    @static let Incremental: Int32 = 2;
    new() { }
}

// Represents reasons why a text document is saved.
def TextDocumentSaveReason {
    // Manually triggered, e.g. by the user pressing save, by starting debugging,
    // or by an API call.
    @static let Manual: Int32 = 1;
    // Automatic after a delay.
    @static let AfterDelay: Int32 = 2;
    // When the editor lost focus.
    @static let FocusOut: Int32 = 3;
    new() { }
}

// The kind of a completion entry.
def CompletionItemKind {
    @static let Text: Int32 = 1;
    @static let Method: Int32 = 2;
    @static let Function: Int32 = 3;
    @static let Constructor: Int32 = 4;
    @static let Field: Int32 = 5;
    @static let Variable: Int32 = 6;
    @static let Class: Int32 = 7;
    @static let Interface: Int32 = 8;
    @static let Module: Int32 = 9;
    @static let Property: Int32 = 10;
    @static let Unit: Int32 = 11;
    @static let Value: Int32 = 12;
    @static let Enum: Int32 = 13;
    @static let Keyword: Int32 = 14;
    @static let Snippet: Int32 = 15;
    @static let Color: Int32 = 16;
    @static let File: Int32 = 17;
    @static let Reference: Int32 = 18;
    @static let Folder: Int32 = 19;
    @static let EnumMember: Int32 = 20;
    @static let Constant: Int32 = 21;
    @static let Struct: Int32 = 22;
    @static let Event: Int32 = 23;
    @static let Operator: Int32 = 24;
    @static let TypeParameter: Int32 = 25;
    new() { }
}

// Completion item tags are extra annotations that tweak the rendering of a completion
// item.
//
// @since 3.15.0
def CompletionItemTag {
    // Render a completion as obsolete, usually using a strike-out.
    @static let Deprecated: Int32 = 1;
    new() { }
}

// Defines whether the insert text in a completion item should be interpreted as
// plain text or a snippet.
def InsertTextFormat {
    // The primary text to be inserted is treated as a plain string.
    @static let PlainText: Int32 = 1;
    // The primary text to be inserted is treated as a snippet.
    //
    // A snippet can define tab stops and placeholders with `$1`, `$2`
    // and `${3:foo}`. `$0` defines the final tab stop, it defaults to
    // the end of the snippet. Placeholders with equal identifiers are linked,
    // that is typing in one will update others too.
    //
    // See also: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#snippet_syntax
    @static let Snippet: Int32 = 2;
    new() { }
}

// How whitespace and indentation is handled during completion
// item insertion.
//
// @since 3.16.0
def InsertTextMode {
    // The insertion or replace strings is taken as it is. If the
    // value is multi line the lines below the cursor will be
    // inserted using the indentation defined in the string value.
    // The client will not apply any kind of adjustments to the
    // string.
    @static let asIs: Int32 = 1;
    // The editor adjusts leading whitespace of new lines so that
    // they match the indentation up to the cursor of the line for
    // which the item is accepted.
    //
    // Consider a line like this: <2tabs><cursor><3tabs>foo. Accepting a
    // multi line completion item is indented using 2 tabs and all
    // following lines inserted will be indented using 2 tabs as well.
    @static let adjustIndentation: Int32 = 2;
    new() { }
}

// A document highlight kind.
def DocumentHighlightKind {
    // A textual occurrence.
    @static let Text: Int32 = 1;
    // Read-access of a symbol, like reading a variable.
    @static let Read: Int32 = 2;
    // Write-access of a symbol, like writing to a variable.
    @static let Write: Int32 = 3;
    new() { }
}

// A set of predefined code action kinds
def CodeActionKind {
    // Empty kind.
    @static let Empty: String = "";
    // Base kind for quickfix actions: 'quickfix'
    @static let QuickFix: String = "quickfix";
    // Base kind for refactoring actions: 'refactor'
    @static let Refactor: String = "refactor";
    // Base kind for refactoring extraction actions: 'refactor.extract'
    //
    // Example extract actions:
    //
    // - Extract method
    // - Extract function
    // - Extract variable
    // - Extract interface from class
    // - ...
    @static let RefactorExtract: String = "refactor.extract";
    // Base kind for refactoring inline actions: 'refactor.inline'
    //
    // Example inline actions:
    //
    // - Inline function
    // - Inline variable
    // - Inline constant
    // - ...
    @static let RefactorInline: String = "refactor.inline";
    // Base kind for refactoring rewrite actions: 'refactor.rewrite'
    //
    // Example rewrite actions:
    //
    // - Convert JavaScript function to class
    // - Add or remove parameter
    // - Encapsulate field
    // - Make method static
    // - Move method to base class
    // - ...
    @static let RefactorRewrite: String = "refactor.rewrite";
    // Base kind for source actions: `source`
    //
    // Source code actions apply to the entire file.
    @static let Source: String = "source";
    // Base kind for an organize imports source action: `source.organizeImports`
    @static let SourceOrganizeImports: String = "source.organizeImports";
    // Base kind for auto-fix source actions: `source.fixAll`.
    //
    // Fix all actions automatically fix errors that have a clear fix that do not require user input.
    // They should not suppress errors or perform unsafe fixes such as generating new types or classes.
    //
    // @since 3.15.0
    @static let SourceFixAll: String = "source.fixAll";
    new() { }
}

def TraceValues {
    // Turn tracing off.
    @static let Off: String = "off";
    // Trace messages only.
    @static let Messages: String = "messages";
    // Verbose message tracing.
    @static let Verbose: String = "verbose";
    new() { }
}

// Describes the content type that a client supports in various
// result literals like `Hover`, `ParameterInfo` or `CompletionItem`.
//
// Please note that `MarkupKinds` must not start with a `$`. This kinds
// are reserved for internal usage.
def MarkupKind {
    // Plain text is supported as a content format
    @static let PlainText: String = "plaintext";
    // Markdown is supported as a content format
    @static let Markdown: String = "markdown";
    new() { }
}

// Describes how an {@link InlineCompletionItemProvider inline completion provider} was triggered.
//
// @since 3.18.0
// @proposed
def InlineCompletionTriggerKind {
    // Completion was triggered explicitly by a user gesture.
    @static let Invoked: Int32 = 0;
    // Completion was triggered automatically while editing.
    @static let Automatic: Int32 = 1;
    new() { }
}

// A set of predefined position encoding kinds.
//
// @since 3.17.0
def PositionEncodingKind {
    // Character offsets count UTF-8 code units (e.g. bytes).
    @static let UTF8: String = "utf-8";
    // Character offsets count UTF-16 code units.
    //
    // This is the default and must always be supported
    // by servers
    @static let UTF16: String = "utf-16";
    // Character offsets count UTF-32 code units.
    //
    // Implementation note: these are the same as Unicode codepoints,
    // so this `PositionEncodingKind` may also be used for an
    // encoding-agnostic representation of character offsets.
    @static let UTF32: String = "utf-32";
    new() { }
}

// The file event type
def FileChangeType {
    // The file got created.
    @static let Created: Int32 = 1;
    // The file got changed.
    @static let Changed: Int32 = 2;
    // The file got deleted.
    @static let Deleted: Int32 = 3;
    new() { }
}

def WatchKind {
    // Interested in create events.
    @static let Create: Int32 = 1;
    // Interested in change events
    @static let Change: Int32 = 2;
    // Interested in delete events
    @static let Delete: Int32 = 4;
    new() { }
}

// The diagnostic's severity.
def DiagnosticSeverity {
    // Reports an error.
    @static let Error: Int32 = 1;
    // Reports a warning.
    @static let Warning: Int32 = 2;
    // Reports an information.
    @static let Information: Int32 = 3;
    // Reports a hint.
    @static let Hint: Int32 = 4;
    new() { }
}

// The diagnostic tags.
//
// @since 3.15.0
def DiagnosticTag {
    // Unused or unnecessary code.
    //
    // Clients are allowed to render diagnostics with this tag faded out instead of having
    // an error squiggle.
    @static let Unnecessary: Int32 = 1;
    // Deprecated or obsolete code.
    //
    // Clients are allowed to rendered diagnostics with this tag strike through.
    @static let Deprecated: Int32 = 2;
    new() { }
}

// How a completion was triggered
def CompletionTriggerKind {
    // Completion was triggered by typing an identifier (24x7 code
    // complete), manual invocation (e.g Ctrl+Space) or via API.
    @static let Invoked: Int32 = 1;
    // Completion was triggered by a trigger character specified by
    // the `triggerCharacters` properties of the `CompletionRegistrationOptions`.
    @static let TriggerCharacter: Int32 = 2;
    // Completion was re-triggered as current completion list is incomplete
    @static let TriggerForIncompleteCompletions: Int32 = 3;
    new() { }
}

// How a signature help was triggered.
//
// @since 3.15.0
def SignatureHelpTriggerKind {
    // Signature help was invoked manually by the user or by a command.
    @static let Invoked: Int32 = 1;
    // Signature help was triggered by a trigger character.
    @static let TriggerCharacter: Int32 = 2;
    // Signature help was triggered by the cursor moving or by the document content changing.
    @static let ContentChange: Int32 = 3;
    new() { }
}

// The reason why code actions were requested.
//
// @since 3.17.0
def CodeActionTriggerKind {
    // Code actions were explicitly requested by the user or by an extension.
    @static let Invoked: Int32 = 1;
    // Code actions were requested automatically.
    //
    // This typically happens when current selection in a file changes, but can
    // also be triggered when file content changes.
    @static let Automatic: Int32 = 2;
    new() { }
}

// A pattern kind describing if a glob pattern matches a file a folder or
// both.
//
// @since 3.16.0
def FileOperationPatternKind {
    // The pattern matches a file only.
    @static let file: String = "file";
    // The pattern matches a folder only.
    @static let folder: String = "folder";
    new() { }
}

// A notebook cell kind.
//
// @since 3.17.0
def NotebookCellKind {
    // A markup-cell is formatted source that is used for display.
    @static let Markup: Int32 = 1;
    // A code-cell is source code.
    @static let Code: Int32 = 2;
    new() { }
}

def ResourceOperationKind {
    // Supports creating new files and folders.
    @static let Create: String = "create";
    // Supports renaming existing files and folders.
    @static let Rename: String = "rename";
    // Supports deleting existing files and folders.
    @static let Delete: String = "delete";
    new() { }
}

def FailureHandlingKind {
    // Applying the workspace change is simply aborted if one of the changes provided
    // fails. All operations executed before the failing operation stay executed.
    @static let Abort: String = "abort";
    // All operations are executed transactional. That means they either all
    // succeed or no changes at all are applied to the workspace.
    @static let Transactional: String = "transactional";
    // If the workspace edit contains only textual file changes they are executed transactional.
    // If resource changes (create, rename or delete file) are part of the change the failure
    // handling strategy is abort.
    @static let TextOnlyTransactional: String = "textOnlyTransactional";
    // The client tries to undo the operations already executed. But there is no
    // guarantee that this is succeeding.
    @static let Undo: String = "undo";
    new() { }
}

def PrepareSupportDefaultBehavior {
    // The client's default behavior is to select the identifier
    // according the to language's syntax rule.
    @static let Identifier: Int32 = 1;
    new() { }
}

def TokenFormat {
    @static let Relative: String = "relative";
    new() { }
}

