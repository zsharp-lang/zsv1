namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public sealed class Capture
    {
        public required CaptureKind Kind { get; set; }

        public required IDefinition Location { get; init; }
    }
}
