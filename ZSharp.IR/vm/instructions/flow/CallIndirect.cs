namespace ZSharp.IR.VM
{
    public sealed class CallIndirect(Signature signature) : Instruction
    {
        public Signature Signature { get; set; } = signature;
    }
}
