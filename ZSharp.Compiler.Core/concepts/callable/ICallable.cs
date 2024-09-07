namespace ZSharp.Compiler
{
    /// <summary>
    /// Should be implemented by any binding that is callable.
    /// </summary>
    public interface ICTCallable
    {
        public CGObject Call(ZSCompiler compiler, Argument[] arguments);
    }

    /// <summary>
    /// Should be implementedby any type that is callable.
    /// </summary>
    public interface IRTCallable
    {
        public CGObject Call(ZSCompiler compiler, CGObject callable, Argument[] arguments);
    }
}
