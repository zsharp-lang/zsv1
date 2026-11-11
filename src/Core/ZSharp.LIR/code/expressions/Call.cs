namespace ZSharp.LIR
{
    public sealed class Call(ICallable callable, params IEnumerable<Expression> arguments)
        : Expression
    {
        public ICallable Callable { get; set; } = callable;

        public List<Expression> Arguments { get; } = [.. arguments];
    }
}
