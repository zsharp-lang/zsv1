namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class Module
    {
        public required string Name { get; init; }

        public List<IDefinition> Items { get; init; } = [];
    }
}
