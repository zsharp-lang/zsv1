namespace ZSharp.IR
{
    public sealed class TypeSystem
    {
        public Class Object { get; init; } = null!;

        public Class String { get; init; } = null!;

        public Class Type { get; init; } = null!;

        public Class Void { get; init; } = null!;

        public Class Null { get; init; } = null!;

        public Class Boolean { get; init; } = null!;

        public Class Int32 { get; init; } = null!;

        public Class Float32 { get; init; } = null!;
    }
}
