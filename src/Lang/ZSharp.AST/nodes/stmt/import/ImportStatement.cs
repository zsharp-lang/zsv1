namespace ZSharp.AST
{
    public sealed class ImportStatement(ImportTokens tokens) : Statement(tokens)
    {
        public new ImportTokens TokenInfo => As<ImportTokens>();

        public List<CallArgument>? Arguments { get; set; }

        public List<ImportedName>? ImportedNames { get; set; }

        public string? Alias { get; set; }

        public required Expression Source { get; set; }
    }
}
