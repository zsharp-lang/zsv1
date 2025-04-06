namespace ZSharp.IR.VM
{
    public sealed class CallVirtual(MemberReference<Method> method) 
        : Instruction
        , IHasOperand<MemberReference<Method>>
    {
        public MemberReference<Method> Method { get; set; } = method;

        MemberReference<Method> IHasOperand<MemberReference<Method>>.Operand => Method;
    }
}
