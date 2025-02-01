namespace ZSharp.IR
{
    public sealed class TypeSystem
    {
        public ConstructedClass Object { get; init; } = null!;

        public ConstructedClass String { get; init; } = null!;

        public ConstructedClass Type { get; init; } = null!;

        public ConstructedClass Void { get; init; } = null!;

        public ConstructedClass Null { get; init; } = null!;

        public ConstructedClass Boolean { get; init; } = null!;

        public ConstructedClass Int32 { get; init; } = null!;

        public ConstructedClass Float32 { get; init; } = null!;
    }
}
