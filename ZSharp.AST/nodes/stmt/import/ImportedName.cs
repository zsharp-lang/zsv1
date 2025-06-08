namespace ZSharp.AST
{
    public sealed class ImportedName : Node
    {
        public required string Name { get; set; }

        public string? Alias { get; set; }
    }
}
