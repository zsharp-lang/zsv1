// Generated from the LSP 3.17.0 model by packages/lsp/generate.zs. Do not edit.
// The structures of the protocol: a class for each, with the properties of what it extends.

def ImplementationParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

// Represents a location inside a resource, such as a line
// inside a text file.
def Location {
    // DocumentUri
    let uri: String = null;
    let range: Range = null;
    new() { }
}

def ImplementationRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def TypeDefinitionParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

def TypeDefinitionRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// A workspace folder inside a client.
def WorkspaceFolder {
    // The associated URI for this workspace folder.
    // URI
    let uri: String = null;
    // The name of the workspace folder. Used to refer to this
    // workspace folder in the user interface.
    // string
    let name: String = null;
    new() { }
}

// The parameters of a `workspace/didChangeWorkspaceFolders` notification.
def DidChangeWorkspaceFoldersParams {
    // The actual workspace folder change event.
    let event: WorkspaceFoldersChangeEvent = null;
    new() { }
}

// The parameters of a configuration request.
def ConfigurationParams {
    // ConfigurationItem[]
    let items: List[ConfigurationItem] = null;
    new() { }
}

// Parameters for a {@link DocumentColorRequest}.
def DocumentColorParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// Represents a color range from a document.
def ColorInformation {
    // The range in the document where this color appears.
    let range: Range = null;
    // The actual color value for this color range.
    let color: Color = null;
    new() { }
}

def DocumentColorRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// Parameters for a {@link ColorPresentationRequest}.
def ColorPresentationParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The color to request presentations for.
    let color: Color = null;
    // The range where the color would be inserted. Serves as a context.
    let range: Range = null;
    new() { }
}

def ColorPresentation {
    // The label of this color presentation. It will be shown on the color
    // picker header. By default this is also the text that is inserted when selecting
    // this color presentation.
    // string
    let label: String = null;
    // An {@link TextEdit edit} which is applied to a document when selecting
    // this presentation for the color.  When `falsy` the {@link ColorPresentation.label label}
    // is used.
    let textEdit: TextEdit = null;
    // An optional array of additional {@link TextEdit text edits} that are applied when
    // selecting this color presentation. Edits must not overlap with the main {@link ColorPresentation.textEdit edit} nor with themselves.
    // TextEdit[]
    let additionalTextEdits: List[TextEdit] = null;
    new() { }
}

def WorkDoneProgressOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// General text document registration options.
def TextDocumentRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    new() { }
}

// Parameters for a {@link FoldingRangeRequest}.
def FoldingRangeParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// Represents a folding range. To be valid, start and end line must be bigger than zero and smaller
// than the number of lines in the document. Clients are free to ignore invalid ranges.
def FoldingRange {
    // The zero-based start line of the range to fold. The folded area starts after the line's last character.
    // To be valid, the end must be zero or larger and smaller than the number of lines in the document.
    // uinteger
    let startLine: Int32 = 0;
    // The zero-based character offset from where the folded range starts. If not defined, defaults to the length of the start line.
    // uinteger
    let startCharacter: Nullable[Int32] = null;
    // The zero-based end line of the range to fold. The folded area ends with the line's last character.
    // To be valid, the end must be zero or larger and smaller than the number of lines in the document.
    // uinteger
    let endLine: Int32 = 0;
    // The zero-based character offset before the folded range ends. If not defined, defaults to the length of the end line.
    // uinteger
    let endCharacter: Nullable[Int32] = null;
    // Describes the kind of the folding range such as `comment' or 'region'. The kind
    // is used to categorize folding ranges and used by commands like 'Fold all comments'.
    // See {@link FoldingRangeKind} for an enumeration of standardized kinds.
    // FoldingRangeKind
    let kind: String = null;
    // The text that the client should show when the specified range is
    // collapsed. If not defined or not supported by the client, a default
    // will be chosen by the client.
    //
    // @since 3.17.0
    // string
    let collapsedText: String = null;
    new() { }
}

def FoldingRangeRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def DeclarationParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

def DeclarationRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// A parameter literal used in selection range requests.
def SelectionRangeParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The positions inside the text document.
    // Position[]
    let positions: List[Position] = null;
    new() { }
}

// A selection range represents a part of a selection hierarchy. A selection range
// may have a parent selection range that contains it.
def SelectionRange {
    // The {@link Range range} of this selection range.
    let range: Range = null;
    // The parent selection range containing this range. Therefore `parent.range` must contain `this.range`.
    let parent: SelectionRange = null;
    new() { }
}

def SelectionRangeRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def WorkDoneProgressCreateParams {
    // The token to be used to report progress.
    // ProgressToken
    let token: Object = null;
    new() { }
}

def WorkDoneProgressCancelParams {
    // The token to be used to report progress.
    // ProgressToken
    let token: Object = null;
    new() { }
}

// The parameter of a `textDocument/prepareCallHierarchy` request.
//
// @since 3.16.0
def CallHierarchyPrepareParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

// Represents programming constructs like functions or constructors in the context
// of call hierarchy.
//
// @since 3.16.0
def CallHierarchyItem {
    // The name of this item.
    // string
    let name: String = null;
    // The kind of this item.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this item.
    // SymbolTag[]
    let tags: List[Int32] = null;
    // More detail for this item, e.g. the signature of a function.
    // string
    let detail: String = null;
    // The resource identifier of this item.
    // DocumentUri
    let uri: String = null;
    // The range enclosing this symbol not including leading/trailing whitespace but everything else, e.g. comments and code.
    let range: Range = null;
    // The range that should be selected and revealed when this symbol is being picked, e.g. the name of a function.
    // Must be contained by the {@link CallHierarchyItem.range `range`}.
    let selectionRange: Range = null;
    // A data entry field that is preserved between a call hierarchy prepare and
    // incoming calls or outgoing calls requests.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Call hierarchy options used during static or dynamic registration.
//
// @since 3.16.0
def CallHierarchyRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// The parameter of a `callHierarchy/incomingCalls` request.
//
// @since 3.16.0
def CallHierarchyIncomingCallsParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    let item: CallHierarchyItem = null;
    new() { }
}

// Represents an incoming call, e.g. a caller of a method or constructor.
//
// @since 3.16.0
def CallHierarchyIncomingCall {
    // The item that makes the call.
    let from_: CallHierarchyItem = null;
    // The ranges at which the calls appear. This is relative to the caller
    // denoted by {@link CallHierarchyIncomingCall.from `this.from`}.
    // Range[]
    let fromRanges: List[Range] = null;
    new() { }
}

// The parameter of a `callHierarchy/outgoingCalls` request.
//
// @since 3.16.0
def CallHierarchyOutgoingCallsParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    let item: CallHierarchyItem = null;
    new() { }
}

// Represents an outgoing call, e.g. calling a getter from a method or a method from a constructor etc.
//
// @since 3.16.0
def CallHierarchyOutgoingCall {
    // The item that is called.
    let to: CallHierarchyItem = null;
    // The range at which this item is called. This is the range relative to the caller, e.g the item
    // passed to {@link CallHierarchyItemProvider.provideCallHierarchyOutgoingCalls `provideCallHierarchyOutgoingCalls`}
    // and not {@link CallHierarchyOutgoingCall.to `this.to`}.
    // Range[]
    let fromRanges: List[Range] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// @since 3.16.0
def SemanticTokens {
    // An optional result id. If provided and clients support delta updating
    // the client will include the result id in the next semantic token request.
    // A server can then instead of computing all semantic tokens again simply
    // send a delta.
    // string
    let resultId: String = null;
    // The actual tokens.
    // uinteger[]
    let data: List[Int32] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensPartialResult {
    // uinteger[]
    let data: List[Int32] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The legend used by the server
    let legend: SemanticTokensLegend = null;
    // Server supports providing semantic tokens for a specific range
    // of a document.
    // boolean | { ... }
    let range: Object = null;
    // Server supports providing semantic tokens for a full document.
    // boolean | { ... }
    let full: Object = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensDeltaParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The result id of a previous response. The result Id can either point to a full response
    // or a delta response depending on what was received last.
    // string
    let previousResultId: String = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensDelta {
    // string
    let resultId: String = null;
    // The semantic token edits to transform a previous result into a new result.
    // SemanticTokensEdit[]
    let edits: List[SemanticTokensEdit] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensDeltaPartialResult {
    // SemanticTokensEdit[]
    let edits: List[SemanticTokensEdit] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensRangeParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The range the semantic tokens are requested for.
    let range: Range = null;
    new() { }
}

// Params to show a resource in the UI.
//
// @since 3.16.0
def ShowDocumentParams {
    // The uri to show.
    // URI
    let uri: String = null;
    // Indicates to show the resource in an external program.
    // To show, for example, `https://code.visualstudio.com/`
    // in the default WEB browser set `external` to `true`.
    // boolean
    let external: Nullable[Boolean] = null;
    // An optional property to indicate whether the editor
    // showing the document should take focus or not.
    // Clients might ignore this property if an external
    // program is started.
    // boolean
    let takeFocus: Nullable[Boolean] = null;
    // An optional selection range if the document is a text
    // document. Clients might ignore the property if an
    // external program is started or the file is not a text
    // file.
    let selection: Range = null;
    new() { }
}

// The result of a showDocument request.
//
// @since 3.16.0
def ShowDocumentResult {
    // A boolean indicating if the show was successful.
    // boolean
    let success: Boolean = false;
    new() { }
}

def LinkedEditingRangeParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

// The result of a linked editing range request.
//
// @since 3.16.0
def LinkedEditingRanges {
    // A list of ranges that can be edited together. The ranges must have
    // identical length and contain identical text content. The ranges cannot overlap.
    // Range[]
    let ranges: List[Range] = null;
    // An optional word pattern (regular expression) that describes valid contents for
    // the given ranges. If no pattern is provided, the client configuration's word
    // pattern will be used.
    // string
    let wordPattern: String = null;
    new() { }
}

def LinkedEditingRangeRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// The parameters sent in notifications/requests for user-initiated creation of
// files.
//
// @since 3.16.0
def CreateFilesParams {
    // An array of all files/folders created in this operation.
    // FileCreate[]
    let files: List[FileCreate] = null;
    new() { }
}

// A workspace edit represents changes to many resources managed in the workspace. The edit
// should either provide `changes` or `documentChanges`. If documentChanges are present
// they are preferred over `changes` if the client can handle versioned document edits.
//
// Since version 3.13.0 a workspace edit can contain resource operations as well. If resource
// operations are present clients need to execute the operations in the order in which they
// are provided. So a workspace edit for example can consist of the following two changes:
// (1) a create file a.txt and (2) a text document edit which insert text into file a.txt.
//
// An invalid sequence (e.g. (1) delete file a.txt and (2) insert text into file a.txt) will
// cause failure of the operation. How the client recovers from the failure is described by
// the client capability: `workspace.workspaceEdit.failureHandling`
def WorkspaceEdit {
    // Holds changes to existing resources.
    // { [key: DocumentUri]: TextEdit[] }
    let changes: Dictionary[String, List[TextEdit]] = null;
    // Depending on the client capability `workspace.workspaceEdit.resourceOperations` document changes
    // are either an array of `TextDocumentEdit`s to express changes to n different text documents
    // where each text document edit addresses a specific version of a text document. Or it can contain
    // above `TextDocumentEdit`s mixed with create, rename and delete file / folder operations.
    //
    // Whether a client supports versioned document edits is expressed via
    // `workspace.workspaceEdit.documentChanges` client capability.
    //
    // If a client neither supports `documentChanges` nor `workspace.workspaceEdit.resourceOperations` then
    // only plain `TextEdit`s using the `changes` property are supported.
    // (TextDocumentEdit | CreateFile | RenameFile | DeleteFile)[]
    let documentChanges: List[Object] = null;
    // A map of change annotations that can be referenced in `AnnotatedTextEdit`s or create, rename and
    // delete file / folder operations.
    //
    // Whether clients honor this property depends on the client capability `workspace.changeAnnotationSupport`.
    //
    // @since 3.16.0
    // { [key: ChangeAnnotationIdentifier]: ChangeAnnotation }
    let changeAnnotations: Dictionary[String, ChangeAnnotation] = null;
    new() { }
}

// The options to register for file operations.
//
// @since 3.16.0
def FileOperationRegistrationOptions {
    // The actual filters.
    // FileOperationFilter[]
    let filters: List[FileOperationFilter] = null;
    new() { }
}

// The parameters sent in notifications/requests for user-initiated renames of
// files.
//
// @since 3.16.0
def RenameFilesParams {
    // An array of all files/folders renamed in this operation. When a folder is renamed, only
    // the folder will be included, and not its children.
    // FileRename[]
    let files: List[FileRename] = null;
    new() { }
}

// The parameters sent in notifications/requests for user-initiated deletes of
// files.
//
// @since 3.16.0
def DeleteFilesParams {
    // An array of all files/folders deleted in this operation.
    // FileDelete[]
    let files: List[FileDelete] = null;
    new() { }
}

def MonikerParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

// Moniker definition to match LSIF 0.5 moniker definition.
//
// @since 3.16.0
def Moniker {
    // The scheme of the moniker. For example tsc or .Net
    // string
    let scheme: String = null;
    // The identifier of the moniker. The value is opaque in LSIF however
    // schema owners are allowed to define the structure if they want.
    // string
    let identifier: String = null;
    // The scope in which the moniker is unique
    // UniquenessLevel
    let unique: String = null;
    // The moniker kind if known.
    // MonikerKind
    let kind: String = null;
    new() { }
}

def MonikerRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// The parameter of a `textDocument/prepareTypeHierarchy` request.
//
// @since 3.17.0
def TypeHierarchyPrepareParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

// @since 3.17.0
def TypeHierarchyItem {
    // The name of this item.
    // string
    let name: String = null;
    // The kind of this item.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this item.
    // SymbolTag[]
    let tags: List[Int32] = null;
    // More detail for this item, e.g. the signature of a function.
    // string
    let detail: String = null;
    // The resource identifier of this item.
    // DocumentUri
    let uri: String = null;
    // The range enclosing this symbol not including leading/trailing whitespace
    // but everything else, e.g. comments and code.
    let range: Range = null;
    // The range that should be selected and revealed when this symbol is being
    // picked, e.g. the name of a function. Must be contained by the
    // {@link TypeHierarchyItem.range `range`}.
    let selectionRange: Range = null;
    // A data entry field that is preserved between a type hierarchy prepare and
    // supertypes or subtypes requests. It could also be used to identify the
    // type hierarchy in the server, helping improve the performance on
    // resolving supertypes and subtypes.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Type hierarchy options used during static or dynamic registration.
//
// @since 3.17.0
def TypeHierarchyRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// The parameter of a `typeHierarchy/supertypes` request.
//
// @since 3.17.0
def TypeHierarchySupertypesParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    let item: TypeHierarchyItem = null;
    new() { }
}

// The parameter of a `typeHierarchy/subtypes` request.
//
// @since 3.17.0
def TypeHierarchySubtypesParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    let item: TypeHierarchyItem = null;
    new() { }
}

// A parameter literal used in inline value requests.
//
// @since 3.17.0
def InlineValueParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The document range for which inline values should be computed.
    let range: Range = null;
    // Additional information about the context in which inline values were
    // requested.
    let context: InlineValueContext = null;
    new() { }
}

// Inline value options used during static or dynamic registration.
//
// @since 3.17.0
def InlineValueRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// A parameter literal used in inlay hint requests.
//
// @since 3.17.0
def InlayHintParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The document range for which inlay hints should be computed.
    let range: Range = null;
    new() { }
}

// Inlay hint information.
//
// @since 3.17.0
def InlayHint {
    // The position of this hint.
    //
    // If multiple hints have the same position, they will be shown in the order
    // they appear in the response.
    let position: Position = null;
    // The label of this hint. A human readable string or an array of
    // InlayHintLabelPart label parts.
    //
    // *Note* that neither the string nor the label part can be empty.
    // string | InlayHintLabelPart[]
    let label: Object = null;
    // The kind of this hint. Can be omitted in which case the client
    // should fall back to a reasonable default.
    // InlayHintKind
    let kind: Nullable[Int32] = null;
    // Optional text edits that are performed when accepting this inlay hint.
    //
    // *Note* that edits are expected to change the document so that the inlay
    // hint (or its nearest variant) is now part of the document and the inlay
    // hint itself is now obsolete.
    // TextEdit[]
    let textEdits: List[TextEdit] = null;
    // The tooltip text when you hover over this item.
    // string | MarkupContent
    let tooltip: Object = null;
    // Render padding before the hint.
    //
    // Note: Padding should use the editor's background color, not the
    // background color of the hint itself. That means padding can be used
    // to visually align/separate an inlay hint.
    // boolean
    let paddingLeft: Nullable[Boolean] = null;
    // Render padding after the hint.
    //
    // Note: Padding should use the editor's background color, not the
    // background color of the hint itself. That means padding can be used
    // to visually align/separate an inlay hint.
    // boolean
    let paddingRight: Nullable[Boolean] = null;
    // A data entry field that is preserved on an inlay hint between
    // a `textDocument/inlayHint` and a `inlayHint/resolve` request.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Inlay hint options used during static or dynamic registration.
//
// @since 3.17.0
def InlayHintRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The server provides support to resolve additional
    // information for an inlay hint item.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// Parameters of the document diagnostic request.
//
// @since 3.17.0
def DocumentDiagnosticParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The additional identifier  provided during registration.
    // string
    let identifier: String = null;
    // The result id of a previous response if provided.
    // string
    let previousResultId: String = null;
    new() { }
}

// A partial result for a document diagnostic report.
//
// @since 3.17.0
def DocumentDiagnosticReportPartialResult {
    // { [key: DocumentUri]: FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport }
    let relatedDocuments: Dictionary[String, Object] = null;
    new() { }
}

// Cancellation data returned from a diagnostic request.
//
// @since 3.17.0
def DiagnosticServerCancellationData {
    // boolean
    let retriggerRequest: Boolean = false;
    new() { }
}

// Diagnostic registration options.
//
// @since 3.17.0
def DiagnosticRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // An optional identifier under which the diagnostics are
    // managed by the client.
    // string
    let identifier: String = null;
    // Whether the language has inter file dependencies meaning that
    // editing code in one file can result in a different diagnostic
    // set in another file. Inter file dependencies are common for
    // most programming languages and typically uncommon for linters.
    // boolean
    let interFileDependencies: Boolean = false;
    // The server provides support for workspace diagnostics as well.
    // boolean
    let workspaceDiagnostics: Boolean = false;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

// Parameters of the workspace diagnostic request.
//
// @since 3.17.0
def WorkspaceDiagnosticParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The additional identifier provided during registration.
    // string
    let identifier: String = null;
    // The currently known diagnostic reports with their
    // previous result ids.
    // PreviousResultId[]
    let previousResultIds: List[PreviousResultId] = null;
    new() { }
}

