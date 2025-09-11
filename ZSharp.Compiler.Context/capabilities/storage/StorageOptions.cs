namespace ZSharp.Compiler
{
    public sealed class StorageOptions
    {
        public string Name { get; init; } = string.Empty;

        public required  CompilerObject Type { get; init; }

        public CompilerObject? Initializer { get; init; }

        public bool? IsReadOnly { get; init; }
    }
}
