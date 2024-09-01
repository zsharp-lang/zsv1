namespace ZSharp.IR.VM
{
    public sealed class GetGlobal(Global global) 
        : Instruction
        , IHasOperand<Global>
    {
        public Global Global { get; } = global;

        Global IHasOperand<Global>.Operand => Global;
    }
}
