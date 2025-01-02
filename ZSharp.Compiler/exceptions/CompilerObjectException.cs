namespace ZSharp.Compiler
{
    public abstract class CompilerObjectException(CompilerObject @object, string? message = null)
        : Exception(message)
    {
        public CompilerObject Object { get; } = @object;
    }
}
