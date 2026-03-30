namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public sealed class Closure
        : Node
    {
        public required Function Function { get; init; }

        public List<Capture> Capture { get; init; } = [];
    }
}