// A workspace diagnostic report.
//
// @since 3.17.0
def WorkspaceDiagnosticReport {
    // WorkspaceDocumentDiagnosticReport[]
    let items: List[Object] = null;
    new() { }
}

// A partial result for a workspace diagnostic report.
//
// @since 3.17.0
def WorkspaceDiagnosticReportPartialResult {
    // WorkspaceDocumentDiagnosticReport[]
    let items: List[Object] = null;
    new() { }
}

// The params sent in an open notebook document notification.
//
// @since 3.17.0
def DidOpenNotebookDocumentParams {
    // The notebook document that got opened.
    let notebookDocument: NotebookDocument = null;
    // The text documents that represent the content
    // of a notebook cell.
    // TextDocumentItem[]
    let cellTextDocuments: List[TextDocumentItem] = null;
    new() { }
}

// The params sent in a change notebook document notification.
//
// @since 3.17.0
def DidChangeNotebookDocumentParams {
    // The notebook document that did change. The version number points
    // to the version after all provided changes have been applied. If
    // only the text document content of a cell changes the notebook version
    // doesn't necessarily have to change.
    let notebookDocument: VersionedNotebookDocumentIdentifier = null;
    // The actual changes to the notebook document.
    //
    // The changes describe single state changes to the notebook document.
    // So if there are two changes c1 (at array index 0) and c2 (at array
    // index 1) for a notebook in state S then c1 moves the notebook from
    // S to S' and c2 from S' to S''. So c1 is computed on the state S and
    // c2 is computed on the state S'.
    //
    // To mirror the content of a notebook using change events use the following approach:
    // - start with the same initial content
    // - apply the 'notebookDocument/didChange' notifications in the order you receive them.
    // - apply the `NotebookChangeEvent`s in a single notification in the order
    //   you receive them.
    let change: NotebookDocumentChangeEvent = null;
    new() { }
}

// The params sent in a save notebook document notification.
//
// @since 3.17.0
def DidSaveNotebookDocumentParams {
    // The notebook document that got saved.
    let notebookDocument: NotebookDocumentIdentifier = null;
    new() { }
}

// The params sent in a close notebook document notification.
//
// @since 3.17.0
def DidCloseNotebookDocumentParams {
    // The notebook document that got closed.
    let notebookDocument: NotebookDocumentIdentifier = null;
    // The text documents that represent the content
    // of a notebook cell that got closed.
    // TextDocumentIdentifier[]
    let cellTextDocuments: List[TextDocumentIdentifier] = null;
    new() { }
}

// A parameter literal used in inline completion requests.
//
// @since 3.18.0
// @proposed
def InlineCompletionParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // Additional information about the context in which inline completions were
    // requested.
    let context: InlineCompletionContext = null;
    new() { }
}

// Represents a collection of {@link InlineCompletionItem inline completion items} to be presented in the editor.
//
// @since 3.18.0
// @proposed
def InlineCompletionList {
    // The inline completion items
    // InlineCompletionItem[]
    let items: List[InlineCompletionItem] = null;
    new() { }
}

// An inline completion item represents a text snippet that is proposed inline to complete text that is being typed.
//
// @since 3.18.0
// @proposed
def InlineCompletionItem {
    // The text to replace the range with. Must be set.
    // string | StringValue
    let insertText: Object = null;
    // A text that is used to decide if this inline completion should be shown. When `falsy` the {@link InlineCompletionItem.insertText} is used.
    // string
    let filterText: String = null;
    // The range to replace. Must begin and end on the same line.
    let range: Range = null;
    // An optional {@link Command} that is executed *after* inserting this completion.
    let command: Command = null;
    new() { }
}

// Inline completion options used during static or dynamic registration.
//
// @since 3.18.0
// @proposed
def InlineCompletionRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def RegistrationParams {
    // Registration[]
    let registrations: List[Registration] = null;
    new() { }
}

def UnregistrationParams {
    // Unregistration[]
    let unregisterations: List[Unregistration] = null;
    new() { }
}

def InitializeParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The process Id of the parent process that started
    // the server.
    //
    // Is `null` if the process has not been started by another process.
    // If the parent process is not alive then the server should exit.
    // integer | null
    let processId: Nullable[Int32] = null;
    // Information about the client
    //
    // @since 3.15.0
    // { ... }
    let clientInfo: InitializeParamsClientInfo = null;
    // The locale the client is currently showing the user interface
    // in. This must not necessarily be the locale of the operating
    // system.
    //
    // Uses IETF language tags as the value's syntax
    // (See https://en.wikipedia.org/wiki/IETF_language_tag)
    //
    // @since 3.16.0
    // string
    let locale: String = null;
    // The rootPath of the workspace. Is null
    // if no folder is open.
    //
    // @deprecated in favour of rootUri.
    // string | null
    let rootPath: String = null;
    // The rootUri of the workspace. Is null if no
    // folder is open. If both `rootPath` and `rootUri` are set
    // `rootUri` wins.
    //
    // @deprecated in favour of workspaceFolders.
    // DocumentUri | null
    let rootUri: String = null;
    // The capabilities provided by the client (editor or tool)
    let capabilities: ClientCapabilities = null;
    // User provided initialization options.
    // LSPAny
    let initializationOptions: Object = null;
    // The initial trace setting. If omitted trace is disabled ('off').
    // TraceValues
    let trace: String = null;
    // The workspace folders configured in the client when the server starts.
    //
    // This property is only available if the client supports workspace folders.
    // It can be `null` if the client supports workspace folders but none are
    // configured.
    //
    // @since 3.6.0
    // WorkspaceFolder[] | null
    let workspaceFolders: List[WorkspaceFolder] = null;
    new() { }
}

// The result returned from an initialize request.
def InitializeResult {
    // The capabilities the language server provides.
    let capabilities: ServerCapabilities = null;
    // Information about the server.
    //
    // @since 3.15.0
    // { ... }
    let serverInfo: InitializeResultServerInfo = null;
    new() { }
}

// The data type of the ResponseError if the
// initialize request fails.
def InitializeError {
    // Indicates whether the client execute the following retry logic:
    // (1) show the message provided by the ResponseError to the user
    // (2) user selects retry or cancel
    // (3) if user selected retry the initialize method is sent again.
    // boolean
    let retry: Boolean = false;
    new() { }
}

def InitializedParams {
    new() { }
}

// The parameters of a change configuration notification.
def DidChangeConfigurationParams {
    // The actual changed settings
    // LSPAny
    let settings: Object = null;
    new() { }
}

def DidChangeConfigurationRegistrationOptions {
    // string | string[]
    let section: Object = null;
    new() { }
}

// The parameters of a notification message.
def ShowMessageParams {
    // The message type. See {@link MessageType}
    // MessageType
    let type: Int32 = 0;
    // The actual message.
    // string
    let message: String = null;
    new() { }
}

def ShowMessageRequestParams {
    // The message type. See {@link MessageType}
    // MessageType
    let type: Int32 = 0;
    // The actual message.
    // string
    let message: String = null;
    // The message action items to present.
    // MessageActionItem[]
    let actions: List[MessageActionItem] = null;
    new() { }
}

def MessageActionItem {
    // A short title like 'Retry', 'Open Log' etc.
    // string
    let title: String = null;
    new() { }
}

// The log message parameters.
def LogMessageParams {
    // The message type. See {@link MessageType}
    // MessageType
    let type: Int32 = 0;
    // The actual message.
    // string
    let message: String = null;
    new() { }
}

// The parameters sent in an open text document notification
def DidOpenTextDocumentParams {
    // The document that was opened.
    let textDocument: TextDocumentItem = null;
    new() { }
}

// The change text document notification's parameters.
def DidChangeTextDocumentParams {
    // The document that did change. The version number points
    // to the version after all provided content changes have
    // been applied.
    let textDocument: VersionedTextDocumentIdentifier = null;
    // The actual content changes. The content changes describe single state changes
    // to the document. So if there are two content changes c1 (at array index 0) and
    // c2 (at array index 1) for a document in state S then c1 moves the document from
    // S to S' and c2 from S' to S''. So c1 is computed on the state S and c2 is computed
    // on the state S'.
    //
    // To mirror the content of a document using change events use the following approach:
    // - start with the same initial content
    // - apply the 'textDocument/didChange' notifications in the order you receive them.
    // - apply the `TextDocumentContentChangeEvent`s in a single notification in the order
    //   you receive them.
    // TextDocumentContentChangeEvent[]
    let contentChanges: List[Object] = null;
    new() { }
}

// Describe options to be used when registered for text document change events.
def TextDocumentChangeRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // How documents are synced to the server.
    // TextDocumentSyncKind
    let syncKind: Int32 = 0;
    new() { }
}

// The parameters sent in a close text document notification
def DidCloseTextDocumentParams {
    // The document that was closed.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// The parameters sent in a save text document notification
def DidSaveTextDocumentParams {
    // The document that was saved.
    let textDocument: TextDocumentIdentifier = null;
    // Optional the content when saved. Depends on the includeText value
    // when the save notification was requested.
    // string
    let text: String = null;
    new() { }
}

// Save registration options.
def TextDocumentSaveRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // The client is supposed to include the content on save.
    // boolean
    let includeText: Nullable[Boolean] = null;
    new() { }
}

// The parameters sent in a will save text document notification.
def WillSaveTextDocumentParams {
    // The document that will be saved.
    let textDocument: TextDocumentIdentifier = null;
    // The 'TextDocumentSaveReason'.
    // TextDocumentSaveReason
    let reason: Int32 = 0;
    new() { }
}

// A text edit applicable to a text document.
def TextEdit {
    // The range of the text document to be manipulated. To insert
    // text into a document create a range where start === end.
    let range: Range = null;
    // The string to be inserted. For delete operations use an
    // empty string.
    // string
    let newText: String = null;
    new() { }
}

// The watched files change notification's parameters.
def DidChangeWatchedFilesParams {
    // The actual file events.
    // FileEvent[]
    let changes: List[FileEvent] = null;
    new() { }
}

// Describe options to be used when registered for text document change events.
def DidChangeWatchedFilesRegistrationOptions {
    // The watchers to register.
    // FileSystemWatcher[]
    let watchers: List[FileSystemWatcher] = null;
    new() { }
}

// The publish diagnostic notification's parameters.
def PublishDiagnosticsParams {
    // The URI for which diagnostic information is reported.
    // DocumentUri
    let uri: String = null;
    // Optional the version number of the document the diagnostics are published for.
    //
    // @since 3.15.0
    // integer
    let version: Nullable[Int32] = null;
    // An array of diagnostic information items.
    // Diagnostic[]
    let diagnostics: List[Diagnostic] = null;
    new() { }
}

// Completion parameters
def CompletionParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The completion context. This is only available if the client specifies
    // to send this using the client capability `textDocument.completion.contextSupport === true`
    let context: CompletionContext = null;
    new() { }
}

// A completion item represents a text snippet that is
// proposed to complete text that is being typed.
def CompletionItem {
    // The label of this completion item.
    //
    // The label property is also by default the text that
    // is inserted when selecting this completion.
    //
    // If label details are provided the label itself should
    // be an unqualified name of the completion item.
    // string
    let label: String = null;
    // Additional details for the label
    //
    // @since 3.17.0
    let labelDetails: CompletionItemLabelDetails = null;
    // The kind of this completion item. Based of the kind
    // an icon is chosen by the editor.
    // CompletionItemKind
    let kind: Nullable[Int32] = null;
    // Tags for this completion item.
    //
    // @since 3.15.0
    // CompletionItemTag[]
    let tags: List[Int32] = null;
    // A human-readable string with additional information
    // about this item, like type or symbol information.
    // string
    let detail: String = null;
    // A human-readable string that represents a doc-comment.
    // string | MarkupContent
    let documentation: Object = null;
    // Indicates if this item is deprecated.
    // @deprecated Use `tags` instead.
    // boolean
    let deprecated: Nullable[Boolean] = null;
    // Select this item when showing.
    //
    // *Note* that only one completion item can be selected and that the
    // tool / client decides which item that is. The rule is that the *first*
    // item of those that match best is selected.
    // boolean
    let preselect: Nullable[Boolean] = null;
    // A string that should be used when comparing this item
    // with other items. When `falsy` the {@link CompletionItem.label label}
    // is used.
    // string
    let sortText: String = null;
    // A string that should be used when filtering a set of
    // completion items. When `falsy` the {@link CompletionItem.label label}
    // is used.
    // string
    let filterText: String = null;
    // A string that should be inserted into a document when selecting
    // this completion. When `falsy` the {@link CompletionItem.label label}
    // is used.
    //
    // The `insertText` is subject to interpretation by the client side.
    // Some tools might not take the string literally. For example
    // VS Code when code complete is requested in this example
    // `con<cursor position>` and a completion item with an `insertText` of
    // `console` is provided it will only insert `sole`. Therefore it is
    // recommended to use `textEdit` instead since it avoids additional client
    // side interpretation.
    // string
    let insertText: String = null;
    // The format of the insert text. The format applies to both the
    // `insertText` property and the `newText` property of a provided
    // `textEdit`. If omitted defaults to `InsertTextFormat.PlainText`.
    //
    // Please note that the insertTextFormat doesn't apply to
    // `additionalTextEdits`.
    // InsertTextFormat
    let insertTextFormat: Nullable[Int32] = null;
    // How whitespace and indentation is handled during completion
    // item insertion. If not provided the clients default value depends on
    // the `textDocument.completion.insertTextMode` client capability.
    //
    // @since 3.16.0
    // InsertTextMode
    let insertTextMode: Nullable[Int32] = null;
    // An {@link TextEdit edit} which is applied to a document when selecting
    // this completion. When an edit is provided the value of
    // {@link CompletionItem.insertText insertText} is ignored.
    //
    // Most editors support two different operations when accepting a completion
    // item. One is to insert a completion text and the other is to replace an
    // existing text with a completion text. Since this can usually not be
    // predetermined by a server it can report both ranges. Clients need to
    // signal support for `InsertReplaceEdits` via the
    // `textDocument.completion.insertReplaceSupport` client capability
    // property.
    //
    // *Note 1:* The text edit's range as well as both ranges from an insert
    // replace edit must be a [single line] and they must contain the position
    // at which completion has been requested.
    // *Note 2:* If an `InsertReplaceEdit` is returned the edit's insert range
    // must be a prefix of the edit's replace range, that means it must be
    // contained and starting at the same position.
    //
    // @since 3.16.0 additional type `InsertReplaceEdit`
    // TextEdit | InsertReplaceEdit
    let textEdit: Object = null;
    // The edit text used if the completion item is part of a CompletionList and
    // CompletionList defines an item default for the text edit range.
    //
    // Clients will only honor this property if they opt into completion list
    // item defaults using the capability `completionList.itemDefaults`.
    //
    // If not provided and a list's default range is provided the label
    // property is used as a text.
    //
    // @since 3.17.0
    // string
    let textEditText: String = null;
    // An optional array of additional {@link TextEdit text edits} that are applied when
    // selecting this completion. Edits must not overlap (including the same insert position)
    // with the main {@link CompletionItem.textEdit edit} nor with themselves.
    //
    // Additional text edits should be used to change text unrelated to the current cursor position
    // (for example adding an import statement at the top of the file if the completion item will
    // insert an unqualified type).
    // TextEdit[]
    let additionalTextEdits: List[TextEdit] = null;
    // An optional set of characters that when pressed while this completion is active will accept it first and
    // then type that character. *Note* that all commit characters should have `length=1` and that superfluous
    // characters will be ignored.
    // string[]
    let commitCharacters: List[String] = null;
    // An optional {@link Command command} that is executed *after* inserting this completion. *Note* that
    // additional modifications to the current document should be described with the
    // {@link CompletionItem.additionalTextEdits additionalTextEdits}-property.
    let command: Command = null;
    // A data entry field that is preserved on a completion item between a
    // {@link CompletionRequest} and a {@link CompletionResolveRequest}.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Represents a collection of {@link CompletionItem completion items} to be presented
// in the editor.
def CompletionList {
    // This list it not complete. Further typing results in recomputing this list.
    //
    // Recomputed lists have all their items replaced (not appended) in the
    // incomplete completion sessions.
    // boolean
    let isIncomplete: Boolean = false;
    // In many cases the items of an actual completion result share the same
    // value for properties like `commitCharacters` or the range of a text
    // edit. A completion list can therefore define item defaults which will
    // be used if a completion item itself doesn't specify the value.
    //
    // If a completion list specifies a default value and a completion item
    // also specifies a corresponding value the one from the item is used.
    //
    // Servers are only allowed to return default values if the client
    // signals support for this via the `completionList.itemDefaults`
    // capability.
    //
    // @since 3.17.0
    // { ... }
    let itemDefaults: CompletionListItemDefaults = null;
    // The completion items.
    // CompletionItem[]
    let items: List[CompletionItem] = null;
    new() { }
}

// Registration options for a {@link CompletionRequest}.
def CompletionRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Most tools trigger completion request automatically without explicitly requesting
    // it using a keyboard shortcut (e.g. Ctrl+Space). Typically they do so when the user
    // starts to type an identifier. For example if the user types `c` in a JavaScript file
    // code complete will automatically pop up present `console` besides others as a
    // completion item. Characters that make up identifiers don't need to be listed here.
    //
    // If code complete should automatically be trigger on characters not being valid inside
    // an identifier (for example `.` in JavaScript) list them in `triggerCharacters`.
    // string[]
    let triggerCharacters: List[String] = null;
    // The list of all possible characters that commit a completion. This field can be used
    // if clients don't support individual commit characters per completion item. See
    // `ClientCapabilities.textDocument.completion.completionItem.commitCharactersSupport`
    //
    // If a server provides both `allCommitCharacters` and commit characters on an individual
    // completion item the ones on the completion item win.
    //
    // @since 3.2.0
    // string[]
    let allCommitCharacters: List[String] = null;
    // The server provides support to resolve additional
    // information for a completion item.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    // The server supports the following `CompletionItem` specific
    // capabilities.
    //
    // @since 3.17.0
    // { ... }
    let completionItem: CompletionRegistrationOptionsCompletionItem = null;
    new() { }
}

// Parameters for a {@link HoverRequest}.
def HoverParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

// The result of a hover request.
def Hover {
    // The hover's content
    // MarkupContent | MarkedString | MarkedString[]
    let contents: Object = null;
    // An optional range inside the text document that is used to
    // visualize the hover, e.g. by changing the background color.
    let range: Range = null;
    new() { }
}

// Registration options for a {@link HoverRequest}.
def HoverRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Parameters for a {@link SignatureHelpRequest}.
def SignatureHelpParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The signature help context. This is only available if the client specifies
    // to send this using the client capability `textDocument.signatureHelp.contextSupport === true`
    //
    // @since 3.15.0
    let context: SignatureHelpContext = null;
    new() { }
}

