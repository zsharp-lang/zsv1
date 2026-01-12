namespace ZSharp.SourceCompiler.Module.Objects
{
    internal sealed class EmptyObject
        : CompilerObject
    {
        public static CompilerObject Empty { get; } = new EmptyObject();

        private EmptyObject() { }
    }
}
