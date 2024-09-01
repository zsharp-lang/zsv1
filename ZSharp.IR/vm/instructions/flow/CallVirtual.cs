namespace ZSharp.IR.VM
{
    public sealed class CallVirtual(Method method) 
        : Instruction
        , IHasOperand<Method>
    {
        public Method Method { get; set; } = method;

        Method IHasOperand<Method>.Operand => Method;
    }
}