// Signature help represents the signature of something
// callable. There can be multiple signature but only one
// active and only one active parameter.
def SignatureHelp {
    // One or more signatures.
    // SignatureInformation[]
    let signatures: List[SignatureInformation] = null;
    // The active signature. If omitted or the value lies outside the
    // range of `signatures` the value defaults to zero or is ignored if
    // the `SignatureHelp` has no signatures.
    //
    // Whenever possible implementors should make an active decision about
    // the active signature and shouldn't rely on a default value.
    //
    // In future version of the protocol this property might become
    // mandatory to better express this.
    // uinteger
    let activeSignature: Nullable[Int32] = null;
    // The active parameter of the active signature. If omitted or the value
    // lies outside the range of `signatures[activeSignature].parameters`
    // defaults to 0 if the active signature has parameters. If
    // the active signature has no parameters it is ignored.
    // In future version of the protocol this property might become
    // mandatory to better express the active parameter if the
    // active signature does have any.
    // uinteger
    let activeParameter: Nullable[Int32] = null;
    new() { }
}

// Registration options for a {@link SignatureHelpRequest}.
def SignatureHelpRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // List of characters that trigger signature help automatically.
    // string[]
    let triggerCharacters: List[String] = null;
    // List of characters that re-trigger signature help.
    //
    // These trigger characters are only active when signature help is already showing. All trigger characters
    // are also counted as re-trigger characters.
    //
    // @since 3.15.0
    // string[]
    let retriggerCharacters: List[String] = null;
    new() { }
}

// Parameters for a {@link DefinitionRequest}.
def DefinitionParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

// Registration options for a {@link DefinitionRequest}.
def DefinitionRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Parameters for a {@link ReferencesRequest}.
def ReferenceParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    let context: ReferenceContext = null;
    new() { }
}

// Registration options for a {@link ReferencesRequest}.
def ReferenceRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Parameters for a {@link DocumentHighlightRequest}.
def DocumentHighlightParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

// A document highlight is a range inside a text document which deserves
// special attention. Usually a document highlight is visualized by changing
// the background color of its range.
def DocumentHighlight {
    // The range this highlight applies to.
    let range: Range = null;
    // The highlight kind, default is {@link DocumentHighlightKind.Text text}.
    // DocumentHighlightKind
    let kind: Nullable[Int32] = null;
    new() { }
}

// Registration options for a {@link DocumentHighlightRequest}.
def DocumentHighlightRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Parameters for a {@link DocumentSymbolRequest}.
def DocumentSymbolParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// Represents information about programming constructs like variables, classes,
// interfaces etc.
def SymbolInformation {
    // The name of this symbol.
    // string
    let name: String = null;
    // The kind of this symbol.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this symbol.
    //
    // @since 3.16.0
    // SymbolTag[]
    let tags: List[Int32] = null;
    // The name of the symbol containing this symbol. This information is for
    // user interface purposes (e.g. to render a qualifier in the user interface
    // if necessary). It can't be used to re-infer a hierarchy for the document
    // symbols.
    // string
    let containerName: String = null;
    // Indicates if this symbol is deprecated.
    //
    // @deprecated Use tags instead
    // boolean
    let deprecated: Nullable[Boolean] = null;
    // The location of this symbol. The location's range is used by a tool
    // to reveal the location in the editor. If the symbol is selected in the
    // tool the range's start information is used to position the cursor. So
    // the range usually spans more than the actual symbol's name and does
    // normally include things like visibility modifiers.
    //
    // The range doesn't have to denote a node range in the sense of an abstract
    // syntax tree. It can therefore not be used to re-construct a hierarchy of
    // the symbols.
    let location: Location = null;
    new() { }
}

// Represents programming constructs like variables, classes, interfaces etc.
// that appear in a document. Document symbols can be hierarchical and they
// have two ranges: one that encloses its definition and one that points to
// its most interesting range, e.g. the range of an identifier.
def DocumentSymbol {
    // The name of this symbol. Will be displayed in the user interface and therefore must not be
    // an empty string or a string only consisting of white spaces.
    // string
    let name: String = null;
    // More detail for this symbol, e.g the signature of a function.
    // string
    let detail: String = null;
    // The kind of this symbol.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this document symbol.
    //
    // @since 3.16.0
    // SymbolTag[]
    let tags: List[Int32] = null;
    // Indicates if this symbol is deprecated.
    //
    // @deprecated Use tags instead
    // boolean
    let deprecated: Nullable[Boolean] = null;
    // The range enclosing this symbol not including leading/trailing whitespace but everything else
    // like comments. This information is typically used to determine if the clients cursor is
    // inside the symbol to reveal in the symbol in the UI.
    let range: Range = null;
    // The range that should be selected and revealed when this symbol is being picked, e.g the name of a function.
    // Must be contained by the `range`.
    let selectionRange: Range = null;
    // Children of this symbol, e.g. properties of a class.
    // DocumentSymbol[]
    let children: List[DocumentSymbol] = null;
    new() { }
}

// Registration options for a {@link DocumentSymbolRequest}.
def DocumentSymbolRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A human-readable string that is shown when multiple outlines trees
    // are shown for the same document.
    //
    // @since 3.16.0
    // string
    let label: String = null;
    new() { }
}

// The parameters of a {@link CodeActionRequest}.
def CodeActionParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The document in which the command was invoked.
    let textDocument: TextDocumentIdentifier = null;
    // The range for which the command was invoked.
    let range: Range = null;
    // Context carrying additional information.
    let context: CodeActionContext = null;
    new() { }
}

// Represents a reference to a command. Provides a title which
// will be used to represent a command in the UI and, optionally,
// an array of arguments which will be passed to the command handler
// function when invoked.
def Command {
    // Title of the command, like `save`.
    // string
    let title: String = null;
    // The identifier of the actual command handler.
    // string
    let command: String = null;
    // Arguments that the command handler should be
    // invoked with.
    // LSPAny[]
    let arguments: List[Object] = null;
    new() { }
}

// A code action represents a change that can be performed in code, e.g. to fix a problem or
// to refactor code.
//
// A CodeAction must set either `edit` and/or a `command`. If both are supplied, the `edit` is applied first, then the `command` is executed.
def CodeAction {
    // A short, human-readable, title for this code action.
    // string
    let title: String = null;
    // The kind of the code action.
    //
    // Used to filter code actions.
    // CodeActionKind
    let kind: String = null;
    // The diagnostics that this code action resolves.
    // Diagnostic[]
    let diagnostics: List[Diagnostic] = null;
    // Marks this as a preferred action. Preferred actions are used by the `auto fix` command and can be targeted
    // by keybindings.
    //
    // A quick fix should be marked preferred if it properly addresses the underlying error.
    // A refactoring should be marked preferred if it is the most reasonable choice of actions to take.
    //
    // @since 3.15.0
    // boolean
    let isPreferred: Nullable[Boolean] = null;
    // Marks that the code action cannot currently be applied.
    //
    // Clients should follow the following guidelines regarding disabled code actions:
    //
    //   - Disabled code actions are not shown in automatic [lightbulbs](https://code.visualstudio.com/docs/editor/editingevolved#_code-action)
    //     code action menus.
    //
    //   - Disabled actions are shown as faded out in the code action menu when the user requests a more specific type
    //     of code action, such as refactorings.
    //
    //   - If the user has a [keybinding](https://code.visualstudio.com/docs/editor/refactoring#_keybindings-for-code-actions)
    //     that auto applies a code action and only disabled code actions are returned, the client should show the user an
    //     error message with `reason` in the editor.
    //
    // @since 3.16.0
    // { ... }
    let disabled: CodeActionDisabled = null;
    // The workspace edit this code action performs.
    let edit: WorkspaceEdit = null;
    // A command this code action executes. If a code action
    // provides an edit and a command, first the edit is
    // executed and then the command.
    let command: Command = null;
    // A data entry field that is preserved on a code action between
    // a `textDocument/codeAction` and a `codeAction/resolve` request.
    //
    // @since 3.16.0
    // LSPAny
    let data: Object = null;
    new() { }
}

// Registration options for a {@link CodeActionRequest}.
def CodeActionRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // CodeActionKinds that this server may return.
    //
    // The list of kinds may be generic, such as `CodeActionKind.Refactor`, or the server
    // may list out every specific kind they provide.
    // CodeActionKind[]
    let codeActionKinds: List[String] = null;
    // The server provides support to resolve additional
    // information for a code action.
    //
    // @since 3.16.0
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link WorkspaceSymbolRequest}.
def WorkspaceSymbolParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // A query string to filter symbols by. Clients may send an empty
    // string here to request all symbols.
    // string
    let query: String = null;
    new() { }
}

// A special workspace symbol that supports locations without a range.
//
// See also SymbolInformation.
//
// @since 3.17.0
def WorkspaceSymbol {
    // The name of this symbol.
    // string
    let name: String = null;
    // The kind of this symbol.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this symbol.
    //
    // @since 3.16.0
    // SymbolTag[]
    let tags: List[Int32] = null;
    // The name of the symbol containing this symbol. This information is for
    // user interface purposes (e.g. to render a qualifier in the user interface
    // if necessary). It can't be used to re-infer a hierarchy for the document
    // symbols.
    // string
    let containerName: String = null;
    // The location of the symbol. Whether a server is allowed to
    // return a location without a range depends on the client
    // capability `workspace.symbol.resolveSupport`.
    //
    // See SymbolInformation#location for more details.
    // Location | { ... }
    let location: Object = null;
    // A data entry field that is preserved on a workspace symbol between a
    // workspace symbol request and a workspace symbol resolve request.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Registration options for a {@link WorkspaceSymbolRequest}.
def WorkspaceSymbolRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The server provides support to resolve additional
    // information for a workspace symbol.
    //
    // @since 3.17.0
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link CodeLensRequest}.
def CodeLensParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The document to request code lens for.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// A code lens represents a {@link Command command} that should be shown along with
// source text, like the number of references, a way to run tests, etc.
//
// A code lens is _unresolved_ when no command is associated to it. For performance
// reasons the creation of a code lens and resolving should be done in two stages.
def CodeLens {
    // The range in which this code lens is valid. Should only span a single line.
    let range: Range = null;
    // The command this code lens represents.
    let command: Command = null;
    // A data entry field that is preserved on a code lens item between
    // a {@link CodeLensRequest} and a {@link CodeLensResolveRequest}
    // LSPAny
    let data: Object = null;
    new() { }
}

// Registration options for a {@link CodeLensRequest}.
def CodeLensRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Code lens has a resolve provider as well.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link DocumentLinkRequest}.
def DocumentLinkParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    // The document to provide document links for.
    let textDocument: TextDocumentIdentifier = null;
    new() { }
}

// A document link is a range in a text document that links to an internal or external resource, like another
// text document or a web site.
def DocumentLink {
    // The range this link applies to.
    let range: Range = null;
    // The uri this link points to. If missing a resolve request is sent later.
    // URI
    let target: String = null;
    // The tooltip text when you hover over this link.
    //
    // If a tooltip is provided, is will be displayed in a string that includes instructions on how to
    // trigger the link, such as `{0} (ctrl + click)`. The specific instructions vary depending on OS,
    // user settings, and localization.
    //
    // @since 3.15.0
    // string
    let tooltip: String = null;
    // A data entry field that is preserved on a document link between a
    // DocumentLinkRequest and a DocumentLinkResolveRequest.
    // LSPAny
    let data: Object = null;
    new() { }
}

// Registration options for a {@link DocumentLinkRequest}.
def DocumentLinkRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Document links have a resolve provider as well.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link DocumentFormattingRequest}.
def DocumentFormattingParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The document to format.
    let textDocument: TextDocumentIdentifier = null;
    // The format options.
    let options: FormattingOptions = null;
    new() { }
}

// Registration options for a {@link DocumentFormattingRequest}.
def DocumentFormattingRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link DocumentRangeFormattingRequest}.
def DocumentRangeFormattingParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The document to format.
    let textDocument: TextDocumentIdentifier = null;
    // The range to format
    let range: Range = null;
    // The format options
    let options: FormattingOptions = null;
    new() { }
}

// Registration options for a {@link DocumentRangeFormattingRequest}.
def DocumentRangeFormattingRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Whether the server supports formatting multiple ranges at once.
    //
    // @since 3.18.0
    // @proposed
    // boolean
    let rangesSupport: Nullable[Boolean] = null;
    new() { }
}

// The parameters of a {@link DocumentRangesFormattingRequest}.
//
// @since 3.18.0
// @proposed
def DocumentRangesFormattingParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The document to format.
    let textDocument: TextDocumentIdentifier = null;
    // The ranges to format
    // Range[]
    let ranges: List[Range] = null;
    // The format options
    let options: FormattingOptions = null;
    new() { }
}

// The parameters of a {@link DocumentOnTypeFormattingRequest}.
def DocumentOnTypeFormattingParams {
    // The document to format.
    let textDocument: TextDocumentIdentifier = null;
    // The position around which the on type formatting should happen.
    // This is not necessarily the exact position where the character denoted
    // by the property `ch` got typed.
    let position: Position = null;
    // The character that has been typed that triggered the formatting
    // on type request. That is not necessarily the last character that
    // got inserted into the document since the client could auto insert
    // characters as well (e.g. like automatic brace completion).
    // string
    let ch: String = null;
    // The formatting options.
    let options: FormattingOptions = null;
    new() { }
}

// Registration options for a {@link DocumentOnTypeFormattingRequest}.
def DocumentOnTypeFormattingRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // A character on which formatting should be triggered, like `{`.
    // string
    let firstTriggerCharacter: String = null;
    // More trigger characters.
    // string[]
    let moreTriggerCharacter: List[String] = null;
    new() { }
}

// The parameters of a {@link RenameRequest}.
def RenameParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The document to rename.
    let textDocument: TextDocumentIdentifier = null;
    // The position at which this request was sent.
    let position: Position = null;
    // The new name of the symbol. If the given name is not valid the
    // request must return a {@link ResponseError} with an
    // appropriate message set.
    // string
    let newName: String = null;
    new() { }
}

// Registration options for a {@link RenameRequest}.
def RenameRegistrationOptions {
    // A document selector to identify the scope of the registration. If set to null
    // the document selector provided on the client side will be used.
    // DocumentSelector | null
    let documentSelector: List[Object] = null;
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Renames should be checked and tested before being executed.
    //
    // @since version 3.12.0
    // boolean
    let prepareProvider: Nullable[Boolean] = null;
    new() { }
}

def PrepareRenameParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

// The parameters of a {@link ExecuteCommandRequest}.
def ExecuteCommandParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The identifier of the actual command handler.
    // string
    let command: String = null;
    // Arguments that the command should be invoked with.
    // LSPAny[]
    let arguments: List[Object] = null;
    new() { }
}

// Registration options for a {@link ExecuteCommandRequest}.
def ExecuteCommandRegistrationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The commands to be executed on the server
    // string[]
    let commands: List[String] = null;
    new() { }
}

// The parameters passed via an apply workspace edit request.
def ApplyWorkspaceEditParams {
    // An optional label of the workspace edit. This label is
    // presented in the user interface for example on an undo
    // stack to undo the workspace edit.
    // string
    let label: String = null;
    // The edits to apply.
    let edit: WorkspaceEdit = null;
    new() { }
}

// The result returned from the apply workspace edit request.
//
// @since 3.17 renamed from ApplyWorkspaceEditResponse
def ApplyWorkspaceEditResult {
    // Indicates whether the edit was applied or not.
    // boolean
    let applied: Boolean = false;
    // An optional textual description for why the edit was not applied.
    // This may be used by the server for diagnostic logging or to provide
    // a suitable error for a request that triggered the edit.
    // string
    let failureReason: String = null;
    // Depending on the client's failure handling strategy `failedChange` might
    // contain the index of the change that failed. This property is only available
    // if the client signals a `failureHandlingStrategy` in its client capabilities.
    // uinteger
    let failedChange: Nullable[Int32] = null;
    new() { }
}

def WorkDoneProgressBegin {
    // 'begin'
    let kind: String = null;
    // Mandatory title of the progress operation. Used to briefly inform about
    // the kind of operation being performed.
    //
    // Examples: "Indexing" or "Linking dependencies".
    // string
    let title: String = null;
    // Controls if a cancel button should show to allow the user to cancel the
    // long running operation. Clients that don't support cancellation are allowed
    // to ignore the setting.
    // boolean
    let cancellable: Nullable[Boolean] = null;
    // Optional, more detailed associated progress message. Contains
    // complementary information to the `title`.
    //
    // Examples: "3/25 files", "project/src/module2", "node_modules/some_dep".
    // If unset, the previous progress message (if any) is still valid.
    // string
    let message: String = null;
    // Optional progress percentage to display (value 100 is considered 100%).
    // If not provided infinite progress is assumed and clients are allowed
    // to ignore the `percentage` value in subsequent report notifications.
    //
    // The value should be steadily rising. Clients are free to ignore values
    // that are not following this rule. The value range is [0, 100].
    // uinteger
    let percentage: Nullable[Int32] = null;
    new() { }
}

def WorkDoneProgressReport {
    // 'report'
    let kind: String = null;
    // Controls enablement state of a cancel button.
    //
    // Clients that don't support cancellation or don't support controlling the button's
    // enablement state are allowed to ignore the property.
    // boolean
    let cancellable: Nullable[Boolean] = null;
    // Optional, more detailed associated progress message. Contains
    // complementary information to the `title`.
    //
    // Examples: "3/25 files", "project/src/module2", "node_modules/some_dep".
    // If unset, the previous progress message (if any) is still valid.
    // string
    let message: String = null;
    // Optional progress percentage to display (value 100 is considered 100%).
    // If not provided infinite progress is assumed and clients are allowed
    // to ignore the `percentage` value in subsequent report notifications.
    //
    // The value should be steadily rising. Clients are free to ignore values
    // that are not following this rule. The value range is [0, 100].
    // uinteger
    let percentage: Nullable[Int32] = null;
    new() { }
}

def WorkDoneProgressEnd {
    // 'end'
    let kind: String = null;
    // Optional, a final message indicating to for example indicate the outcome
    // of the operation.
    // string
    let message: String = null;
    new() { }
}

def SetTraceParams {
    // TraceValues
    let value: String = null;
    new() { }
}

def LogTraceParams {
    // string
    let message: String = null;
    // string
    let verbose: String = null;
    new() { }
}

def CancelParams {
    // The request id to cancel.
    // integer | string
    let id: Object = null;
    new() { }
}

def ProgressParams {
    // The progress token provided by the client or server.
    // ProgressToken
    let token: Object = null;
    // The progress data.
    // LSPAny
    let value: Object = null;
    new() { }
}

// A parameter literal used in requests to pass a text document and a position inside that
// document.
def TextDocumentPositionParams {
    // The text document.
    let textDocument: TextDocumentIdentifier = null;
    // The position inside the text document.
    let position: Position = null;
    new() { }
}

def WorkDoneProgressParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    new() { }
}

def PartialResultParams {
    // An optional token that a server can use to report partial results (e.g. streaming) to
    // the client.
    // ProgressToken
    let partialResultToken: Object = null;
    new() { }
}

