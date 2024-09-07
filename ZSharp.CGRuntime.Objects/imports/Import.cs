using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Import : CGObject
    {
        public required List<Argument> Arguments { get; set; }

        public required List<ImportedName> ImportedNames { get; set; }

        public string? Alias { get; set; }
    }
}
