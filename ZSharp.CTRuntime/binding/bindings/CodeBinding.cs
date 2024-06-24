namespace ZSharp.CTRuntime
{
    public sealed class CodeBinding(RTType code) : IRTBinding
    {
        public RTType Code { get; } = code;

        public ZSType Type => Code.Type;

        public RTType Read(ZSCompiler compiler)
            => Code;

        public RTType Write(ZSCompiler compiler, IRTBinding value)
        {
            throw new NotImplementedException();
        }
    }
}
