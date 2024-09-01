namespace ZSharp.IR.VM
{
    public sealed class SetLocal(Local local) 
        : Instruction
        , IHasOperand<Local>
    {
        public Local Local { get; set; } = local;

        Local IHasOperand<Local>.Operand => Local;
    }
}