// Represents the connection of two locations. Provides additional metadata over normal {@link Location locations},
// including an origin range.
def LocationLink {
    // Span of the origin of this link.
    //
    // Used as the underlined span for mouse interaction. Defaults to the word range at
    // the definition position.
    let originSelectionRange: Range = null;
    // The target resource identifier of this link.
    // DocumentUri
    let targetUri: String = null;
    // The full target range of this link. If the target for example is a symbol then target range is the
    // range enclosing this symbol not including leading/trailing whitespace but everything else
    // like comments. This information is typically used to highlight the range in the editor.
    let targetRange: Range = null;
    // The range that should be selected and revealed when this link is being followed, e.g the name of a function.
    // Must be contained by the `targetRange`. See also `DocumentSymbol#range`
    let targetSelectionRange: Range = null;
    new() { }
}

// A range in a text document expressed as (zero-based) start and end positions.
//
// If you want to specify a range that contains a line including the line ending
// character(s) then use an end position denoting the start of the next line.
// For example:
// ```ts
// {
//     start: { line: 5, character: 23 }
//     end : { line 6, character : 0 }
// }
// ```
def Range {
    // The range's start position.
    let start: Position = null;
    // The range's end position.
    let end: Position = null;
    new() { }
}

def ImplementationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Static registration options to be returned in the initialize
// request.
def StaticRegistrationOptions {
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def TypeDefinitionOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// The workspace folder change event.
def WorkspaceFoldersChangeEvent {
    // The array of added workspace folders
    // WorkspaceFolder[]
    let added: List[WorkspaceFolder] = null;
    // The array of the removed workspace folders
    // WorkspaceFolder[]
    let removed: List[WorkspaceFolder] = null;
    new() { }
}

def ConfigurationItem {
    // The scope to get the configuration section for.
    // URI
    let scopeUri: String = null;
    // The configuration section asked for.
    // string
    let section: String = null;
    new() { }
}

// A literal to identify a text document in the client.
def TextDocumentIdentifier {
    // The text document's uri.
    // DocumentUri
    let uri: String = null;
    new() { }
}

// Represents a color in RGBA space.
def Color {
    // The red component of this color in the range [0-1].
    // decimal
    let red: Double = 0.0;
    // The green component of this color in the range [0-1].
    // decimal
    let green: Double = 0.0;
    // The blue component of this color in the range [0-1].
    // decimal
    let blue: Double = 0.0;
    // The alpha component of this color in the range [0-1].
    // decimal
    let alpha: Double = 0.0;
    new() { }
}

def DocumentColorOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

def FoldingRangeOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

def DeclarationOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Position in a text document expressed as zero-based line and character
// offset. Prior to 3.17 the offsets were always based on a UTF-16 string
// representation. So a string of the form `a𐐀b` the character offset of the
// character `a` is 0, the character offset of `𐐀` is 1 and the character
// offset of b is 3 since `𐐀` is represented using two code units in UTF-16.
// Since 3.17 clients and servers can agree on a different string encoding
// representation (e.g. UTF-8). The client announces it's supported encoding
// via the client capability [`general.positionEncodings`](https://microsoft.github.io/language-server-protocol/specifications/specification-current/#clientCapabilities).
// The value is an array of position encodings the client supports, with
// decreasing preference (e.g. the encoding at index `0` is the most preferred
// one). To stay backwards compatible the only mandatory encoding is UTF-16
// represented via the string `utf-16`. The server can pick one of the
// encodings offered by the client and signals that encoding back to the
// client via the initialize result's property
// [`capabilities.positionEncoding`](https://microsoft.github.io/language-server-protocol/specifications/specification-current/#serverCapabilities). If the string value
// `utf-16` is missing from the client's capability `general.positionEncodings`
// servers can safely assume that the client supports UTF-16. If the server
// omits the position encoding in its initialize result the encoding defaults
// to the string value `utf-16`. Implementation considerations: since the
// conversion from one encoding into another requires the content of the
// file / line the conversion is best done where the file is read which is
// usually on the server side.
//
// Positions are line end character agnostic. So you can not specify a position
// that denotes `\r|\n` or `\n|` where `|` represents the character offset.
//
// @since 3.17.0 - support for negotiated position encoding.
def Position {
    // Line position in a document (zero-based).
    //
    // If a line number is greater than the number of lines in a document, it defaults back to the number of lines in the document.
    // If a line number is negative, it defaults to 0.
    // uinteger
    let line: Int32 = 0;
    // Character offset on a line in a document (zero-based).
    //
    // The meaning of this offset is determined by the negotiated
    // `PositionEncodingKind`.
    //
    // If the character value is greater than the line length it defaults back to the
    // line length.
    // uinteger
    let character: Int32 = 0;
    new() { }
}

def SelectionRangeOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Call hierarchy options used during static registration.
//
// @since 3.16.0
def CallHierarchyOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The legend used by the server
    let legend: SemanticTokensLegend = null;
    // Server supports providing semantic tokens for a specific range
    // of a document.
    // boolean | { ... }
    let range: Object = null;
    // Server supports providing semantic tokens for a full document.
    // boolean | { ... }
    let full: Object = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensEdit {
    // The start offset of the edit.
    // uinteger
    let start: Int32 = 0;
    // The count of elements to remove.
    // uinteger
    let deleteCount: Int32 = 0;
    // The elements to insert.
    // uinteger[]
    let data: List[Int32] = null;
    new() { }
}

def LinkedEditingRangeOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Represents information on a file/folder create.
//
// @since 3.16.0
def FileCreate {
    // A file:// URI for the location of the file/folder being created.
    // string
    let uri: String = null;
    new() { }
}

// Describes textual changes on a text document. A TextDocumentEdit describes all changes
// on a document version Si and after they are applied move the document to version Si+1.
// So the creator of a TextDocumentEdit doesn't need to sort the array of edits or do any
// kind of ordering. However the edits must be non overlapping.
def TextDocumentEdit {
    // The text document to change.
    let textDocument: OptionalVersionedTextDocumentIdentifier = null;
    // The edits to be applied.
    //
    // @since 3.16.0 - support for AnnotatedTextEdit. This is guarded using a
    // client capability.
    // (TextEdit | AnnotatedTextEdit)[]
    let edits: List[Object] = null;
    new() { }
}

// Create file operation.
def CreateFile {
    // A create
    // 'create'
    let kind: String = null;
    // An optional annotation identifier describing the operation.
    //
    // @since 3.16.0
    // ChangeAnnotationIdentifier
    let annotationId: String = null;
    // The resource to create.
    // DocumentUri
    let uri: String = null;
    // Additional options
    let options: CreateFileOptions = null;
    new() { }
}

// Rename file operation
def RenameFile {
    // A rename
    // 'rename'
    let kind: String = null;
    // An optional annotation identifier describing the operation.
    //
    // @since 3.16.0
    // ChangeAnnotationIdentifier
    let annotationId: String = null;
    // The old (existing) location.
    // DocumentUri
    let oldUri: String = null;
    // The new location.
    // DocumentUri
    let newUri: String = null;
    // Rename options.
    let options: RenameFileOptions = null;
    new() { }
}

// Delete file operation
def DeleteFile {
    // A delete
    // 'delete'
    let kind: String = null;
    // An optional annotation identifier describing the operation.
    //
    // @since 3.16.0
    // ChangeAnnotationIdentifier
    let annotationId: String = null;
    // The file to delete.
    // DocumentUri
    let uri: String = null;
    // Delete options.
    let options: DeleteFileOptions = null;
    new() { }
}

// Additional information that describes document changes.
//
// @since 3.16.0
def ChangeAnnotation {
    // A human-readable string describing the actual change. The string
    // is rendered prominent in the user interface.
    // string
    let label: String = null;
    // A flag which indicates that user confirmation is needed
    // before applying the change.
    // boolean
    let needsConfirmation: Nullable[Boolean] = null;
    // A human-readable string which is rendered less prominent in
    // the user interface.
    // string
    let description: String = null;
    new() { }
}

// A filter to describe in which file operation requests or notifications
// the server is interested in receiving.
//
// @since 3.16.0
def FileOperationFilter {
    // A Uri scheme like `file` or `untitled`.
    // string
    let scheme: String = null;
    // The actual file operation pattern.
    let pattern: FileOperationPattern = null;
    new() { }
}

// Represents information on a file/folder rename.
//
// @since 3.16.0
def FileRename {
    // A file:// URI for the original location of the file/folder being renamed.
    // string
    let oldUri: String = null;
    // A file:// URI for the new location of the file/folder being renamed.
    // string
    let newUri: String = null;
    new() { }
}

// Represents information on a file/folder delete.
//
// @since 3.16.0
def FileDelete {
    // A file:// URI for the location of the file/folder being deleted.
    // string
    let uri: String = null;
    new() { }
}

def MonikerOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Type hierarchy options used during static registration.
//
// @since 3.17.0
def TypeHierarchyOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// @since 3.17.0
def InlineValueContext {
    // The stack frame (as a DAP Id) where the execution has stopped.
    // integer
    let frameId: Int32 = 0;
    // The document range where execution has stopped.
    // Typically the end position of the range denotes the line where the inline values are shown.
    let stoppedLocation: Range = null;
    new() { }
}

// Provide inline value as text.
//
// @since 3.17.0
def InlineValueText {
    // The document range for which the inline value applies.
    let range: Range = null;
    // The text of the inline value.
    // string
    let text: String = null;
    new() { }
}

// Provide inline value through a variable lookup.
// If only a range is specified, the variable name will be extracted from the underlying document.
// An optional variable name can be used to override the extracted name.
//
// @since 3.17.0
def InlineValueVariableLookup {
    // The document range for which the inline value applies.
    // The range is used to extract the variable name from the underlying document.
    let range: Range = null;
    // If specified the name of the variable to look up.
    // string
    let variableName: String = null;
    // How to perform the lookup.
    // boolean
    let caseSensitiveLookup: Boolean = false;
    new() { }
}

// Provide an inline value through an expression evaluation.
// If only a range is specified, the expression will be extracted from the underlying document.
// An optional expression can be used to override the extracted expression.
//
// @since 3.17.0
def InlineValueEvaluatableExpression {
    // The document range for which the inline value applies.
    // The range is used to extract the evaluatable expression from the underlying document.
    let range: Range = null;
    // If specified the expression overrides the extracted expression.
    // string
    let expression: String = null;
    new() { }
}

// Inline value options used during static registration.
//
// @since 3.17.0
def InlineValueOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// An inlay hint label part allows for interactive and composite labels
// of inlay hints.
//
// @since 3.17.0
def InlayHintLabelPart {
    // The value of this label part.
    // string
    let value: String = null;
    // The tooltip text when you hover over this label part. Depending on
    // the client capability `inlayHint.resolveSupport` clients might resolve
    // this property late using the resolve request.
    // string | MarkupContent
    let tooltip: Object = null;
    // An optional source code location that represents this
    // label part.
    //
    // The editor will use this location for the hover and for code navigation
    // features: This part will become a clickable link that resolves to the
    // definition of the symbol at the given location (not necessarily the
    // location itself), it shows the hover that shows at the given location,
    // and it shows a context menu with further code navigation commands.
    //
    // Depending on the client capability `inlayHint.resolveSupport` clients
    // might resolve this property late using the resolve request.
    let location: Location = null;
    // An optional command for this label part.
    //
    // Depending on the client capability `inlayHint.resolveSupport` clients
    // might resolve this property late using the resolve request.
    let command: Command = null;
    new() { }
}

// A `MarkupContent` literal represents a string value which content is interpreted base on its
// kind flag. Currently the protocol supports `plaintext` and `markdown` as markup kinds.
//
// If the kind is `markdown` then the value can contain fenced code blocks like in GitHub issues.
// See https://help.github.com/articles/creating-and-highlighting-code-blocks/#syntax-highlighting
//
// Here is an example how such a string can be constructed using JavaScript / TypeScript:
// ```ts
// let markdown: MarkdownContent = {
//  kind: MarkupKind.Markdown,
//  value: [
//    '# Header',
//    'Some text',
//    '```typescript',
//    'someCode();',
//    '```'
//  ].join('\n')
// };
// ```
//
// *Please Note* that clients might sanitize the return markdown. A client could decide to
// remove HTML from the markdown to avoid script execution.
def MarkupContent {
    // The type of the Markup
    // MarkupKind
    let kind: String = null;
    // The content itself
    // string
    let value: String = null;
    new() { }
}

// Inlay hint options used during static registration.
//
// @since 3.17.0
def InlayHintOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The server provides support to resolve additional
    // information for an inlay hint item.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// A full diagnostic report with a set of related documents.
//
// @since 3.17.0
def RelatedFullDocumentDiagnosticReport {
    // A full document diagnostic report.
    // 'full'
    let kind: String = null;
    // An optional result id. If provided it will
    // be sent on the next diagnostic request for the
    // same document.
    // string
    let resultId: String = null;
    // The actual items.
    // Diagnostic[]
    let items: List[Diagnostic] = null;
    // Diagnostics of related documents. This information is useful
    // in programming languages where code in a file A can generate
    // diagnostics in a file B which A depends on. An example of
    // such a language is C/C++ where marco definitions in a file
    // a.cpp and result in errors in a header file b.hpp.
    //
    // @since 3.17.0
    // { [key: DocumentUri]: FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport }
    let relatedDocuments: Dictionary[String, Object] = null;
    new() { }
}

// An unchanged diagnostic report with a set of related documents.
//
// @since 3.17.0
def RelatedUnchangedDocumentDiagnosticReport {
    // A document diagnostic report indicating
    // no changes to the last result. A server can
    // only return `unchanged` if result ids are
    // provided.
    // 'unchanged'
    let kind: String = null;
    // A result id which will be sent on the next
    // diagnostic request for the same document.
    // string
    let resultId: String = null;
    // Diagnostics of related documents. This information is useful
    // in programming languages where code in a file A can generate
    // diagnostics in a file B which A depends on. An example of
    // such a language is C/C++ where marco definitions in a file
    // a.cpp and result in errors in a header file b.hpp.
    //
    // @since 3.17.0
    // { [key: DocumentUri]: FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport }
    let relatedDocuments: Dictionary[String, Object] = null;
    new() { }
}

// A diagnostic report with a full set of problems.
//
// @since 3.17.0
def FullDocumentDiagnosticReport {
    // A full document diagnostic report.
    // 'full'
    let kind: String = null;
    // An optional result id. If provided it will
    // be sent on the next diagnostic request for the
    // same document.
    // string
    let resultId: String = null;
    // The actual items.
    // Diagnostic[]
    let items: List[Diagnostic] = null;
    new() { }
}

// A diagnostic report indicating that the last returned
// report is still accurate.
//
// @since 3.17.0
def UnchangedDocumentDiagnosticReport {
    // A document diagnostic report indicating
    // no changes to the last result. A server can
    // only return `unchanged` if result ids are
    // provided.
    // 'unchanged'
    let kind: String = null;
    // A result id which will be sent on the next
    // diagnostic request for the same document.
    // string
    let resultId: String = null;
    new() { }
}

// Diagnostic options.
//
// @since 3.17.0
def DiagnosticOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // An optional identifier under which the diagnostics are
    // managed by the client.
    // string
    let identifier: String = null;
    // Whether the language has inter file dependencies meaning that
    // editing code in one file can result in a different diagnostic
    // set in another file. Inter file dependencies are common for
    // most programming languages and typically uncommon for linters.
    // boolean
    let interFileDependencies: Boolean = false;
    // The server provides support for workspace diagnostics as well.
    // boolean
    let workspaceDiagnostics: Boolean = false;
    new() { }
}

// A previous result id in a workspace pull request.
//
// @since 3.17.0
def PreviousResultId {
    // The URI for which the client knowns a
    // result id.
    // DocumentUri
    let uri: String = null;
    // The value of the previous result id.
    // string
    let value: String = null;
    new() { }
}

// A notebook document.
//
// @since 3.17.0
def NotebookDocument {
    // The notebook document's uri.
    // URI
    let uri: String = null;
    // The type of the notebook.
    // string
    let notebookType: String = null;
    // The version number of this document (it will increase after each
    // change, including undo/redo).
    // integer
    let version: Int32 = 0;
    // Additional metadata stored with the notebook
    // document.
    //
    // Note: should always be an object literal (e.g. LSPObject)
    // LSPObject
    let metadata: Dictionary[String, Object] = null;
    // The cells of a notebook.
    // NotebookCell[]
    let cells: List[NotebookCell] = null;
    new() { }
}

// An item to transfer a text document from the client to the
// server.
def TextDocumentItem {
    // The text document's uri.
    // DocumentUri
    let uri: String = null;
    // The text document's language identifier.
    // string
    let languageId: String = null;
    // The version number of this document (it will increase after each
    // change, including undo/redo).
    // integer
    let version: Int32 = 0;
    // The content of the opened text document.
    // string
    let text: String = null;
    new() { }
}

// A versioned notebook document identifier.
//
// @since 3.17.0
def VersionedNotebookDocumentIdentifier {
    // The version number of this notebook document.
    // integer
    let version: Int32 = 0;
    // The notebook document's uri.
    // URI
    let uri: String = null;
    new() { }
}

// A change event for a notebook document.
//
// @since 3.17.0
def NotebookDocumentChangeEvent {
    // The changed meta data if any.
    //
    // Note: should always be an object literal (e.g. LSPObject)
    // LSPObject
    let metadata: Dictionary[String, Object] = null;
    // Changes to cells
    // { ... }
    let cells: NotebookDocumentChangeEventCells = null;
    new() { }
}

// A literal to identify a notebook document in the client.
//
// @since 3.17.0
def NotebookDocumentIdentifier {
    // The notebook document's uri.
    // URI
    let uri: String = null;
    new() { }
}

// Provides information about the context in which an inline completion was requested.
//
// @since 3.18.0
// @proposed
def InlineCompletionContext {
    // Describes how the inline completion was triggered.
    // InlineCompletionTriggerKind
    let triggerKind: Int32 = 0;
    // Provides information about the currently selected item in the autocomplete widget if it is visible.
    let selectedCompletionInfo: SelectedCompletionInfo = null;
    new() { }
}

// A string value used as a snippet is a template which allows to insert text
// and to control the editor cursor when insertion happens.
//
// A snippet can define tab stops and placeholders with `$1`, `$2`
// and `${3:foo}`. `$0` defines the final tab stop, it defaults to
// the end of the snippet. Variables are defined with `$name` and
// `${name:default value}`.
//
// @since 3.18.0
// @proposed
def StringValue {
    // The kind of string value.
    // 'snippet'
    let kind: String = null;
    // The snippet string.
    // string
    let value: String = null;
    new() { }
}

// Inline completion options used during static registration.
//
// @since 3.18.0
// @proposed
def InlineCompletionOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// General parameters to register for a notification or to register a provider.
def Registration {
    // The id used to register the request. The id can be used to deregister
    // the request again.
    // string
    let id: String = null;
    // The method / capability to register for.
    // string
    let method: String = null;
    // Options necessary for the registration.
    // LSPAny
    let registerOptions: Object = null;
    new() { }
}

