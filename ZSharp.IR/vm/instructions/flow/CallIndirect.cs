namespace ZSharp.IR.VM
{
    public sealed class CallIndirect(Signature signature) 
        : Instruction
        , IHasOperand<Signature>
    {
        public Signature Signature { get; set; } = signature;

        Signature IHasOperand<Signature>.Operand => Signature;
    }
}
