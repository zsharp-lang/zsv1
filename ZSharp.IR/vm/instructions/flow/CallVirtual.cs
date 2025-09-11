namespace ZSharp.IR.VM
{
    public sealed class CallVirtual(MethodReference method) 
        : Instruction
        , IHasOperand<MethodReference>
    {
        public MethodReference Method { get; set; } = method;

        MethodReference IHasOperand<MethodReference>.Operand => Method;
    }
}
