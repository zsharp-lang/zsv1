namespace ZSharp.Platform.Runtime.Loaders
{
    public struct SourceLocation
    {
        public int StartLine { get; init; }

        public int EndLine { get; init; }

        public int StartColumn { get; init; }

        public int EndColumn { get; init; }
    }
}
