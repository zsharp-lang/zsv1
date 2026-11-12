namespace ZSharp.LIR
{
    /// <summary>
    /// Produces a value of a <see cref="FunctionType"/> referring to a callable.
    /// </summary>
    public sealed class GetFunction(ICallable callable)
        : Expression
    {
        public ICallable Callable { get; set; } = callable;
    }
}
