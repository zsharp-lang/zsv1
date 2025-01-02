namespace ZSharp.Compiler
{
    public sealed class InvalidCastException(CompilerObject @object, CompilerObject type)
        : CompilerObjectException(@object)
    {
        public CompilerObject Target => Object;

        public CompilerObject Type { get; } = type;
    }
}
