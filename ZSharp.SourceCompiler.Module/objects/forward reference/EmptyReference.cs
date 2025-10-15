namespace ZSharp.SourceCompiler.Module.Objects
{
    internal sealed class EmptyReference
        : CompilerObject
    {
        public static CompilerObject Instance { get; } = new EmptyReference();
    }
}
