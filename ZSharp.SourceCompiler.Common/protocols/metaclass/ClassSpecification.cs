namespace ZSharp.SourceCompiler.Objects
{
    public sealed class ClassSpecification
    {
        public required string Name { get; init; }

        public required ClassDefinitionProxy Definition { get; init; }

        public required List<CompilerObject> Bases { get; init; } = [];

        public required List<CompilerObject> Content { get; init; } = [];

        public required Dictionary<MemberName, ClassMemberProxy> Members { get; init; } = [];
    }
}
