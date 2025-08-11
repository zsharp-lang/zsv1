namespace ZSharp.IR.VM
{
    public sealed class GetObject(IRDefinition ir) 
        : Instruction
        , IHasOperand<IRDefinition>
    {
        public IRDefinition IR { get; set; } = ir;

        IRDefinition IHasOperand<IRDefinition>.Operand => IR;
    }
}
