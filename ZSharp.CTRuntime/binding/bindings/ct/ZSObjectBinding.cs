namespace ZSharp.CTRuntime
{
    public sealed class ZSObjectBinding(ZSValue @object) : ICTBinding
    {
        public ZSValue Object { get; } = @object;

        public ZSType Type => Object.Type;

        public CTType Read(ZSCompiler compiler) => Object;

        public CTType Write(ZSCompiler compiler, ICTBinding value)
            => throw new("Cannot assign to a literal object");
    }
}