// General parameters to unregister a request or notification.
def Unregistration {
    // The id used to unregister the request or notification. Usually an id
    // provided during the register request.
    // string
    let id: String = null;
    // The method to unregister for.
    // string
    let method: String = null;
    new() { }
}

// The initialize parameters
def _InitializeParams {
    // An optional token that a server can use to report work done progress.
    // ProgressToken
    let workDoneToken: Object = null;
    // The process Id of the parent process that started
    // the server.
    //
    // Is `null` if the process has not been started by another process.
    // If the parent process is not alive then the server should exit.
    // integer | null
    let processId: Nullable[Int32] = null;
    // Information about the client
    //
    // @since 3.15.0
    // { ... }
    let clientInfo: _InitializeParamsClientInfo = null;
    // The locale the client is currently showing the user interface
    // in. This must not necessarily be the locale of the operating
    // system.
    //
    // Uses IETF language tags as the value's syntax
    // (See https://en.wikipedia.org/wiki/IETF_language_tag)
    //
    // @since 3.16.0
    // string
    let locale: String = null;
    // The rootPath of the workspace. Is null
    // if no folder is open.
    //
    // @deprecated in favour of rootUri.
    // string | null
    let rootPath: String = null;
    // The rootUri of the workspace. Is null if no
    // folder is open. If both `rootPath` and `rootUri` are set
    // `rootUri` wins.
    //
    // @deprecated in favour of workspaceFolders.
    // DocumentUri | null
    let rootUri: String = null;
    // The capabilities provided by the client (editor or tool)
    let capabilities: ClientCapabilities = null;
    // User provided initialization options.
    // LSPAny
    let initializationOptions: Object = null;
    // The initial trace setting. If omitted trace is disabled ('off').
    // TraceValues
    let trace: String = null;
    new() { }
}

def WorkspaceFoldersInitializeParams {
    // The workspace folders configured in the client when the server starts.
    //
    // This property is only available if the client supports workspace folders.
    // It can be `null` if the client supports workspace folders but none are
    // configured.
    //
    // @since 3.6.0
    // WorkspaceFolder[] | null
    let workspaceFolders: List[WorkspaceFolder] = null;
    new() { }
}

// Defines the capabilities provided by a language
// server.
def ServerCapabilities {
    // The position encoding the server picked from the encodings offered
    // by the client via the client capability `general.positionEncodings`.
    //
    // If the client didn't provide any position encodings the only valid
    // value that a server can return is 'utf-16'.
    //
    // If omitted it defaults to 'utf-16'.
    //
    // @since 3.17.0
    // PositionEncodingKind
    let positionEncoding: String = null;
    // Defines how text documents are synced. Is either a detailed structure
    // defining each notification or for backwards compatibility the
    // TextDocumentSyncKind number.
    // TextDocumentSyncOptions | TextDocumentSyncKind
    let textDocumentSync: Object = null;
    // Defines how notebook documents are synced.
    //
    // @since 3.17.0
    // NotebookDocumentSyncOptions | NotebookDocumentSyncRegistrationOptions
    let notebookDocumentSync: Object = null;
    // The server provides completion support.
    let completionProvider: CompletionOptions = null;
    // The server provides hover support.
    // boolean | HoverOptions
    let hoverProvider: Object = null;
    // The server provides signature help support.
    let signatureHelpProvider: SignatureHelpOptions = null;
    // The server provides Goto Declaration support.
    // boolean | DeclarationOptions | DeclarationRegistrationOptions
    let declarationProvider: Object = null;
    // The server provides goto definition support.
    // boolean | DefinitionOptions
    let definitionProvider: Object = null;
    // The server provides Goto Type Definition support.
    // boolean | TypeDefinitionOptions | TypeDefinitionRegistrationOptions
    let typeDefinitionProvider: Object = null;
    // The server provides Goto Implementation support.
    // boolean | ImplementationOptions | ImplementationRegistrationOptions
    let implementationProvider: Object = null;
    // The server provides find references support.
    // boolean | ReferenceOptions
    let referencesProvider: Object = null;
    // The server provides document highlight support.
    // boolean | DocumentHighlightOptions
    let documentHighlightProvider: Object = null;
    // The server provides document symbol support.
    // boolean | DocumentSymbolOptions
    let documentSymbolProvider: Object = null;
    // The server provides code actions. CodeActionOptions may only be
    // specified if the client states that it supports
    // `codeActionLiteralSupport` in its initial `initialize` request.
    // boolean | CodeActionOptions
    let codeActionProvider: Object = null;
    // The server provides code lens.
    let codeLensProvider: CodeLensOptions = null;
    // The server provides document link support.
    let documentLinkProvider: DocumentLinkOptions = null;
    // The server provides color provider support.
    // boolean | DocumentColorOptions | DocumentColorRegistrationOptions
    let colorProvider: Object = null;
    // The server provides workspace symbol support.
    // boolean | WorkspaceSymbolOptions
    let workspaceSymbolProvider: Object = null;
    // The server provides document formatting.
    // boolean | DocumentFormattingOptions
    let documentFormattingProvider: Object = null;
    // The server provides document range formatting.
    // boolean | DocumentRangeFormattingOptions
    let documentRangeFormattingProvider: Object = null;
    // The server provides document formatting on typing.
    let documentOnTypeFormattingProvider: DocumentOnTypeFormattingOptions = null;
    // The server provides rename support. RenameOptions may only be
    // specified if the client states that it supports
    // `prepareSupport` in its initial `initialize` request.
    // boolean | RenameOptions
    let renameProvider: Object = null;
    // The server provides folding provider support.
    // boolean | FoldingRangeOptions | FoldingRangeRegistrationOptions
    let foldingRangeProvider: Object = null;
    // The server provides selection range support.
    // boolean | SelectionRangeOptions | SelectionRangeRegistrationOptions
    let selectionRangeProvider: Object = null;
    // The server provides execute command support.
    let executeCommandProvider: ExecuteCommandOptions = null;
    // The server provides call hierarchy support.
    //
    // @since 3.16.0
    // boolean | CallHierarchyOptions | CallHierarchyRegistrationOptions
    let callHierarchyProvider: Object = null;
    // The server provides linked editing range support.
    //
    // @since 3.16.0
    // boolean | LinkedEditingRangeOptions | LinkedEditingRangeRegistrationOptions
    let linkedEditingRangeProvider: Object = null;
    // The server provides semantic tokens support.
    //
    // @since 3.16.0
    // SemanticTokensOptions | SemanticTokensRegistrationOptions
    let semanticTokensProvider: Object = null;
    // The server provides moniker support.
    //
    // @since 3.16.0
    // boolean | MonikerOptions | MonikerRegistrationOptions
    let monikerProvider: Object = null;
    // The server provides type hierarchy support.
    //
    // @since 3.17.0
    // boolean | TypeHierarchyOptions | TypeHierarchyRegistrationOptions
    let typeHierarchyProvider: Object = null;
    // The server provides inline values.
    //
    // @since 3.17.0
    // boolean | InlineValueOptions | InlineValueRegistrationOptions
    let inlineValueProvider: Object = null;
    // The server provides inlay hints.
    //
    // @since 3.17.0
    // boolean | InlayHintOptions | InlayHintRegistrationOptions
    let inlayHintProvider: Object = null;
    // The server has support for pull model diagnostics.
    //
    // @since 3.17.0
    // DiagnosticOptions | DiagnosticRegistrationOptions
    let diagnosticProvider: Object = null;
    // Inline completion options used during static registration.
    //
    // @since 3.18.0
    // @proposed
    // boolean | InlineCompletionOptions
    let inlineCompletionProvider: Object = null;
    // Workspace specific server capabilities.
    // { ... }
    let workspace: ServerCapabilitiesWorkspace = null;
    // Experimental server capabilities.
    // LSPAny
    let experimental: Object = null;
    new() { }
}

// A text document identifier to denote a specific version of a text document.
def VersionedTextDocumentIdentifier {
    // The text document's uri.
    // DocumentUri
    let uri: String = null;
    // The version number of this document.
    // integer
    let version: Int32 = 0;
    new() { }
}

// Save options.
def SaveOptions {
    // The client is supposed to include the content on save.
    // boolean
    let includeText: Nullable[Boolean] = null;
    new() { }
}

// An event describing a file change.
def FileEvent {
    // The file's uri.
    // DocumentUri
    let uri: String = null;
    // The change type.
    // FileChangeType
    let type: Int32 = 0;
    new() { }
}

def FileSystemWatcher {
    // The glob pattern to watch. See {@link GlobPattern glob pattern} for more detail.
    //
    // @since 3.17.0 support for relative patterns.
    // GlobPattern
    let globPattern: Object = null;
    // The kind of events of interest. If omitted it defaults
    // to WatchKind.Create | WatchKind.Change | WatchKind.Delete
    // which is 7.
    // WatchKind
    let kind: Nullable[Int32] = null;
    new() { }
}

// Represents a diagnostic, such as a compiler error or warning. Diagnostic objects
// are only valid in the scope of a resource.
def Diagnostic {
    // The range at which the message applies
    let range: Range = null;
    // The diagnostic's severity. Can be omitted. If omitted it is up to the
    // client to interpret diagnostics as error, warning, info or hint.
    // DiagnosticSeverity
    let severity: Nullable[Int32] = null;
    // The diagnostic's code, which usually appear in the user interface.
    // integer | string
    let code: Object = null;
    // An optional property to describe the error code.
    // Requires the code field (above) to be present/not null.
    //
    // @since 3.16.0
    let codeDescription: CodeDescription = null;
    // A human-readable string describing the source of this
    // diagnostic, e.g. 'typescript' or 'super lint'. It usually
    // appears in the user interface.
    // string
    let source: String = null;
    // The diagnostic's message. It usually appears in the user interface
    // string
    let message: String = null;
    // Additional metadata about the diagnostic.
    //
    // @since 3.15.0
    // DiagnosticTag[]
    let tags: List[Int32] = null;
    // An array of related diagnostic information, e.g. when symbol-names within
    // a scope collide all definitions can be marked via this property.
    // DiagnosticRelatedInformation[]
    let relatedInformation: List[DiagnosticRelatedInformation] = null;
    // A data entry field that is preserved between a `textDocument/publishDiagnostics`
    // notification and `textDocument/codeAction` request.
    //
    // @since 3.16.0
    // LSPAny
    let data: Object = null;
    new() { }
}

// Contains additional information about the context in which a completion request is triggered.
def CompletionContext {
    // How the completion was triggered.
    // CompletionTriggerKind
    let triggerKind: Int32 = 0;
    // The trigger character (a single character) that has trigger code complete.
    // Is undefined if `triggerKind !== CompletionTriggerKind.TriggerCharacter`
    // string
    let triggerCharacter: String = null;
    new() { }
}

// Additional details for a completion item label.
//
// @since 3.17.0
def CompletionItemLabelDetails {
    // An optional string which is rendered less prominently directly after {@link CompletionItem.label label},
    // without any spacing. Should be used for function signatures and type annotations.
    // string
    let detail: String = null;
    // An optional string which is rendered less prominently after {@link CompletionItem.detail}. Should be used
    // for fully qualified names and file paths.
    // string
    let description: String = null;
    new() { }
}

// A special text edit to provide an insert and a replace operation.
//
// @since 3.16.0
def InsertReplaceEdit {
    // The string to be inserted.
    // string
    let newText: String = null;
    // The range if the insert is requested
    let insert: Range = null;
    // The range if the replace is requested.
    let replace: Range = null;
    new() { }
}

// Completion options.
def CompletionOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Most tools trigger completion request automatically without explicitly requesting
    // it using a keyboard shortcut (e.g. Ctrl+Space). Typically they do so when the user
    // starts to type an identifier. For example if the user types `c` in a JavaScript file
    // code complete will automatically pop up present `console` besides others as a
    // completion item. Characters that make up identifiers don't need to be listed here.
    //
    // If code complete should automatically be trigger on characters not being valid inside
    // an identifier (for example `.` in JavaScript) list them in `triggerCharacters`.
    // string[]
    let triggerCharacters: List[String] = null;
    // The list of all possible characters that commit a completion. This field can be used
    // if clients don't support individual commit characters per completion item. See
    // `ClientCapabilities.textDocument.completion.completionItem.commitCharactersSupport`
    //
    // If a server provides both `allCommitCharacters` and commit characters on an individual
    // completion item the ones on the completion item win.
    //
    // @since 3.2.0
    // string[]
    let allCommitCharacters: List[String] = null;
    // The server provides support to resolve additional
    // information for a completion item.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    // The server supports the following `CompletionItem` specific
    // capabilities.
    //
    // @since 3.17.0
    // { ... }
    let completionItem: CompletionOptionsCompletionItem = null;
    new() { }
}

// Hover options.
def HoverOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Additional information about the context in which a signature help request was triggered.
//
// @since 3.15.0
def SignatureHelpContext {
    // Action that caused signature help to be triggered.
    // SignatureHelpTriggerKind
    let triggerKind: Int32 = 0;
    // Character that caused signature help to be triggered.
    //
    // This is undefined when `triggerKind !== SignatureHelpTriggerKind.TriggerCharacter`
    // string
    let triggerCharacter: String = null;
    // `true` if signature help was already showing when it was triggered.
    //
    // Retriggers occurs when the signature help is already active and can be caused by actions such as
    // typing a trigger character, a cursor move, or document content changes.
    // boolean
    let isRetrigger: Boolean = false;
    // The currently active `SignatureHelp`.
    //
    // The `activeSignatureHelp` has its `SignatureHelp.activeSignature` field updated based on
    // the user navigating through available signatures.
    let activeSignatureHelp: SignatureHelp = null;
    new() { }
}

// Represents the signature of something callable. A signature
// can have a label, like a function-name, a doc-comment, and
// a set of parameters.
def SignatureInformation {
    // The label of this signature. Will be shown in
    // the UI.
    // string
    let label: String = null;
    // The human-readable doc-comment of this signature. Will be shown
    // in the UI but can be omitted.
    // string | MarkupContent
    let documentation: Object = null;
    // The parameters of this signature.
    // ParameterInformation[]
    let parameters: List[ParameterInformation] = null;
    // The index of the active parameter.
    //
    // If provided, this is used in place of `SignatureHelp.activeParameter`.
    //
    // @since 3.16.0
    // uinteger
    let activeParameter: Nullable[Int32] = null;
    new() { }
}

// Server Capabilities for a {@link SignatureHelpRequest}.
def SignatureHelpOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // List of characters that trigger signature help automatically.
    // string[]
    let triggerCharacters: List[String] = null;
    // List of characters that re-trigger signature help.
    //
    // These trigger characters are only active when signature help is already showing. All trigger characters
    // are also counted as re-trigger characters.
    //
    // @since 3.15.0
    // string[]
    let retriggerCharacters: List[String] = null;
    new() { }
}

// Server Capabilities for a {@link DefinitionRequest}.
def DefinitionOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Value-object that contains additional information when
// requesting references.
def ReferenceContext {
    // Include the declaration of the current symbol.
    // boolean
    let includeDeclaration: Boolean = false;
    new() { }
}

// Reference options.
def ReferenceOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Provider options for a {@link DocumentHighlightRequest}.
def DocumentHighlightOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// A base for all symbol information.
def BaseSymbolInformation {
    // The name of this symbol.
    // string
    let name: String = null;
    // The kind of this symbol.
    // SymbolKind
    let kind: Int32 = 0;
    // Tags for this symbol.
    //
    // @since 3.16.0
    // SymbolTag[]
    let tags: List[Int32] = null;
    // The name of the symbol containing this symbol. This information is for
    // user interface purposes (e.g. to render a qualifier in the user interface
    // if necessary). It can't be used to re-infer a hierarchy for the document
    // symbols.
    // string
    let containerName: String = null;
    new() { }
}

// Provider options for a {@link DocumentSymbolRequest}.
def DocumentSymbolOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // A human-readable string that is shown when multiple outlines trees
    // are shown for the same document.
    //
    // @since 3.16.0
    // string
    let label: String = null;
    new() { }
}

// Contains additional diagnostic information about the context in which
// a {@link CodeActionProvider.provideCodeActions code action} is run.
def CodeActionContext {
    // An array of diagnostics known on the client side overlapping the range provided to the
    // `textDocument/codeAction` request. They are provided so that the server knows which
    // errors are currently presented to the user for the given range. There is no guarantee
    // that these accurately reflect the error state of the resource. The primary parameter
    // to compute code actions is the provided range.
    // Diagnostic[]
    let diagnostics: List[Diagnostic] = null;
    // Requested kind of actions to return.
    //
    // Actions not of this kind are filtered out by the client before being shown. So servers
    // can omit computing them.
    // CodeActionKind[]
    let only: List[String] = null;
    // The reason why code actions were requested.
    //
    // @since 3.17.0
    // CodeActionTriggerKind
    let triggerKind: Nullable[Int32] = null;
    new() { }
}

// Provider options for a {@link CodeActionRequest}.
def CodeActionOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // CodeActionKinds that this server may return.
    //
    // The list of kinds may be generic, such as `CodeActionKind.Refactor`, or the server
    // may list out every specific kind they provide.
    // CodeActionKind[]
    let codeActionKinds: List[String] = null;
    // The server provides support to resolve additional
    // information for a code action.
    //
    // @since 3.16.0
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// Server capabilities for a {@link WorkspaceSymbolRequest}.
def WorkspaceSymbolOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The server provides support to resolve additional
    // information for a workspace symbol.
    //
    // @since 3.17.0
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// Code Lens provider options of a {@link CodeLensRequest}.
def CodeLensOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Code lens has a resolve provider as well.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// Provider options for a {@link DocumentLinkRequest}.
def DocumentLinkOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Document links have a resolve provider as well.
    // boolean
    let resolveProvider: Nullable[Boolean] = null;
    new() { }
}

// Value-object describing what options formatting should use.
def FormattingOptions {
    // Size of a tab in spaces.
    // uinteger
    let tabSize: Int32 = 0;
    // Prefer spaces over tabs.
    // boolean
    let insertSpaces: Boolean = false;
    // Trim trailing whitespace on a line.
    //
    // @since 3.15.0
    // boolean
    let trimTrailingWhitespace: Nullable[Boolean] = null;
    // Insert a newline character at the end of the file if one does not exist.
    //
    // @since 3.15.0
    // boolean
    let insertFinalNewline: Nullable[Boolean] = null;
    // Trim all newlines after the final newline at the end of the file.
    //
    // @since 3.15.0
    // boolean
    let trimFinalNewlines: Nullable[Boolean] = null;
    new() { }
}

// Provider options for a {@link DocumentFormattingRequest}.
def DocumentFormattingOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    new() { }
}

