namespace ZSharp.IR.VM
{
    public sealed class GetLocal(Local local) 
        : Instruction
        , IHasOperand<Local>
    {
        public Local Local { get; set; } = local;

        Local IHasOperand<Local>.Operand => Local;
    }
}
