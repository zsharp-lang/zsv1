namespace ZSharp.Compiler
{
    /// <summary>
    /// Should be implemented by any binding that is callable.
    /// </summary>
    public interface ICTCallable
        : CompilerObject
    {
        public CompilerObject Call(Compiler compiler, Argument[] arguments);
    }

    /// <summary>
    /// Should be implementedby any type that is callable.
    /// </summary>
    public interface IRTCallable
        : CompilerObject
    {
        public CompilerObject Call(Compiler compiler, CompilerObject callable, Argument[] arguments);
    }
}
