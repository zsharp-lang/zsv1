using ZSharp.HIR;

namespace Package.ZSharp
{
    public sealed class Import(Expression source)
    {
        public Expression Source { get; set; } = source;

        public List<ImportedName> ImportedNames { get; } = [];
    }
}
