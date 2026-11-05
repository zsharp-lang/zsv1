namespace ZSharp.LIR
{
    public sealed class Function(Signature signature)
        : ModuleMember
        , ICallable
    {
        public Signature Signature { get; set; } = signature;

        public Code? Body { get; set; }
    }
}
