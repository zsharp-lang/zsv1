namespace ZSharp.IR.VM
{
    public sealed class GetObject(IRObject ir) 
        : Instruction
        , IHasOperand<IRObject>
    {
        public IRObject IR { get; set; } = ir;

        IRObject IHasOperand<IRObject>.Operand => IR;
    }
}
