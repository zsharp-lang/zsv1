namespace ZSharp.IR.VM
{
    public sealed class CallInternal(InternalFunction function) 
        : Instruction
        , IHasOperand<InternalFunction>
    {
        public InternalFunction Function { get; set; } = function;

        InternalFunction IHasOperand<InternalFunction>.Operand => Function;
    }
}
