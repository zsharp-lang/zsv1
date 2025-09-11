namespace ZSharp.Runtime.NET.IR2IL.Code
{
    public sealed class Parameter
    {
        public required string Name { get; init; }

        public required int Index { get; init; }

        public required Type Type { get; init; }
    }
}
