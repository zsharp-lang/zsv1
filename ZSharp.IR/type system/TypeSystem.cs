namespace ZSharp.IR
{
    public sealed class TypeSystem
    {
        public ClassReference Object { get; init; } = null!;

        public ClassReference String { get; init; } = null!;

        public ClassReference Type { get; init; } = null!;

        public ClassReference Void { get; init; } = null!;

        public ClassReference Null { get; init; } = null!;

        public ClassReference Boolean { get; init; } = null!;

        public ClassReference Int32 { get; init; } = null!;

        public ClassReference Float32 { get; init; } = null!;

        public Class Array { get; init; } = null!;

        public Class Reference { get; init; } = null!;

        public Class Pointer { get; init; } = null!;
    }
}
