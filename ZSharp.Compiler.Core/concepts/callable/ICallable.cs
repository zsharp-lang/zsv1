namespace ZSharp.CTRuntime
{
    /// <summary>
    /// Should be implemented by any binding that is callable.
    /// </summary>
    public interface ICTCallable
    {
        public CTObject Call(ZSCompiler compiler, Argument[] arguments);
    }

    /// <summary>
    /// Should be implementedby any type that is callable.
    /// </summary>
    public interface IRTCallable
    {
        public CTObject Call(ZSCompiler compiler, Code callable, Argument[] arguments);
    }
}
