namespace ZSharp.LIR
{
    public sealed class FunctionHandle(Signature signature)
        : Handle
        , ICallable
    {
        public Signature Signature { get; set; } = signature;
    }
}
