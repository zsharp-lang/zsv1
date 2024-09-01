namespace ZSharp.AST
{
    public sealed class ImportedName
    {
        public required string Name { get; set; }

        public string? Alias { get; set; }
    }
}