// Provider options for a {@link DocumentRangeFormattingRequest}.
def DocumentRangeFormattingOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Whether the server supports formatting multiple ranges at once.
    //
    // @since 3.18.0
    // @proposed
    // boolean
    let rangesSupport: Nullable[Boolean] = null;
    new() { }
}

// Provider options for a {@link DocumentOnTypeFormattingRequest}.
def DocumentOnTypeFormattingOptions {
    // A character on which formatting should be triggered, like `{`.
    // string
    let firstTriggerCharacter: String = null;
    // More trigger characters.
    // string[]
    let moreTriggerCharacter: List[String] = null;
    new() { }
}

// Provider options for a {@link RenameRequest}.
def RenameOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Renames should be checked and tested before being executed.
    //
    // @since version 3.12.0
    // boolean
    let prepareProvider: Nullable[Boolean] = null;
    new() { }
}

// The server capabilities of a {@link ExecuteCommandRequest}.
def ExecuteCommandOptions {
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // The commands to be executed on the server
    // string[]
    let commands: List[String] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensLegend {
    // The token types a server uses.
    // string[]
    let tokenTypes: List[String] = null;
    // The token modifiers a server uses.
    // string[]
    let tokenModifiers: List[String] = null;
    new() { }
}

// A text document identifier to optionally denote a specific version of a text document.
def OptionalVersionedTextDocumentIdentifier {
    // The text document's uri.
    // DocumentUri
    let uri: String = null;
    // The version number of this document. If a versioned text document identifier
    // is sent from the server to the client and the file is not open in the editor
    // (the server has not received an open notification before) the server can send
    // `null` to indicate that the version is unknown and the content on disk is the
    // truth (as specified with document content ownership).
    // integer | null
    let version: Nullable[Int32] = null;
    new() { }
}

// A special text edit with an additional change annotation.
//
// @since 3.16.0.
def AnnotatedTextEdit {
    // The range of the text document to be manipulated. To insert
    // text into a document create a range where start === end.
    let range: Range = null;
    // The string to be inserted. For delete operations use an
    // empty string.
    // string
    let newText: String = null;
    // The actual identifier of the change annotation
    // ChangeAnnotationIdentifier
    let annotationId: String = null;
    new() { }
}

// A generic resource operation.
def ResourceOperation {
    // The resource operation kind.
    // string
    let kind: String = null;
    // An optional annotation identifier describing the operation.
    //
    // @since 3.16.0
    // ChangeAnnotationIdentifier
    let annotationId: String = null;
    new() { }
}

// Options to create a file.
def CreateFileOptions {
    // Overwrite existing file. Overwrite wins over `ignoreIfExists`
    // boolean
    let overwrite: Nullable[Boolean] = null;
    // Ignore if exists.
    // boolean
    let ignoreIfExists: Nullable[Boolean] = null;
    new() { }
}

// Rename file options
def RenameFileOptions {
    // Overwrite target if existing. Overwrite wins over `ignoreIfExists`
    // boolean
    let overwrite: Nullable[Boolean] = null;
    // Ignores if target exists.
    // boolean
    let ignoreIfExists: Nullable[Boolean] = null;
    new() { }
}

// Delete file options
def DeleteFileOptions {
    // Delete the content recursively if a folder is denoted.
    // boolean
    let recursive: Nullable[Boolean] = null;
    // Ignore the operation if the file doesn't exist.
    // boolean
    let ignoreIfNotExists: Nullable[Boolean] = null;
    new() { }
}

// A pattern to describe in which file operation requests or notifications
// the server is interested in receiving.
//
// @since 3.16.0
def FileOperationPattern {
    // The glob pattern to match. Glob patterns can have the following syntax:
    // - `*` to match zero or more characters in a path segment
    // - `?` to match on one character in a path segment
    // - `**` to match any number of path segments, including none
    // - `{}` to group sub patterns into an OR expression. (e.g. `**​/*.{ts,js}` matches all TypeScript and JavaScript files)
    // - `[]` to declare a range of characters to match in a path segment (e.g., `example.[0-9]` to match on `example.0`, `example.1`, …)
    // - `[!...]` to negate a range of characters to match in a path segment (e.g., `example.[!0-9]` to match on `example.a`, `example.b`, but not `example.0`)
    // string
    let glob: String = null;
    // Whether to match files or folders with this pattern.
    //
    // Matches both if undefined.
    // FileOperationPatternKind
    let matches: String = null;
    // Additional options used during matching.
    let options: FileOperationPatternOptions = null;
    new() { }
}

// A full document diagnostic report for a workspace diagnostic result.
//
// @since 3.17.0
def WorkspaceFullDocumentDiagnosticReport {
    // A full document diagnostic report.
    // 'full'
    let kind: String = null;
    // An optional result id. If provided it will
    // be sent on the next diagnostic request for the
    // same document.
    // string
    let resultId: String = null;
    // The actual items.
    // Diagnostic[]
    let items: List[Diagnostic] = null;
    // The URI for which diagnostic information is reported.
    // DocumentUri
    let uri: String = null;
    // The version number for which the diagnostics are reported.
    // If the document is not marked as open `null` can be provided.
    // integer | null
    let version: Nullable[Int32] = null;
    new() { }
}

// An unchanged document diagnostic report for a workspace diagnostic result.
//
// @since 3.17.0
def WorkspaceUnchangedDocumentDiagnosticReport {
    // A document diagnostic report indicating
    // no changes to the last result. A server can
    // only return `unchanged` if result ids are
    // provided.
    // 'unchanged'
    let kind: String = null;
    // A result id which will be sent on the next
    // diagnostic request for the same document.
    // string
    let resultId: String = null;
    // The URI for which diagnostic information is reported.
    // DocumentUri
    let uri: String = null;
    // The version number for which the diagnostics are reported.
    // If the document is not marked as open `null` can be provided.
    // integer | null
    let version: Nullable[Int32] = null;
    new() { }
}

// A notebook cell.
//
// A cell's document URI must be unique across ALL notebook
// cells and can therefore be used to uniquely identify a
// notebook cell or the cell's text document.
//
// @since 3.17.0
def NotebookCell {
    // The cell's kind
    // NotebookCellKind
    let kind: Int32 = 0;
    // The URI of the cell's text document
    // content.
    // DocumentUri
    let document: String = null;
    // Additional metadata stored with the cell.
    //
    // Note: should always be an object literal (e.g. LSPObject)
    // LSPObject
    let metadata: Dictionary[String, Object] = null;
    // Additional execution summary information
    // if supported by the client.
    let executionSummary: ExecutionSummary = null;
    new() { }
}

// A change describing how to move a `NotebookCell`
// array from state S to S'.
//
// @since 3.17.0
def NotebookCellArrayChange {
    // The start oftest of the cell that changed.
    // uinteger
    let start: Int32 = 0;
    // The deleted cells
    // uinteger
    let deleteCount: Int32 = 0;
    // The new cells, if any
    // NotebookCell[]
    let cells: List[NotebookCell] = null;
    new() { }
}

// Describes the currently selected completion item.
//
// @since 3.18.0
// @proposed
def SelectedCompletionInfo {
    // The range that will be replaced if this completion item is accepted.
    let range: Range = null;
    // The text the range will be replaced with if this completion is accepted.
    // string
    let text: String = null;
    new() { }
}

// Defines the capabilities provided by the client.
def ClientCapabilities {
    // Workspace specific client capabilities.
    let workspace: WorkspaceClientCapabilities = null;
    // Text document specific client capabilities.
    let textDocument: TextDocumentClientCapabilities = null;
    // Capabilities specific to the notebook document support.
    //
    // @since 3.17.0
    let notebookDocument: NotebookDocumentClientCapabilities = null;
    // Window specific client capabilities.
    let window: WindowClientCapabilities = null;
    // General client capabilities.
    //
    // @since 3.16.0
    let general: GeneralClientCapabilities = null;
    // Experimental client capabilities.
    // LSPAny
    let experimental: Object = null;
    new() { }
}

def TextDocumentSyncOptions {
    // Open and close notifications are sent to the server. If omitted open close notification should not
    // be sent.
    // boolean
    let openClose: Nullable[Boolean] = null;
    // Change notifications are sent to the server. See TextDocumentSyncKind.None, TextDocumentSyncKind.Full
    // and TextDocumentSyncKind.Incremental. If omitted it defaults to TextDocumentSyncKind.None.
    // TextDocumentSyncKind
    let change: Nullable[Int32] = null;
    // If present will save notifications are sent to the server. If omitted the notification should not be
    // sent.
    // boolean
    let willSave: Nullable[Boolean] = null;
    // If present will save wait until requests are sent to the server. If omitted the request should not be
    // sent.
    // boolean
    let willSaveWaitUntil: Nullable[Boolean] = null;
    // If present save notifications are sent to the server. If omitted the notification should not be
    // sent.
    // boolean | SaveOptions
    let save: Object = null;
    new() { }
}

// Options specific to a notebook plus its cells
// to be synced to the server.
//
// If a selector provides a notebook document
// filter but no cell selector all cells of a
// matching notebook document will be synced.
//
// If a selector provides no notebook document
// filter but only a cell selector all notebook
// document that contain at least one matching
// cell will be synced.
//
// @since 3.17.0
def NotebookDocumentSyncOptions {
    // The notebooks to be synced
    // ({ ... } | { ... })[]
    let notebookSelector: List[Object] = null;
    // Whether save notification should be forwarded to
    // the server. Will only be honored if mode === `notebook`.
    // boolean
    let save: Nullable[Boolean] = null;
    new() { }
}

// Registration options specific to a notebook.
//
// @since 3.17.0
def NotebookDocumentSyncRegistrationOptions {
    // The notebooks to be synced
    // ({ ... } | { ... })[]
    let notebookSelector: List[Object] = null;
    // Whether save notification should be forwarded to
    // the server. Will only be honored if mode === `notebook`.
    // boolean
    let save: Nullable[Boolean] = null;
    // The id used to register the request. The id can be used to deregister
    // the request again. See also Registration#id.
    // string
    let id: String = null;
    new() { }
}

def WorkspaceFoldersServerCapabilities {
    // The server has support for workspace folders
    // boolean
    let supported: Nullable[Boolean] = null;
    // Whether the server wants to receive workspace folder
    // change notifications.
    //
    // If a string is provided the string is treated as an ID
    // under which the notification is registered on the client
    // side. The ID can be used to unregister for these events
    // using the `client/unregisterCapability` request.
    // string | boolean
    let changeNotifications: Object = null;
    new() { }
}

// Options for notifications/requests for user operations on files.
//
// @since 3.16.0
def FileOperationOptions {
    // The server is interested in receiving didCreateFiles notifications.
    let didCreate: FileOperationRegistrationOptions = null;
    // The server is interested in receiving willCreateFiles requests.
    let willCreate: FileOperationRegistrationOptions = null;
    // The server is interested in receiving didRenameFiles notifications.
    let didRename: FileOperationRegistrationOptions = null;
    // The server is interested in receiving willRenameFiles requests.
    let willRename: FileOperationRegistrationOptions = null;
    // The server is interested in receiving didDeleteFiles file notifications.
    let didDelete: FileOperationRegistrationOptions = null;
    // The server is interested in receiving willDeleteFiles file requests.
    let willDelete: FileOperationRegistrationOptions = null;
    new() { }
}

// Structure to capture a description for an error code.
//
// @since 3.16.0
def CodeDescription {
    // An URI to open with more information about the diagnostic error.
    // URI
    let href: String = null;
    new() { }
}

// Represents a related message and source code location for a diagnostic. This should be
// used to point to code locations that cause or related to a diagnostics, e.g when duplicating
// a symbol in a scope.
def DiagnosticRelatedInformation {
    // The location of this related diagnostic information.
    let location: Location = null;
    // The message of this related diagnostic information.
    // string
    let message: String = null;
    new() { }
}

// Represents a parameter of a callable-signature. A parameter can
// have a label and a doc-comment.
def ParameterInformation {
    // The label of this parameter information.
    //
    // Either a string or an inclusive start and exclusive end offsets within its containing
    // signature label. (see SignatureInformation.label). The offsets are based on a UTF-16
    // string representation as `Position` and `Range` does.
    //
    // *Note*: a label of type string should be a substring of its containing signature label.
    // Its intended use case is to highlight the parameter label part in the `SignatureInformation.label`.
    // string | [uinteger, uinteger]
    let label: Object = null;
    // The human-readable doc-comment of this parameter. Will be shown
    // in the UI but can be omitted.
    // string | MarkupContent
    let documentation: Object = null;
    new() { }
}

// A notebook cell text document filter denotes a cell text
// document by different properties.
//
// @since 3.17.0
def NotebookCellTextDocumentFilter {
    // A filter that matches against the notebook
    // containing the notebook cell. If a string
    // value is provided it matches against the
    // notebook type. '*' matches every notebook.
    // string | NotebookDocumentFilter
    let notebook: Object = null;
    // A language id like `python`.
    //
    // Will be matched against the language id of the
    // notebook cell document. '*' matches every language.
    // string
    let language: String = null;
    new() { }
}

// Matching options for the file operation pattern.
//
// @since 3.16.0
def FileOperationPatternOptions {
    // The pattern should be matched ignoring casing.
    // boolean
    let ignoreCase: Nullable[Boolean] = null;
    new() { }
}

def ExecutionSummary {
    // A strict monotonically increasing value
    // indicating the execution order of a cell
    // inside a notebook.
    // uinteger
    let executionOrder: Int32 = 0;
    // Whether the execution was successful or
    // not if known by the client.
    // boolean
    let success: Nullable[Boolean] = null;
    new() { }
}

// Workspace specific client capabilities.
def WorkspaceClientCapabilities {
    // The client supports applying batch edits
    // to the workspace by supporting the request
    // 'workspace/applyEdit'
    // boolean
    let applyEdit: Nullable[Boolean] = null;
    // Capabilities specific to `WorkspaceEdit`s.
    let workspaceEdit: WorkspaceEditClientCapabilities = null;
    // Capabilities specific to the `workspace/didChangeConfiguration` notification.
    let didChangeConfiguration: DidChangeConfigurationClientCapabilities = null;
    // Capabilities specific to the `workspace/didChangeWatchedFiles` notification.
    let didChangeWatchedFiles: DidChangeWatchedFilesClientCapabilities = null;
    // Capabilities specific to the `workspace/symbol` request.
    let symbol: WorkspaceSymbolClientCapabilities = null;
    // Capabilities specific to the `workspace/executeCommand` request.
    let executeCommand: ExecuteCommandClientCapabilities = null;
    // The client has support for workspace folders.
    //
    // @since 3.6.0
    // boolean
    let workspaceFolders: Nullable[Boolean] = null;
    // The client supports `workspace/configuration` requests.
    //
    // @since 3.6.0
    // boolean
    let configuration: Nullable[Boolean] = null;
    // Capabilities specific to the semantic token requests scoped to the
    // workspace.
    //
    // @since 3.16.0.
    let semanticTokens: SemanticTokensWorkspaceClientCapabilities = null;
    // Capabilities specific to the code lens requests scoped to the
    // workspace.
    //
    // @since 3.16.0.
    let codeLens: CodeLensWorkspaceClientCapabilities = null;
    // The client has support for file notifications/requests for user operations on files.
    //
    // Since 3.16.0
    let fileOperations: FileOperationClientCapabilities = null;
    // Capabilities specific to the inline values requests scoped to the
    // workspace.
    //
    // @since 3.17.0.
    let inlineValue: InlineValueWorkspaceClientCapabilities = null;
    // Capabilities specific to the inlay hint requests scoped to the
    // workspace.
    //
    // @since 3.17.0.
    let inlayHint: InlayHintWorkspaceClientCapabilities = null;
    // Capabilities specific to the diagnostic requests scoped to the
    // workspace.
    //
    // @since 3.17.0.
    let diagnostics: DiagnosticWorkspaceClientCapabilities = null;
    // Capabilities specific to the folding range requests scoped to the workspace.
    //
    // @since 3.18.0
    // @proposed
    let foldingRange: FoldingRangeWorkspaceClientCapabilities = null;
    new() { }
}

// Text document specific client capabilities.
def TextDocumentClientCapabilities {
    // Defines which synchronization capabilities the client supports.
    let synchronization: TextDocumentSyncClientCapabilities = null;
    // Capabilities specific to the `textDocument/completion` request.
    let completion: CompletionClientCapabilities = null;
    // Capabilities specific to the `textDocument/hover` request.
    let hover: HoverClientCapabilities = null;
    // Capabilities specific to the `textDocument/signatureHelp` request.
    let signatureHelp: SignatureHelpClientCapabilities = null;
    // Capabilities specific to the `textDocument/declaration` request.
    //
    // @since 3.14.0
    let declaration: DeclarationClientCapabilities = null;
    // Capabilities specific to the `textDocument/definition` request.
    let definition: DefinitionClientCapabilities = null;
    // Capabilities specific to the `textDocument/typeDefinition` request.
    //
    // @since 3.6.0
    let typeDefinition: TypeDefinitionClientCapabilities = null;
    // Capabilities specific to the `textDocument/implementation` request.
    //
    // @since 3.6.0
    let implementation: ImplementationClientCapabilities = null;
    // Capabilities specific to the `textDocument/references` request.
    let references: ReferenceClientCapabilities = null;
    // Capabilities specific to the `textDocument/documentHighlight` request.
    let documentHighlight: DocumentHighlightClientCapabilities = null;
    // Capabilities specific to the `textDocument/documentSymbol` request.
    let documentSymbol: DocumentSymbolClientCapabilities = null;
    // Capabilities specific to the `textDocument/codeAction` request.
    let codeAction: CodeActionClientCapabilities = null;
    // Capabilities specific to the `textDocument/codeLens` request.
    let codeLens: CodeLensClientCapabilities = null;
    // Capabilities specific to the `textDocument/documentLink` request.
    let documentLink: DocumentLinkClientCapabilities = null;
    // Capabilities specific to the `textDocument/documentColor` and the
    // `textDocument/colorPresentation` request.
    //
    // @since 3.6.0
    let colorProvider: DocumentColorClientCapabilities = null;
    // Capabilities specific to the `textDocument/formatting` request.
    let formatting: DocumentFormattingClientCapabilities = null;
    // Capabilities specific to the `textDocument/rangeFormatting` request.
    let rangeFormatting: DocumentRangeFormattingClientCapabilities = null;
    // Capabilities specific to the `textDocument/onTypeFormatting` request.
    let onTypeFormatting: DocumentOnTypeFormattingClientCapabilities = null;
    // Capabilities specific to the `textDocument/rename` request.
    let rename: RenameClientCapabilities = null;
    // Capabilities specific to the `textDocument/foldingRange` request.
    //
    // @since 3.10.0
    let foldingRange: FoldingRangeClientCapabilities = null;
    // Capabilities specific to the `textDocument/selectionRange` request.
    //
    // @since 3.15.0
    let selectionRange: SelectionRangeClientCapabilities = null;
    // Capabilities specific to the `textDocument/publishDiagnostics` notification.
    let publishDiagnostics: PublishDiagnosticsClientCapabilities = null;
    // Capabilities specific to the various call hierarchy requests.
    //
    // @since 3.16.0
    let callHierarchy: CallHierarchyClientCapabilities = null;
    // Capabilities specific to the various semantic token request.
    //
    // @since 3.16.0
    let semanticTokens: SemanticTokensClientCapabilities = null;
    // Capabilities specific to the `textDocument/linkedEditingRange` request.
    //
    // @since 3.16.0
    let linkedEditingRange: LinkedEditingRangeClientCapabilities = null;
    // Client capabilities specific to the `textDocument/moniker` request.
    //
    // @since 3.16.0
    let moniker: MonikerClientCapabilities = null;
    // Capabilities specific to the various type hierarchy requests.
    //
    // @since 3.17.0
    let typeHierarchy: TypeHierarchyClientCapabilities = null;
    // Capabilities specific to the `textDocument/inlineValue` request.
    //
    // @since 3.17.0
    let inlineValue: InlineValueClientCapabilities = null;
    // Capabilities specific to the `textDocument/inlayHint` request.
    //
    // @since 3.17.0
    let inlayHint: InlayHintClientCapabilities = null;
    // Capabilities specific to the diagnostic pull model.
    //
    // @since 3.17.0
    let diagnostic: DiagnosticClientCapabilities = null;
    // Client capabilities specific to inline completions.
    //
    // @since 3.18.0
    // @proposed
    let inlineCompletion: InlineCompletionClientCapabilities = null;
    new() { }
}

// Capabilities specific to the notebook document support.
//
// @since 3.17.0
def NotebookDocumentClientCapabilities {
    // Capabilities specific to notebook document synchronization
    //
    // @since 3.17.0
    let synchronization: NotebookDocumentSyncClientCapabilities = null;
    new() { }
}

def WindowClientCapabilities {
    // It indicates whether the client supports server initiated
    // progress using the `window/workDoneProgress/create` request.
    //
    // The capability also controls Whether client supports handling
    // of progress notifications. If set servers are allowed to report a
    // `workDoneProgress` property in the request specific server
    // capabilities.
    //
    // @since 3.15.0
    // boolean
    let workDoneProgress: Nullable[Boolean] = null;
    // Capabilities specific to the showMessage request.
    //
    // @since 3.16.0
    let showMessage: ShowMessageRequestClientCapabilities = null;
    // Capabilities specific to the showDocument request.
    //
    // @since 3.16.0
    let showDocument: ShowDocumentClientCapabilities = null;
    new() { }
}

// General client capabilities.
//
// @since 3.16.0
def GeneralClientCapabilities {
    // Client capability that signals how the client
    // handles stale requests (e.g. a request
    // for which the client will not process the response
    // anymore since the information is outdated).
    //
    // @since 3.17.0
    // { ... }
    let staleRequestSupport: GeneralClientCapabilitiesStaleRequestSupport = null;
    // Client capabilities specific to regular expressions.
    //
    // @since 3.16.0
    let regularExpressions: RegularExpressionsClientCapabilities = null;
    // Client capabilities specific to the client's markdown parser.
    //
    // @since 3.16.0
    let markdown: MarkdownClientCapabilities = null;
    // The position encodings supported by the client. Client and server
    // have to agree on the same position encoding to ensure that offsets
    // (e.g. character position in a line) are interpreted the same on both
    // sides.
    //
    // To keep the protocol backwards compatible the following applies: if
    // the value 'utf-16' is missing from the array of position encodings
    // servers can assume that the client supports UTF-16. UTF-16 is
    // therefore a mandatory encoding.
    //
    // If omitted it defaults to ['utf-16'].
    //
    // Implementation considerations: since the conversion from one encoding
    // into another requires the content of the file / line the conversion
    // is best done where the file is read which is usually on the server
    // side.
    //
    // @since 3.17.0
    // PositionEncodingKind[]
    let positionEncodings: List[String] = null;
    new() { }
}

// A relative pattern is a helper to construct glob patterns that are matched
// relatively to a base URI. The common value for a `baseUri` is a workspace
// folder root, but it can be another absolute URI as well.
//
// @since 3.17.0
def RelativePattern {
    // A workspace folder or a base URI to which this pattern will be matched
    // against relatively.
    // WorkspaceFolder | URI
    let baseUri: Object = null;
    // The actual glob pattern;
    // Pattern
    let pattern: String = null;
    new() { }
}

def WorkspaceEditClientCapabilities {
    // The client supports versioned document changes in `WorkspaceEdit`s
    // boolean
    let documentChanges: Nullable[Boolean] = null;
    // The resource operations the client supports. Clients should at least
    // support 'create', 'rename' and 'delete' files and folders.
    //
    // @since 3.13.0
    // ResourceOperationKind[]
    let resourceOperations: List[String] = null;
    // The failure handling strategy of a client if applying the workspace edit
    // fails.
    //
    // @since 3.13.0
    // FailureHandlingKind
    let failureHandling: String = null;
    // Whether the client normalizes line endings to the client specific
    // setting.
    // If set to `true` the client will normalize line ending characters
    // in a workspace edit to the client-specified new line
    // character.
    //
    // @since 3.16.0
    // boolean
    let normalizesLineEndings: Nullable[Boolean] = null;
    // Whether the client in general supports change annotations on text edits,
    // create file, rename file and delete file changes.
    //
    // @since 3.16.0
    // { ... }
    let changeAnnotationSupport: WorkspaceEditClientCapabilitiesChangeAnnotationSupport = null;
    new() { }
}

def DidChangeConfigurationClientCapabilities {
    // Did change configuration notification supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

def DidChangeWatchedFilesClientCapabilities {
    // Did change watched files notification supports dynamic registration. Please note
    // that the current protocol doesn't support static configuration for file changes
    // from the server side.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Whether the client has support for {@link  RelativePattern relative pattern}
    // or not.
    //
    // @since 3.17.0
    // boolean
    let relativePatternSupport: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities for a {@link WorkspaceSymbolRequest}.
def WorkspaceSymbolClientCapabilities {
    // Symbol request supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Specific capabilities for the `SymbolKind` in the `workspace/symbol` request.
    // { ... }
    let symbolKind: WorkspaceSymbolClientCapabilitiesSymbolKind = null;
    // The client supports tags on `SymbolInformation`.
    // Clients supporting tags have to handle unknown tags gracefully.
    //
    // @since 3.16.0
    // { ... }
    let tagSupport: WorkspaceSymbolClientCapabilitiesTagSupport = null;
    // The client support partial workspace symbols. The client will send the
    // request `workspaceSymbol/resolve` to the server to resolve additional
    // properties.
    //
    // @since 3.17.0
    // { ... }
    let resolveSupport: WorkspaceSymbolClientCapabilitiesResolveSupport = null;
    new() { }
}

// The client capabilities of a {@link ExecuteCommandRequest}.
def ExecuteCommandClientCapabilities {
    // Execute command supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from
    // the server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // semantic tokens currently shown. It should be used with absolute care
    // and is useful for situation where a server for example detects a project
    // wide change that requires such a calculation.
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

// @since 3.16.0
def CodeLensWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from the
    // server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // code lenses currently shown. It should be used with absolute care and is
    // useful for situation where a server for example detect a project wide
    // change that requires such a calculation.
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

// Capabilities relating to events from file operations by the user in the client.
//
// These events do not come from the file system, they come from user operations
// like renaming a file in the UI.
//
// @since 3.16.0
def FileOperationClientCapabilities {
    // Whether the client supports dynamic registration for file requests/notifications.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client has support for sending didCreateFiles notifications.
    // boolean
    let didCreate: Nullable[Boolean] = null;
    // The client has support for sending willCreateFiles requests.
    // boolean
    let willCreate: Nullable[Boolean] = null;
    // The client has support for sending didRenameFiles notifications.
    // boolean
    let didRename: Nullable[Boolean] = null;
    // The client has support for sending willRenameFiles requests.
    // boolean
    let willRename: Nullable[Boolean] = null;
    // The client has support for sending didDeleteFiles notifications.
    // boolean
    let didDelete: Nullable[Boolean] = null;
    // The client has support for sending willDeleteFiles requests.
    // boolean
    let willDelete: Nullable[Boolean] = null;
    new() { }
}

// Client workspace capabilities specific to inline values.
//
// @since 3.17.0
def InlineValueWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from the
    // server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // inline values currently shown. It should be used with absolute care and is
    // useful for situation where a server for example detects a project wide
    // change that requires such a calculation.
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

// Client workspace capabilities specific to inlay hints.
//
// @since 3.17.0
def InlayHintWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from
    // the server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // inlay hints currently shown. It should be used with absolute care and
    // is useful for situation where a server for example detects a project wide
    // change that requires such a calculation.
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

// Workspace client capabilities specific to diagnostic pull requests.
//
// @since 3.17.0
def DiagnosticWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from
    // the server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // pulled diagnostics currently shown. It should be used with absolute care and
    // is useful for situation where a server for example detects a project wide
    // change that requires such a calculation.
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

// Client workspace capabilities specific to folding ranges
//
// @since 3.18.0
// @proposed
def FoldingRangeWorkspaceClientCapabilities {
    // Whether the client implementation supports a refresh request sent from the
    // server to the client.
    //
    // Note that this event is global and will force the client to refresh all
    // folding ranges currently shown. It should be used with absolute care and is
    // useful for situation where a server for example detects a project wide
    // change that requires such a calculation.
    //
    // @since 3.18.0
    // @proposed
    // boolean
    let refreshSupport: Nullable[Boolean] = null;
    new() { }
}

def TextDocumentSyncClientCapabilities {
    // Whether text document synchronization supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports sending will save notifications.
    // boolean
    let willSave: Nullable[Boolean] = null;
    // The client supports sending a will save request and
    // waits for a response providing text edits which will
    // be applied to the document before it is saved.
    // boolean
    let willSaveWaitUntil: Nullable[Boolean] = null;
    // The client supports did save notifications.
    // boolean
    let didSave: Nullable[Boolean] = null;
    new() { }
}

// Completion client capabilities
def CompletionClientCapabilities {
    // Whether completion supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports the following `CompletionItem` specific
    // capabilities.
    // { ... }
    let completionItem: CompletionClientCapabilitiesCompletionItem = null;
    // { ... }
    let completionItemKind: CompletionClientCapabilitiesCompletionItemKind = null;
    // Defines how the client handles whitespace and indentation
    // when accepting a completion item that uses multi line
    // text in either `insertText` or `textEdit`.
    //
    // @since 3.17.0
    // InsertTextMode
    let insertTextMode: Nullable[Int32] = null;
    // The client supports to send additional context information for a
    // `textDocument/completion` request.
    // boolean
    let contextSupport: Nullable[Boolean] = null;
    // The client supports the following `CompletionList` specific
    // capabilities.
    //
    // @since 3.17.0
    // { ... }
    let completionList: CompletionClientCapabilitiesCompletionList = null;
    new() { }
}

def HoverClientCapabilities {
    // Whether hover supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Client supports the following content formats for the content
    // property. The order describes the preferred format of the client.
    // MarkupKind[]
    let contentFormat: List[String] = null;
    new() { }
}

// Client Capabilities for a {@link SignatureHelpRequest}.
def SignatureHelpClientCapabilities {
    // Whether signature help supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports the following `SignatureInformation`
    // specific properties.
    // { ... }
    let signatureInformation: SignatureHelpClientCapabilitiesSignatureInformation = null;
    // The client supports to send additional context information for a
    // `textDocument/signatureHelp` request. A client that opts into
    // contextSupport will also support the `retriggerCharacters` on
    // `SignatureHelpOptions`.
    //
    // @since 3.15.0
    // boolean
    let contextSupport: Nullable[Boolean] = null;
    new() { }
}

// @since 3.14.0
def DeclarationClientCapabilities {
    // Whether declaration supports dynamic registration. If this is set to `true`
    // the client supports the new `DeclarationRegistrationOptions` return value
    // for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports additional metadata in the form of declaration links.
    // boolean
    let linkSupport: Nullable[Boolean] = null;
    new() { }
}

// Client Capabilities for a {@link DefinitionRequest}.
def DefinitionClientCapabilities {
    // Whether definition supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports additional metadata in the form of definition links.
    //
    // @since 3.14.0
    // boolean
    let linkSupport: Nullable[Boolean] = null;
    new() { }
}

// Since 3.6.0
def TypeDefinitionClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `TypeDefinitionRegistrationOptions` return value
    // for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports additional metadata in the form of definition links.
    //
    // Since 3.14.0
    // boolean
    let linkSupport: Nullable[Boolean] = null;
    new() { }
}

// @since 3.6.0
def ImplementationClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `ImplementationRegistrationOptions` return value
    // for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports additional metadata in the form of definition links.
    //
    // @since 3.14.0
    // boolean
    let linkSupport: Nullable[Boolean] = null;
    new() { }
}

// Client Capabilities for a {@link ReferencesRequest}.
def ReferenceClientCapabilities {
    // Whether references supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client Capabilities for a {@link DocumentHighlightRequest}.
def DocumentHighlightClientCapabilities {
    // Whether document highlight supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client Capabilities for a {@link DocumentSymbolRequest}.
def DocumentSymbolClientCapabilities {
    // Whether document symbol supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Specific capabilities for the `SymbolKind` in the
    // `textDocument/documentSymbol` request.
    // { ... }
    let symbolKind: DocumentSymbolClientCapabilitiesSymbolKind = null;
    // The client supports hierarchical document symbols.
    // boolean
    let hierarchicalDocumentSymbolSupport: Nullable[Boolean] = null;
    // The client supports tags on `SymbolInformation`. Tags are supported on
    // `DocumentSymbol` if `hierarchicalDocumentSymbolSupport` is set to true.
    // Clients supporting tags have to handle unknown tags gracefully.
    //
    // @since 3.16.0
    // { ... }
    let tagSupport: DocumentSymbolClientCapabilitiesTagSupport = null;
    // The client supports an additional label presented in the UI when
    // registering a document symbol provider.
    //
    // @since 3.16.0
    // boolean
    let labelSupport: Nullable[Boolean] = null;
    new() { }
}

// The Client Capabilities of a {@link CodeActionRequest}.
def CodeActionClientCapabilities {
    // Whether code action supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client support code action literals of type `CodeAction` as a valid
    // response of the `textDocument/codeAction` request. If the property is not
    // set the request can only return `Command` literals.
    //
    // @since 3.8.0
    // { ... }
    let codeActionLiteralSupport: CodeActionClientCapabilitiesCodeActionLiteralSupport = null;
    // Whether code action supports the `isPreferred` property.
    //
    // @since 3.15.0
    // boolean
    let isPreferredSupport: Nullable[Boolean] = null;
    // Whether code action supports the `disabled` property.
    //
    // @since 3.16.0
    // boolean
    let disabledSupport: Nullable[Boolean] = null;
    // Whether code action supports the `data` property which is
    // preserved between a `textDocument/codeAction` and a
    // `codeAction/resolve` request.
    //
    // @since 3.16.0
    // boolean
    let dataSupport: Nullable[Boolean] = null;
    // Whether the client supports resolving additional code action
    // properties via a separate `codeAction/resolve` request.
    //
    // @since 3.16.0
    // { ... }
    let resolveSupport: CodeActionClientCapabilitiesResolveSupport = null;
    // Whether the client honors the change annotations in
    // text edits and resource operations returned via the
    // `CodeAction#edit` property by for example presenting
    // the workspace edit in the user interface and asking
    // for confirmation.
    //
    // @since 3.16.0
    // boolean
    let honorsChangeAnnotations: Nullable[Boolean] = null;
    new() { }
}

// The client capabilities  of a {@link CodeLensRequest}.
def CodeLensClientCapabilities {
    // Whether code lens supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// The client capabilities of a {@link DocumentLinkRequest}.
def DocumentLinkClientCapabilities {
    // Whether document link supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Whether the client supports the `tooltip` property on `DocumentLink`.
    //
    // @since 3.15.0
    // boolean
    let tooltipSupport: Nullable[Boolean] = null;
    new() { }
}

def DocumentColorClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `DocumentColorRegistrationOptions` return value
    // for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities of a {@link DocumentFormattingRequest}.
def DocumentFormattingClientCapabilities {
    // Whether formatting supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities of a {@link DocumentRangeFormattingRequest}.
def DocumentRangeFormattingClientCapabilities {
    // Whether range formatting supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Whether the client supports formatting multiple ranges at once.
    //
    // @since 3.18.0
    // @proposed
    // boolean
    let rangesSupport: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities of a {@link DocumentOnTypeFormattingRequest}.
def DocumentOnTypeFormattingClientCapabilities {
    // Whether on type formatting supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

def RenameClientCapabilities {
    // Whether rename supports dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Client supports testing for validity of rename operations
    // before execution.
    //
    // @since 3.12.0
    // boolean
    let prepareSupport: Nullable[Boolean] = null;
    // Client supports the default behavior result.
    //
    // The value indicates the default behavior used by the
    // client.
    //
    // @since 3.16.0
    // PrepareSupportDefaultBehavior
    let prepareSupportDefaultBehavior: Nullable[Int32] = null;
    // Whether the client honors the change annotations in
    // text edits and resource operations returned via the
    // rename request's workspace edit by for example presenting
    // the workspace edit in the user interface and asking
    // for confirmation.
    //
    // @since 3.16.0
    // boolean
    let honorsChangeAnnotations: Nullable[Boolean] = null;
    new() { }
}

def FoldingRangeClientCapabilities {
    // Whether implementation supports dynamic registration for folding range
    // providers. If this is set to `true` the client supports the new
    // `FoldingRangeRegistrationOptions` return value for the corresponding
    // server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The maximum number of folding ranges that the client prefers to receive
    // per document. The value serves as a hint, servers are free to follow the
    // limit.
    // uinteger
    let rangeLimit: Nullable[Int32] = null;
    // If set, the client signals that it only supports folding complete lines.
    // If set, client will ignore specified `startCharacter` and `endCharacter`
    // properties in a FoldingRange.
    // boolean
    let lineFoldingOnly: Nullable[Boolean] = null;
    // Specific options for the folding range kind.
    //
    // @since 3.17.0
    // { ... }
    let foldingRangeKind: FoldingRangeClientCapabilitiesFoldingRangeKind = null;
    // Specific options for the folding range.
    //
    // @since 3.17.0
    // { ... }
    let foldingRange: FoldingRangeClientCapabilitiesFoldingRange = null;
    new() { }
}

def SelectionRangeClientCapabilities {
    // Whether implementation supports dynamic registration for selection range providers. If this is set to `true`
    // the client supports the new `SelectionRangeRegistrationOptions` return value for the corresponding server
    // capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// The publish diagnostic client capabilities.
def PublishDiagnosticsClientCapabilities {
    // Whether the clients accepts diagnostics with related information.
    // boolean
    let relatedInformation: Nullable[Boolean] = null;
    // Client supports the tag property to provide meta data about a diagnostic.
    // Clients supporting tags have to handle unknown tags gracefully.
    //
    // @since 3.15.0
    // { ... }
    let tagSupport: PublishDiagnosticsClientCapabilitiesTagSupport = null;
    // Whether the client interprets the version property of the
    // `textDocument/publishDiagnostics` notification's parameter.
    //
    // @since 3.15.0
    // boolean
    let versionSupport: Nullable[Boolean] = null;
    // Client supports a codeDescription property
    //
    // @since 3.16.0
    // boolean
    let codeDescriptionSupport: Nullable[Boolean] = null;
    // Whether code action supports the `data` property which is
    // preserved between a `textDocument/publishDiagnostics` and
    // `textDocument/codeAction` request.
    //
    // @since 3.16.0
    // boolean
    let dataSupport: Nullable[Boolean] = null;
    new() { }
}

// @since 3.16.0
def CallHierarchyClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// @since 3.16.0
def SemanticTokensClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Which requests the client supports and might send to the server
    // depending on the server's capability. Please note that clients might not
    // show semantic tokens or degrade some of the user experience if a range
    // or full request is advertised by the client but not provided by the
    // server. If for example the client capability `requests.full` and
    // `request.range` are both set to true but the server only provides a
    // range provider the client might not render a minimap correctly or might
    // even decide to not show any semantic tokens at all.
    // { ... }
    let requests: SemanticTokensClientCapabilitiesRequests = null;
    // The token types that the client supports.
    // string[]
    let tokenTypes: List[String] = null;
    // The token modifiers that the client supports.
    // string[]
    let tokenModifiers: List[String] = null;
    // The token formats the clients supports.
    // TokenFormat[]
    let formats: List[String] = null;
    // Whether the client supports tokens that can overlap each other.
    // boolean
    let overlappingTokenSupport: Nullable[Boolean] = null;
    // Whether the client supports tokens that can span multiple lines.
    // boolean
    let multilineTokenSupport: Nullable[Boolean] = null;
    // Whether the client allows the server to actively cancel a
    // semantic token request, e.g. supports returning
    // LSPErrorCodes.ServerCancelled. If a server does the client
    // needs to retrigger the request.
    //
    // @since 3.17.0
    // boolean
    let serverCancelSupport: Nullable[Boolean] = null;
    // Whether the client uses semantic tokens to augment existing
    // syntax tokens. If set to `true` client side created syntax
    // tokens and semantic tokens are both used for colorization. If
    // set to `false` the client only uses the returned semantic tokens
    // for colorization.
    //
    // If the value is `undefined` then the client behavior is not
    // specified.
    //
    // @since 3.17.0
    // boolean
    let augmentsSyntaxTokens: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities for the linked editing range request.
//
// @since 3.16.0
def LinkedEditingRangeClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities specific to the moniker request.
//
// @since 3.16.0
def MonikerClientCapabilities {
    // Whether moniker supports dynamic registration. If this is set to `true`
    // the client supports the new `MonikerRegistrationOptions` return value
    // for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// @since 3.17.0
def TypeHierarchyClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities specific to inline values.
//
// @since 3.17.0
def InlineValueClientCapabilities {
    // Whether implementation supports dynamic registration for inline value providers.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Inlay hint client capabilities.
//
// @since 3.17.0
def InlayHintClientCapabilities {
    // Whether inlay hints support dynamic registration.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Indicates which properties a client can resolve lazily on an inlay
    // hint.
    // { ... }
    let resolveSupport: InlayHintClientCapabilitiesResolveSupport = null;
    new() { }
}

// Client capabilities specific to diagnostic pull requests.
//
// @since 3.17.0
def DiagnosticClientCapabilities {
    // Whether implementation supports dynamic registration. If this is set to `true`
    // the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // Whether the clients supports related documents for document diagnostic pulls.
    // boolean
    let relatedDocumentSupport: Nullable[Boolean] = null;
    new() { }
}

// Client capabilities specific to inline completions.
//
// @since 3.18.0
// @proposed
def InlineCompletionClientCapabilities {
    // Whether implementation supports dynamic registration for inline completion providers.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    new() { }
}

// Notebook specific client capabilities.
//
// @since 3.17.0
def NotebookDocumentSyncClientCapabilities {
    // Whether implementation supports dynamic registration. If this is
    // set to `true` the client supports the new
    // `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
    // return value for the corresponding server capability as well.
    // boolean
    let dynamicRegistration: Nullable[Boolean] = null;
    // The client supports sending execution summary data per cell.
    // boolean
    let executionSummarySupport: Nullable[Boolean] = null;
    new() { }
}

// Show message request client capabilities
def ShowMessageRequestClientCapabilities {
    // Capabilities specific to the `MessageActionItem` type.
    // { ... }
    let messageActionItem: ShowMessageRequestClientCapabilitiesMessageActionItem = null;
    new() { }
}

// Client capabilities for the showDocument request.
//
// @since 3.16.0
def ShowDocumentClientCapabilities {
    // The client has support for the showDocument
    // request.
    // boolean
    let support: Boolean = false;
    new() { }
}

// Client capabilities specific to regular expressions.
//
// @since 3.16.0
def RegularExpressionsClientCapabilities {
    // The engine's name.
    // string
    let engine: String = null;
    // The engine's version.
    // string
    let version: String = null;
    new() { }
}

// Client capabilities specific to the used markdown parser.
//
// @since 3.16.0
def MarkdownClientCapabilities {
    // The name of the parser.
    // string
    let parser: String = null;
    // The version of the parser.
    // string
    let version: String = null;
    // A list of HTML tags that the client allows / supports in
    // Markdown.
    //
    // @since 3.17.0
    // string[]
    let allowedTags: List[String] = null;
    new() { }
}

// Literal structures the protocol writes inline, each named by what holds it.

// (a literal structure of InitializeParams)
def InitializeParamsClientInfo {
    // The name of the client as defined by the client.
    // string
    let name: String = null;
    // The client's version as defined by the client.
    // string
    let version: String = null;
    new() { }
}

// (a literal structure of InitializeResult)
def InitializeResultServerInfo {
    // The name of the server as defined by the server.
    // string
    let name: String = null;
    // The server's version as defined by the server.
    // string
    let version: String = null;
    new() { }
}

// (a literal structure of CompletionList)
def CompletionListItemDefaults {
    // A default commit character set.
    //
    // @since 3.17.0
    // string[]
    let commitCharacters: List[String] = null;
    // A default edit range.
    //
    // @since 3.17.0
    // Range | { ... }
    let editRange: Object = null;
    // A default insert text format.
    //
    // @since 3.17.0
    // InsertTextFormat
    let insertTextFormat: Nullable[Int32] = null;
    // A default insert text mode.
    //
    // @since 3.17.0
    // InsertTextMode
    let insertTextMode: Nullable[Int32] = null;
    // A default data value.
    //
    // @since 3.17.0
    // LSPAny
    let data: Object = null;
    new() { }
}

// (a literal structure of CompletionRegistrationOptions)
def CompletionRegistrationOptionsCompletionItem {
    // The server has support for completion item label
    // details (see also `CompletionItemLabelDetails`) when
    // receiving a completion item in a resolve call.
    //
    // @since 3.17.0
    // boolean
    let labelDetailsSupport: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of CodeAction)
def CodeActionDisabled {
    // Human readable description of why the code action is currently disabled.
    //
    // This is displayed in the code actions UI.
    // string
    let reason: String = null;
    new() { }
}

// (a literal structure of NotebookDocumentChangeEventCells)
def NotebookDocumentChangeEventCellsStructure {
    // The change to the cell array.
    let array: NotebookCellArrayChange = null;
    // Additional opened cell text documents.
    // TextDocumentItem[]
    let didOpen: List[TextDocumentItem] = null;
    // Additional closed cell text documents.
    // TextDocumentIdentifier[]
    let didClose: List[TextDocumentIdentifier] = null;
    new() { }
}

// (a literal structure of NotebookDocumentChangeEventCells)
def NotebookDocumentChangeEventCellsTextContentItem {
    let document: VersionedTextDocumentIdentifier = null;
    // TextDocumentContentChangeEvent[]
    let changes: List[Object] = null;
    new() { }
}

// (a literal structure of NotebookDocumentChangeEvent)
def NotebookDocumentChangeEventCells {
    // Changes to the cell structure to add or
    // remove cells.
    // { ... }
    let structure: NotebookDocumentChangeEventCellsStructure = null;
    // Changes to notebook cells properties like its
    // kind, execution summary or metadata.
    // NotebookCell[]
    let data: List[NotebookCell] = null;
    // Changes to the text content of notebook cells.
    // { ... }[]
    let textContent: List[NotebookDocumentChangeEventCellsTextContentItem] = null;
    new() { }
}

// (a literal structure of _InitializeParams)
def _InitializeParamsClientInfo {
    // The name of the client as defined by the client.
    // string
    let name: String = null;
    // The client's version as defined by the client.
    // string
    let version: String = null;
    new() { }
}

// (a literal structure of ServerCapabilities)
def ServerCapabilitiesWorkspace {
    // The server supports workspace folder.
    //
    // @since 3.6.0
    let workspaceFolders: WorkspaceFoldersServerCapabilities = null;
    // The server is interested in notifications/requests for operations on files.
    //
    // @since 3.16.0
    let fileOperations: FileOperationOptions = null;
    new() { }
}

// (a literal structure of CompletionOptions)
def CompletionOptionsCompletionItem {
    // The server has support for completion item label
    // details (see also `CompletionItemLabelDetails`) when
    // receiving a completion item in a resolve call.
    //
    // @since 3.17.0
    // boolean
    let labelDetailsSupport: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of GeneralClientCapabilities)
def GeneralClientCapabilitiesStaleRequestSupport {
    // The client will actively cancel the request.
    // boolean
    let cancel: Boolean = false;
    // The list of requests for which the client
    // will retry the request if it receives a
    // response with error code `ContentModified`
    // string[]
    let retryOnContentModified: List[String] = null;
    new() { }
}

// (a literal structure of WorkspaceEditClientCapabilities)
def WorkspaceEditClientCapabilitiesChangeAnnotationSupport {
    // Whether the client groups edits with equal labels into tree nodes,
    // for instance all edits labelled with "Changes in Strings" would
    // be a tree node.
    // boolean
    let groupsOnLabel: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of WorkspaceSymbolClientCapabilities)
def WorkspaceSymbolClientCapabilitiesSymbolKind {
    // The symbol kind values the client supports. When this
    // property exists the client also guarantees that it will
    // handle values outside its set gracefully and falls back
    // to a default value when unknown.
    //
    // If this property is not present the client only supports
    // the symbol kinds from `File` to `Array` as defined in
    // the initial version of the protocol.
    // SymbolKind[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of WorkspaceSymbolClientCapabilities)
def WorkspaceSymbolClientCapabilitiesTagSupport {
    // The tags supported by the client.
    // SymbolTag[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of WorkspaceSymbolClientCapabilities)
def WorkspaceSymbolClientCapabilitiesResolveSupport {
    // The properties that a client can resolve lazily. Usually
    // `location.range`
    // string[]
    let properties: List[String] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilitiesCompletionItem)
def CompletionClientCapabilitiesCompletionItemTagSupport {
    // The tags supported by the client.
    // CompletionItemTag[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilitiesCompletionItem)
def CompletionClientCapabilitiesCompletionItemResolveSupport {
    // The properties that a client can resolve lazily.
    // string[]
    let properties: List[String] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilitiesCompletionItem)
def CompletionClientCapabilitiesCompletionItemInsertTextModeSupport {
    // InsertTextMode[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilities)
def CompletionClientCapabilitiesCompletionItem {
    // Client supports snippets as insert text.
    //
    // A snippet can define tab stops and placeholders with `$1`, `$2`
    // and `${3:foo}`. `$0` defines the final tab stop, it defaults to
    // the end of the snippet. Placeholders with equal identifiers are linked,
    // that is typing in one will update others too.
    // boolean
    let snippetSupport: Nullable[Boolean] = null;
    // Client supports commit characters on a completion item.
    // boolean
    let commitCharactersSupport: Nullable[Boolean] = null;
    // Client supports the following content formats for the documentation
    // property. The order describes the preferred format of the client.
    // MarkupKind[]
    let documentationFormat: List[String] = null;
    // Client supports the deprecated property on a completion item.
    // boolean
    let deprecatedSupport: Nullable[Boolean] = null;
    // Client supports the preselect property on a completion item.
    // boolean
    let preselectSupport: Nullable[Boolean] = null;
    // Client supports the tag property on a completion item. Clients supporting
    // tags have to handle unknown tags gracefully. Clients especially need to
    // preserve unknown tags when sending a completion item back to the server in
    // a resolve call.
    //
    // @since 3.15.0
    // { ... }
    let tagSupport: CompletionClientCapabilitiesCompletionItemTagSupport = null;
    // Client support insert replace edit to control different behavior if a
    // completion item is inserted in the text or should replace text.
    //
    // @since 3.16.0
    // boolean
    let insertReplaceSupport: Nullable[Boolean] = null;
    // Indicates which properties a client can resolve lazily on a completion
    // item. Before version 3.16.0 only the predefined properties `documentation`
    // and `details` could be resolved lazily.
    //
    // @since 3.16.0
    // { ... }
    let resolveSupport: CompletionClientCapabilitiesCompletionItemResolveSupport = null;
    // The client supports the `insertTextMode` property on
    // a completion item to override the whitespace handling mode
    // as defined by the client (see `insertTextMode`).
    //
    // @since 3.16.0
    // { ... }
    let insertTextModeSupport: CompletionClientCapabilitiesCompletionItemInsertTextModeSupport = null;
    // The client has support for completion item label
    // details (see also `CompletionItemLabelDetails`).
    //
    // @since 3.17.0
    // boolean
    let labelDetailsSupport: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilities)
def CompletionClientCapabilitiesCompletionItemKind {
    // The completion item kind values the client supports. When this
    // property exists the client also guarantees that it will
    // handle values outside its set gracefully and falls back
    // to a default value when unknown.
    //
    // If this property is not present the client only supports
    // the completion items kinds from `Text` to `Reference` as defined in
    // the initial version of the protocol.
    // CompletionItemKind[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of CompletionClientCapabilities)
def CompletionClientCapabilitiesCompletionList {
    // The client supports the following itemDefaults on
    // a completion list.
    //
    // The value lists the supported property names of the
    // `CompletionList.itemDefaults` object. If omitted
    // no properties are supported.
    //
    // @since 3.17.0
    // string[]
    let itemDefaults: List[String] = null;
    new() { }
}

// (a literal structure of SignatureHelpClientCapabilitiesSignatureInformation)
def SignatureHelpClientCapabilitiesSignatureInformationParameterInformation {
    // The client supports processing label offsets instead of a
    // simple label string.
    //
    // @since 3.14.0
    // boolean
    let labelOffsetSupport: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of SignatureHelpClientCapabilities)
def SignatureHelpClientCapabilitiesSignatureInformation {
    // Client supports the following content formats for the documentation
    // property. The order describes the preferred format of the client.
    // MarkupKind[]
    let documentationFormat: List[String] = null;
    // Client capabilities specific to parameter information.
    // { ... }
    let parameterInformation: SignatureHelpClientCapabilitiesSignatureInformationParameterInformation = null;
    // The client supports the `activeParameter` property on `SignatureInformation`
    // literal.
    //
    // @since 3.16.0
    // boolean
    let activeParameterSupport: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of DocumentSymbolClientCapabilities)
def DocumentSymbolClientCapabilitiesSymbolKind {
    // The symbol kind values the client supports. When this
    // property exists the client also guarantees that it will
    // handle values outside its set gracefully and falls back
    // to a default value when unknown.
    //
    // If this property is not present the client only supports
    // the symbol kinds from `File` to `Array` as defined in
    // the initial version of the protocol.
    // SymbolKind[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of DocumentSymbolClientCapabilities)
def DocumentSymbolClientCapabilitiesTagSupport {
    // The tags supported by the client.
    // SymbolTag[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of CodeActionClientCapabilitiesCodeActionLiteralSupport)
def CodeActionClientCapabilitiesCodeActionLiteralSupportCodeActionKind {
    // The code action kind values the client supports. When this
    // property exists the client also guarantees that it will
    // handle values outside its set gracefully and falls back
    // to a default value when unknown.
    // CodeActionKind[]
    let valueSet: List[String] = null;
    new() { }
}

// (a literal structure of CodeActionClientCapabilities)
def CodeActionClientCapabilitiesCodeActionLiteralSupport {
    // The code action kind is support with the following value
    // set.
    // { ... }
    let codeActionKind: CodeActionClientCapabilitiesCodeActionLiteralSupportCodeActionKind = null;
    new() { }
}

// (a literal structure of CodeActionClientCapabilities)
def CodeActionClientCapabilitiesResolveSupport {
    // The properties that a client can resolve lazily.
    // string[]
    let properties: List[String] = null;
    new() { }
}

// (a literal structure of FoldingRangeClientCapabilities)
def FoldingRangeClientCapabilitiesFoldingRangeKind {
    // The folding range kind values the client supports. When this
    // property exists the client also guarantees that it will
    // handle values outside its set gracefully and falls back
    // to a default value when unknown.
    // FoldingRangeKind[]
    let valueSet: List[String] = null;
    new() { }
}

// (a literal structure of FoldingRangeClientCapabilities)
def FoldingRangeClientCapabilitiesFoldingRange {
    // If set, the client signals that it supports setting collapsedText on
    // folding ranges to display custom labels instead of the default text.
    //
    // @since 3.17.0
    // boolean
    let collapsedText: Nullable[Boolean] = null;
    new() { }
}

// (a literal structure of PublishDiagnosticsClientCapabilities)
def PublishDiagnosticsClientCapabilitiesTagSupport {
    // The tags supported by the client.
    // DiagnosticTag[]
    let valueSet: List[Int32] = null;
    new() { }
}

// (a literal structure of SemanticTokensClientCapabilities)
def SemanticTokensClientCapabilitiesRequests {
    // The client will send the `textDocument/semanticTokens/range` request if
    // the server provides a corresponding handler.
    // boolean | { ... }
    let range: Object = null;
    // The client will send the `textDocument/semanticTokens/full` request if
    // the server provides a corresponding handler.
    // boolean | { ... }
    let full: Object = null;
    new() { }
}

// (a literal structure of InlayHintClientCapabilities)
def InlayHintClientCapabilitiesResolveSupport {
    // The properties that a client can resolve lazily.
    // string[]
    let properties: List[String] = null;
    new() { }
}

// (a literal structure of ShowMessageRequestClientCapabilities)
def ShowMessageRequestClientCapabilitiesMessageActionItem {
    // Whether the client supports additional attributes which
    // are preserved and send back to the server in the
    // request's response.
    // boolean
    let additionalPropertiesSupport: Nullable[Boolean] = null;
    new() { }
}

