namespace ZSharp.SourceCompiler
{
    public sealed class EmptyObject
        : CompilerObject
    {
        public static CompilerObject Empty { get; } = new EmptyObject();

        private EmptyObject() { }
    }
}
