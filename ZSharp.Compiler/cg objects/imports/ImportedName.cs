using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ImportedName : CompilerObject
    {
        public required string Name { get; set; }

        public string? Alias { get; set; }
    }
}
