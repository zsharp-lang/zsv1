using ZSharp.HIR;

namespace ZSharp.SourceCompiler.HIR
{
    public sealed class SimpleDocument
    {
        public required string Path { get; set; }

        public List<Definition> Definitions { get; } = [];
    }
}
