namespace ZSharp.Compiler
{
    public abstract class CompilerObjectException(CompilerObject @object, string? message = null, Exception? innerException = null)
        : Exception(message, innerException)
    {
        public CompilerObject Object { get; } = @object;
    }
}
