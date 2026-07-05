using ZSharp.HIR;

namespace Package.ZSharp
{
    public sealed class ImportedName(string name, string? alias = null)
        : Reference
    {
        public string Name { get; set; } = name;

        public string? Alias { get; set; } = alias;
    }
}
